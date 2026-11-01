using System.Collections.Generic;

namespace AutoKeyboundBrawler.Core.Localization;

internal static class GlyphRanges
{
    public const int CodepointCount = 65536;

    public const int FirstNonAsciiCodepoint = 128;

    public static bool MarkText(bool[] present, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }
        bool changed = false;
        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            if (character >= FirstNonAsciiCodepoint && !char.IsSurrogate(character) && !present[(uint)character])
            {
                present[(uint)character] = true;
                changed = true;
            }
        }
        return changed;
    }

    public static void MarkRanges(bool[] present, ushort[]? ranges)
    {
        if (ranges == null)
        {
            return;
        }
        for (int i = 0; i + 1 < ranges.Length; i += 2)
        {
            if (ranges[i] == 0)
            {
                return;
            }
            for (int j = ranges[i]; j <= ranges[i + 1]; j++)
            {
                present[j] = true;
            }
        }
    }

    public static ushort[] ToRanges(bool[] present)
    {
        var ranges = new List<ushort>();
        int runStart = -1;
        for (int i = 1; i < CodepointCount; i++)
        {
            if (present[i])
            {
                if (runStart < 0)
                {
                    runStart = i;
                }
            }
            else if (runStart >= 0)
            {
                ranges.Add((ushort)runStart);
                ranges.Add((ushort)(i - 1));
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
}
