using System;

namespace AutoKeyboundBrawler.Core.Typing;

public enum TypingSpeed : byte
{
    Relaxed,
    Steady,
    Fast,
    Blazing
}

internal readonly record struct TypingPace(int KeyDelayMs, int KeyJitterMs, int SubmitDelayMs);

internal static class TypingPaces
{
    private static readonly TypingPace[] paces = new TypingPace[4]
    {
        new TypingPace(170, 60, 260),
        new TypingPace(105, 40, 180),
        new TypingPace(65, 20, 110),
        new TypingPace(20, 6, 45)
    };

    public static readonly TypingSpeed[] All = new TypingSpeed[4]
    {
        TypingSpeed.Relaxed,
        TypingSpeed.Steady,
        TypingSpeed.Fast,
        TypingSpeed.Blazing
    };

    public static TypingPace Of(TypingSpeed speed)
    {
        return paces[Math.Clamp((int)speed, 0, paces.Length - 1)];
    }
}
