using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AutoKeyboundBrawler.Core.Localization;

internal sealed class StringCatalog
{
    private const int FirstNonAsciiCodepoint = 128;

    public static readonly StringCatalog Empty = new StringCatalog(new Dictionary<string, string>(0, StringComparer.Ordinal));

    private readonly Dictionary<string, string> entries;

    public int Count => entries.Count;

    private StringCatalog(Dictionary<string, string> entries)
    {
        this.entries = entries;
    }

    public bool TryGet(string key, out string value)
    {
        return entries.TryGetValue(key, out value!);
    }

    public bool Contains(string key)
    {
        return entries.ContainsKey(key);
    }

    public static StringCatalog Load(string path)
    {
        if (!File.Exists(path))
        {
            return Empty;
        }
        var target = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            Flatten(JObject.Parse(File.ReadAllText(path)), null, target);
        }
        catch (Exception exception)
        {
            RunLog.Error(exception, "Failed to load language catalog '" + path + "'");
            return Empty;
        }
        return new StringCatalog(target);
    }

    public static ushort[] ScanGlyphRanges(string path)
    {
        if (!File.Exists(path))
        {
            return new ushort[1];
        }
        string content;
        try
        {
            content = File.ReadAllText(path);
        }
        catch (Exception exception)
        {
            RunLog.Error(exception, "Failed to scan glyphs from '" + path + "'");
            return new ushort[1];
        }
        bool[] present = new bool[GlyphRanges.CodepointCount];
        for (int i = 0; i < content.Length; i++)
        {
            var character = content[i];
            if (character >= FirstNonAsciiCodepoint && !char.IsSurrogate(character))
            {
                present[(uint)character] = true;
            }
        }
        var ranges = new List<ushort>();
        int runStart = -1;
        for (int j = FirstNonAsciiCodepoint; j <= ushort.MaxValue; j++)
        {
            if (present[j])
            {
                if (runStart < 0)
                {
                    runStart = j;
                }
            }
            else if (runStart >= 0)
            {
                ranges.Add((ushort)runStart);
                ranges.Add((ushort)(j - 1));
                runStart = -1;
            }
        }
        if (runStart >= 0)
        {
            ranges.Add((ushort)runStart);
            ranges.Add(ushort.MaxValue);
        }
        ranges.Add(0);
        return ranges.ToArray();
    }

    private static void Flatten(JObject node, string? prefix, Dictionary<string, string> target)
    {
        foreach (JProperty property in node.Properties())
        {
            string key = ((prefix == null) ? property.Name : (prefix + "." + property.Name));
            if (property.Value is JObject node2)
            {
                Flatten(node2, key, target);
            }
            else
            {
                target[key] = property.Value.ToString();
            }
        }
    }
}
