using System;
using System.Collections.Generic;
using System.Text;
using Dalamud.Game;
using ECommons.DalamudServices;
using Lumina.Excel;

namespace AutoKeyboundBrawler.Core.Game;

internal static class PhraseReadings
{
    private const string SheetName = "KTGTypeWordTextData";

    private const int ShownColumn = 0;

    private const int ReadingColumn = 1;

    private const string OptionalPunctuation = "?!-'’‘！？‐‑‒–";

    private static bool? japanese;

    private static Dictionary<string, string>? readings;

    private static readonly Dictionary<string, string> typedForms = new Dictionary<string, string>(StringComparer.Ordinal);

    private static readonly Dictionary<string, string> comparableForms = new Dictionary<string, string>(StringComparer.Ordinal);

    public static bool ClientIsJapanese
    {
        get => japanese ??= Svc.Data.Language == ClientLanguage.Japanese;
    }

    public static string TypedForm(string shown)
    {
        if (typedForms.TryGetValue(shown, out string? value))
        {
            return value;
        }
        value = Comparable(ClientIsJapanese ? Reading(shown) : shown);
        typedForms[shown] = value;
        return value;
    }

    public static string Comparable(string text)
    {
        if (comparableForms.TryGetValue(text, out string? value))
        {
            return value;
        }
        value = WithoutOptionalPunctuation(text);
        comparableForms[text] = value;
        return value;
    }

    private static string Reading(string shown)
    {
        return (readings ??= LoadReadings()).TryGetValue(shown, out string? value) ? value : shown;
    }

    private static string WithoutOptionalPunctuation(string text)
    {
        if (text.IndexOfAny<char>(OptionalPunctuation) < 0)
        {
            return text;
        }
        var builder = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            if (OptionalPunctuation.IndexOf(text[i]) < 0)
            {
                builder.Append(text[i]);
            }
        }
        var trimmed = builder.ToString().Trim();
        if (trimmed.Length != 0)
        {
            return trimmed;
        }
        return text;
    }

    private static Dictionary<string, string> LoadReadings()
    {
        var dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            var sheet = Svc.Data.Excel.GetSheet<RawRow>(null, SheetName);
            for (int i = 0; i < sheet.Count; i++)
            {
                var row = sheet.GetRowAt(i);
                var shown = row.ReadStringColumn(ShownColumn).ExtractText().Trim();
                var reading = row.ReadStringColumn(ReadingColumn).ExtractText().Trim();
                if (shown.Length != 0 && reading.Length != 0)
                {
                    dictionary.TryAdd(shown, reading);
                    dictionary.TryAdd(reading, reading);
                }
            }
        }
        catch (Exception exception)
        {
            RunLog.Warning(exception, "could not read the KTGTypeWordTextData sheet; phrases are typed as shown");
        }
        return dictionary;
    }
}
