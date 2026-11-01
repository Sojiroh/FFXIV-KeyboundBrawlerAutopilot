using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using ECommons.DalamudServices;

namespace AutoKeyboundBrawler.Core;

public enum RunLogLevel : byte
{
    Verbose,
    Debug,
    Info,
    Warning,
    Error
}

public readonly record struct RunLogLine(long Sequence, DateTime AtUtc, RunLogLevel Level, string Source, string Message, string? Detail, int Repeat);

public static class RunLog
{
    public const int Capacity = 3000;

    public const int LevelCount = 5;

    private const string TimeFormat = "HH:mm:ss.fff";

    private static readonly object gate = new object();

    private static readonly RunLogLine[] lines = new RunLogLine[Capacity];

    private static readonly Dictionary<string, string> sourceNames = new Dictionary<string, string>(StringComparer.Ordinal);

    private static int start;

    private static int count;

    private static int version;

    private static long nextSequence = 1L;

    private static long latestWarningSequence;

    private static long latestErrorSequence;

    private static long seenSequence;

    public static int Version => Volatile.Read(in version);

    public static int Count
    {
        get
        {
            lock (gate)
            {
                return count;
            }
        }
    }

    public static RunLogLevel? Unseen
    {
        get
        {
            lock (gate)
            {
                if (latestErrorSequence > seenSequence)
                {
                    return RunLogLevel.Error;
                }
                return (latestWarningSequence > seenSequence) ? new RunLogLevel?(RunLogLevel.Warning) : ((RunLogLevel?)null);
            }
        }
    }

    public static void Verbose(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Verbose($"{AkbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Verbose, message, null, callerFile);
    }

    public static void Debug(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Debug($"{AkbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Debug, message, null, callerFile);
    }

    public static void Info(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Info($"{AkbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Info, message, null, callerFile);
    }

    public static void Info(string message, string detail, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Info($"{AkbConstants.LogPrefix} {message}{Environment.NewLine}{detail}");
        Push(RunLogLevel.Info, message, detail, callerFile);
    }

    public static void Warning(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Warning($"{AkbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Warning, message, null, callerFile);
    }

    public static void Warning(Exception exception, string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Warning(exception, $"{AkbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Warning, message, exception.ToString(), callerFile);
    }

    public static void Error(string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Error($"{AkbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Error, message, null, callerFile);
    }

    public static void Error(Exception exception, string message, [CallerFilePath] string callerFile = "")
    {
        Svc.Log.Error(exception, $"{AkbConstants.LogPrefix} {message}");
        Push(RunLogLevel.Error, message, exception.ToString(), callerFile);
    }

    public static void Record(RunLogLevel level, string message, [CallerFilePath] string callerFile = "")
    {
        Push(level, message, null, callerFile);
    }

    public static void Clear()
    {
        lock (gate)
        {
            start = 0;
            count = 0;
            latestWarningSequence = 0L;
            latestErrorSequence = 0L;
            version++;
        }
    }

    public static void MarkSeen()
    {
        lock (gate)
        {
            seenSequence = nextSequence - 1;
        }
    }

    public static int Snapshot(in RunLogFilter filter, RunLogLine[] destination, Span<int> levelTotals)
    {
        levelTotals.Clear();
        lock (gate)
        {
            int copied = 0;
            for (int i = 0; i < count; i++)
            {
                RunLogLine line = lines[(start + i) % Capacity];
                levelTotals[(int)line.Level]++;
                if (filter.Matches(in line))
                {
                    destination[copied++] = line;
                }
            }
            return copied;
        }
    }

    public static string Time(DateTime atUtc)
    {
        return atUtc.ToLocalTime().ToString(TimeFormat, CultureInfo.InvariantCulture);
    }

    public static string Tag(RunLogLevel level)
    {
        return level switch
        {
            RunLogLevel.Verbose => "VERB",
            RunLogLevel.Debug => "DBUG",
            RunLogLevel.Warning => "WARN",
            RunLogLevel.Error => "ERR ",
            _ => "INFO",
        };
    }

    private static void Push(RunLogLevel level, string message, string? detail, string callerFile)
    {
        lock (gate)
        {
            DateTime utcNow = DateTime.UtcNow;
            string source = SourceName(callerFile);
            long sequence = nextSequence++;
            if (level == RunLogLevel.Warning)
            {
                latestWarningSequence = sequence;
            }
            else if (level == RunLogLevel.Error)
            {
                latestErrorSequence = sequence;
            }
            if (count > 0)
            {
                int lastIndex = (start + count - 1) % Capacity;
                RunLogLine previous = lines[lastIndex];
                if (previous.Level == level && (object)previous.Source == source && string.Equals(previous.Message, message, StringComparison.Ordinal) && string.Equals(previous.Detail, detail, StringComparison.Ordinal))
                {
                    lines[lastIndex] = previous with
                    {
                        AtUtc = utcNow,
                        Repeat = previous.Repeat + 1
                    };
                    version++;
                    return;
                }
            }
            int writeIndex = (start + count) % Capacity;
            lines[writeIndex] = new RunLogLine(sequence, utcNow, level, source, message, detail, 1);
            if (count < Capacity)
            {
                count++;
            }
            else
            {
                start = (start + 1) % Capacity;
            }
            version++;
        }
    }

    private static string SourceName(string callerFile)
    {
        if (sourceNames.TryGetValue(callerFile, out string? value))
        {
            return value;
        }
        value = Path.GetFileNameWithoutExtension(callerFile);
        int dot = value.IndexOf('.');
        if (dot > 0)
        {
            value = value.Substring(0, dot);
        }
        sourceNames[callerFile] = value;
        return value;
    }
}
