using System;
using AutoKeyboundBrawler.Core.Localization;

namespace AutoKeyboundBrawler.Windows;

internal static class Formatting
{
    public static string Clock(long milliseconds)
    {
        var totalSeconds = (int)(milliseconds / 1000);
        return string.Format(Loc.Culture, "{0:D2}:{1:D2}", totalSeconds / 60, totalSeconds % 60);
    }

    public static string Elapsed(TimeSpan span)
    {
        if (span.TotalHours >= 1.0)
        {
            return string.Format(Loc.Culture, "{0}h {1:D2}m", (int)span.TotalHours, span.Minutes);
        }
        return string.Format(Loc.Culture, "{0}m {1:D2}s", span.Minutes, span.Seconds);
    }

    public static string Phrases(int count)
    {
        return Loc.Plural(L.Common.PhrasesCount, count);
    }

    public static string Number(int value)
    {
        return value.ToString(Loc.Culture);
    }

    public static string Amount(long value)
    {
        return value.ToString("N0", Loc.Culture);
    }
}
