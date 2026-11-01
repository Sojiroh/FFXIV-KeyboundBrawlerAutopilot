using System;
using System.Globalization;
using System.Text;
using Dalamud.Plugin;
using Dalamud.Plugin.Internal.Types.Manifest;
using ECommons.DalamudServices;

namespace AutoKeyboundBrawler.Core;

public static class RunLogReport
{
    private const int MaxSourceWidth = 24;

    private const string DetailIndent = "    ";

    private const string Separator = " | ";

    private static readonly string[] levelNames = new string[5] { "Verbose", "Debug", "Info", "Warning", "Error" };

    public static string WithHeader(ReadOnlySpan<RunLogLine> lines, in RunLogFilter filter, int bufferedCount)
    {
        var builder = new StringBuilder(256 + lines.Length * 112);
        AppendEnvironment(builder);
        AppendScope(builder, lines.Length, in filter, bufferedCount);
        builder.AppendLine();
        AppendLines(builder, lines);
        return builder.ToString();
    }

    public static string Plain(ReadOnlySpan<RunLogLine> lines)
    {
        var builder = new StringBuilder(lines.Length * 112);
        AppendLines(builder, lines);
        return builder.ToString();
    }

    private static void AppendEnvironment(StringBuilder builder)
    {
        IPluginManifest manifest = Svc.PluginInterface.Manifest;
        Version? dalamudVersion = typeof(IDalamudPluginInterface).Assembly.GetName().Version;
        DateTimeOffset timestamp = DateTimeOffset.Now;
        builder.Append(manifest.Name).Append(' ').Append(manifest.AssemblyVersion)
            .Append(Separator)
            .Append("Dalamud ")
            .Append(dalamudVersion)
            .Append(Separator)
            .Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss 'UTC'zzz", CultureInfo.InvariantCulture))
            .Append(Separator)
            .Append("Territory ")
            .Append(Svc.ClientState.TerritoryType)
            .AppendLine();
    }

    private static void AppendScope(StringBuilder builder, int shown, in RunLogFilter filter, int bufferedCount)
    {
        builder.Append(shown).Append(" of ").Append(bufferedCount)
            .Append(" lines");
        if (filter.LevelMask != RunLogFilter.AllLevels)
        {
            builder.Append(Separator).Append("levels: ");
            bool first = true;
            for (int i = 0; i < RunLog.LevelCount; i++)
            {
                if (filter.Shows((RunLogLevel)i))
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(levelNames[i]);
                    first = false;
                }
            }
        }
        if (filter.Search.Length > 0)
        {
            builder.Append(Separator).Append("search: \"").Append(filter.Search)
                .Append('"');
        }
        if (filter.Source != null)
        {
            builder.Append(Separator).Append("source: ").Append(filter.Source);
        }
        builder.AppendLine();
    }

    private static void AppendLines(StringBuilder builder, ReadOnlySpan<RunLogLine> lines)
    {
        int sourceWidth = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            sourceWidth = Math.Max(sourceWidth, lines[i].Source.Length);
        }
        sourceWidth = Math.Min(sourceWidth, MaxSourceWidth);
        for (int j = 0; j < lines.Length; j++)
        {
            AppendLine(builder, in lines[j], sourceWidth);
        }
    }

    private static void AppendLine(StringBuilder builder, in RunLogLine line, int sourceWidth)
    {
        builder.Append(RunLog.Time(line.AtUtc)).Append(' ').Append(RunLog.Tag(line.Level))
            .Append(' ')
            .Append(line.Source.PadRight(sourceWidth))
            .Append("  ");
        AppendPrintable(builder, line.Message);
        if (line.Repeat > 1)
        {
            builder.Append(" (x").Append(line.Repeat).Append(')');
        }
        builder.AppendLine();
        if (line.Detail == null)
        {
            return;
        }
        foreach (ReadOnlySpan<char> current in line.Detail.EnumerateLines())
        {
            builder.Append(DetailIndent);
            AppendPrintable(builder, current);
            builder.AppendLine();
        }
    }

    private static void AppendPrintable(StringBuilder builder, ReadOnlySpan<char> text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            builder.Append((char.IsControl(character) && character != '\t') ? '?' : character);
        }
    }
}
