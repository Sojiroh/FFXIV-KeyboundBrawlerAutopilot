using System;

namespace AutoKeyboundBrawler.Core;

public readonly record struct RunLogFilter(int LevelMask, string Search, string? Source)
{
    public bool IsNarrowed => LevelMask != AllLevels || Search.Length > 0 || Source is not null;

    public const int AllLevels = 31;

    public static readonly RunLogFilter Everything = new RunLogFilter(AllLevels, string.Empty, null);

    public bool Shows(RunLogLevel level)
    {
        return (LevelMask & (1 << (int)level)) != 0;
    }

    public bool Matches(in RunLogLine line)
    {
        if (!Shows(line.Level))
        {
            return false;
        }
        if (Source != null && !string.Equals(line.Source, Source, StringComparison.Ordinal))
        {
            return false;
        }
        if (Search.Length == 0)
        {
            return true;
        }
        return line.Message.Contains(Search, StringComparison.OrdinalIgnoreCase) || line.Source.Contains(Search, StringComparison.OrdinalIgnoreCase) || (line.Detail != null && line.Detail.Contains(Search, StringComparison.OrdinalIgnoreCase));
    }
}
