using System;
using AutoKeyboundBrawler.Core.Game;

namespace AutoKeyboundBrawler.Core.Typing;

internal sealed class BarrierMemory
{
    private const int Capacity = 16;

    private const int SpotCapacity = 8;

    private readonly string[] phrases = new string[Capacity];

    private readonly bool[] onBarrier = new bool[Capacity];

    private readonly long[] firstSeenAt = new long[Capacity];

    private readonly float[] spotX = new float[SpotCapacity];

    private readonly float[] spotY = new float[SpotCapacity];

    private readonly long[] spotSeenAt = new long[SpotCapacity];

    private int count;

    private int spotCount;

    public void Apply(BattleSnapshot snapshot, long now)
    {
        ForgetPhrasesOffScreen(snapshot);
        ForgetStaleSpots(now);
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            if (snapshot.IsLinked(i))
            {
                continue;
            }
            string phrase = snapshot.Phrases[i];
            bool atSpot = AtSpot(snapshot, i);
            int knownIndex = IndexOf(phrase);
            if (knownIndex < 0)
            {
                Remember(phrase, snapshot.PhraseOnBarrier[i] | atSpot, now);
                LogSpotMatch(phrase, atSpot && !snapshot.PhraseOnBarrier[i]);
                snapshot.PhraseOnBarrier[i] |= atSpot;
                continue;
            }
            bool freshOnBarrier = snapshot.PhraseOnBarrier[i] && now - firstSeenAt[knownIndex] < AkbConstants.BarrierSettleMs;
            if (!onBarrier[knownIndex] && (freshOnBarrier | atSpot))
            {
                onBarrier[knownIndex] = true;
                LogSpotMatch(phrase, atSpot && !freshOnBarrier);
            }
            snapshot.PhraseOnBarrier[i] = onBarrier[knownIndex];
        }
        for (int j = 0; j < snapshot.PhraseCount; j++)
        {
            if (snapshot.PhraseOnBarrier[j])
            {
                MarkSpot(snapshot.PhraseScreenX[j], snapshot.PhraseScreenY[j], snapshot.PhraseScale[j], now);
            }
        }
    }

    public void Clear()
    {
        Array.Clear(phrases);
        Array.Clear(onBarrier);
        Array.Clear(firstSeenAt);
        count = 0;
        spotCount = 0;
    }

    private static void LogSpotMatch(string phrase, bool matched)
    {
        if (matched)
        {
            RunLog.Debug("\"" + phrase + "\" sits where a barrier phrase just was, so it is typed as one.");
        }
    }

    private bool AtSpot(BattleSnapshot snapshot, int index)
    {
        if (!KeyboundUi.InBarrierZone(snapshot, index))
        {
            return false;
        }
        float scale = snapshot.PhraseScale[index];
        for (int i = 0; i < spotCount; i++)
        {
            if (MathF.Abs(snapshot.PhraseScreenX[index] - spotX[i]) <= AkbConstants.BarrierSpotReachX * scale && MathF.Abs(snapshot.PhraseScreenY[index] - spotY[i]) <= AkbConstants.BarrierSpotReachY * scale)
            {
                return true;
            }
        }
        return false;
    }

    private void MarkSpot(float x, float y, float scale, long now)
    {
        int oldest = 0;
        for (int i = 0; i < spotCount; i++)
        {
            if (MathF.Abs(x - spotX[i]) <= AkbConstants.BarrierSpotReachX * scale && MathF.Abs(y - spotY[i]) <= AkbConstants.BarrierSpotReachY * scale)
            {
                spotX[i] = x;
                spotY[i] = y;
                spotSeenAt[i] = now;
                return;
            }
            if (spotSeenAt[i] < spotSeenAt[oldest])
            {
                oldest = i;
            }
        }
        int slot = ((spotCount < SpotCapacity) ? spotCount++ : oldest);
        spotX[slot] = x;
        spotY[slot] = y;
        spotSeenAt[slot] = now;
    }

    private void ForgetStaleSpots(long now)
    {
        for (int spot = spotCount - 1; spot >= 0; spot--)
        {
            if (now - spotSeenAt[spot] > AkbConstants.BarrierSpotMemoryMs)
            {
                spotCount--;
                spotX[spot] = spotX[spotCount];
                spotY[spot] = spotY[spotCount];
                spotSeenAt[spot] = spotSeenAt[spotCount];
            }
        }
    }

    private void ForgetPhrasesOffScreen(BattleSnapshot snapshot)
    {
        for (int slot = count - 1; slot >= 0; slot--)
        {
            if (!IsOnScreen(snapshot, phrases[slot]))
            {
                count--;
                phrases[slot] = phrases[count];
                onBarrier[slot] = onBarrier[count];
                firstSeenAt[slot] = firstSeenAt[count];
                phrases[count] = string.Empty;
                onBarrier[count] = false;
            }
        }
    }

    private void Remember(string phrase, bool barrier, long now)
    {
        if (count == Capacity)
        {
            return;
        }
        phrases[count] = phrase;
        onBarrier[count] = barrier;
        firstSeenAt[count] = now;
        count++;
    }

    private static bool IsOnScreen(BattleSnapshot snapshot, string phrase)
    {
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            if (string.Equals(snapshot.Phrases[i], phrase, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private int IndexOf(string phrase)
    {
        for (int i = 0; i < count; i++)
        {
            if (string.Equals(phrases[i], phrase, StringComparison.Ordinal))
            {
                return i;
            }
        }
        return -1;
    }
}
