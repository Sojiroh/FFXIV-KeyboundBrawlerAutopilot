using System;

namespace AutoKeyboundBrawler.Core.Typing;

public readonly record struct DelayRange(float MinSeconds, float MaxSeconds);

internal static class Humanizer
{
    public const float SecondsMin = 0f;

    public const float SecondsMax = 3f;

    public const float SecondsStep = 0.05f;

    private const float MillisecondsPerSecond = 1000f;

    private const double TypoChance = 0.03;

    private static readonly string[] keyboardRows = new string[3] { "qwertyuiop", "asdfghjkl", "zxcvbnm" };

    public static int DrawDelayMs(Configuration configuration, HumanAction action)
    {
        if (!configuration.HumanizerEnabled)
        {
            return 0;
        }
        DelayRange delayRange = configuration.DelayFor(action);
        float minSeconds = Math.Clamp(delayRange.MinSeconds, SecondsMin, SecondsMax);
        float maxSeconds = Math.Max(minSeconds, Math.Clamp(delayRange.MaxSeconds, SecondsMin, SecondsMax));
        return (int)((minSeconds + (maxSeconds - minSeconds) * Random.Shared.NextSingle()) * MillisecondsPerSecond);
    }

    public static char RollTypo(Configuration configuration, char intended)
    {
        if (!configuration.HumanizerEnabled || !configuration.HumanizerTypos || Random.Shared.NextDouble() >= TypoChance)
        {
            return '\0';
        }
        char lowered = char.ToLowerInvariant(intended);
        for (int i = 0; i < keyboardRows.Length; i++)
        {
            string row = keyboardRows[i];
            int column = row.IndexOf(lowered);
            if (column >= 0)
            {
                int index = NeighbourColumn(column, row.Length);
                if (!char.IsUpper(intended))
                {
                    return row[index];
                }
                return char.ToUpperInvariant(row[index]);
            }
        }
        return '\0';
    }

    private static int NeighbourColumn(int column, int rowLength)
    {
        if (column == 0)
        {
            return 1;
        }
        if (column == rowLength - 1)
        {
            return column - 1;
        }
        if (Random.Shared.Next(2) != 0)
        {
            return column + 1;
        }
        return column - 1;
    }
}
