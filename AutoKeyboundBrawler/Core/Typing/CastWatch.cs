using System;
using AutoKeyboundBrawler.Core.Game;

namespace AutoKeyboundBrawler.Core.Typing;

internal sealed class CastWatch
{
    private const nint Free = 0;

    private const float MillisecondsPerSecond = 1000f;

    private readonly nint[] castIds = new nint[8];

    private readonly string[] spells = new string[8];

    private readonly bool[] sawFilling = new bool[8];

    private readonly bool[] completed = new bool[8];

    private readonly bool[] seenThisFrame = new bool[8];

    private long holdFrom;

    private long holdUntil;

    private string holdSpell = string.Empty;

    private long entityLetterlockSeenAt;

    private bool heldByStatus;

    public bool Holding { get; private set; }

    public string HoldingSpell { get; private set; } = string.Empty;

    public string LastCompletedSpell { get; private set; } = string.Empty;

    public long LastCompletedAt { get; private set; }

    public float HoldRemaining(long now)
    {
        long holdLength = holdUntil - holdFrom;
        if (holdLength > 0)
        {
            return Math.Clamp((float)(holdUntil - now) / (float)holdLength, 0f, 1f);
        }
        return 0f;
    }

    public void Update(BattleSnapshot snapshot, BrawlerAgent agent, long now)
    {
        TrackEntityCasts(agent, now);
        Array.Clear(seenThisFrame);
        for (int i = 0; i < snapshot.CastCount; i++)
        {
            int slot = SlotFor(snapshot, i);
            if (slot >= 0)
            {
                seenThisFrame[slot] = true;
                if (!snapshot.CastInterrupted[i])
                {
                    Track(slot, snapshot.CastProgress[i], now);
                }
            }
        }
        for (int j = 0; j < castIds.Length; j++)
        {
            if (!seenThisFrame[j])
            {
                Release(j);
            }
        }
        TrackWatchStatus(now);
        Holding = now < holdUntil;
        HoldingSpell = (Holding ? holdSpell : string.Empty);
    }

    public void Reset()
    {
        for (int i = 0; i < castIds.Length; i++)
        {
            Release(i);
        }
        holdFrom = 0L;
        holdUntil = 0L;
        holdSpell = string.Empty;
        entityLetterlockSeenAt = 0L;
        heldByStatus = false;
        Holding = false;
        HoldingSpell = string.Empty;
    }

    private void TrackEntityCasts(BrawlerAgent agent, long now)
    {
        for (int i = 0; i < agent.Count; i++)
        {
            uint actionId = agent.CastActionIds[i];
            if (!LetterlockSpells.IsLetterlock(actionId))
            {
                continue;
            }
            entityLetterlockSeenAt = now;
            long remainingMs = (long)(agent.CastRemainingSeconds[i] * MillisecondsPerSecond);
            if (remainingMs <= AkbConstants.LetterlockLeadMs)
            {
                string spellName = LetterlockSpells.NameOf(actionId);
                if (now >= holdUntil)
                {
                    RunLog.Debug($"Holding keys for {((spellName.Length > 0) ? spellName : "a Letterlock cast")}, {remainingMs} ms before it completes.");
                }
                HoldFor(spellName, now, now + remainingMs + AkbConstants.LetterlockAfterMs);
            }
        }
    }

    private void TrackWatchStatus(long now)
    {
        if (now >= holdUntil)
        {
            heldByStatus = false;
        }
        if (!LetterlockSpells.PlayerWatched())
        {
            return;
        }
        long holdTarget = now + AkbConstants.LetterlockStatusMarginMs;
        if (holdTarget <= holdUntil)
        {
            return;
        }
        if (!heldByStatus)
        {
            heldByStatus = true;
            RunLog.Debug("Holding keys longer: the Letterlock status is still on you.");
        }
        HoldFor((now < holdUntil) ? holdSpell : string.Empty, now, holdTarget);
    }

    private bool TimedByEntity(long now) => entityLetterlockSeenAt != 0L && now - entityLetterlockSeenAt <= AkbConstants.LetterlockAfterMs;

    private void Track(int slot, float progress, long now)
    {
        string spell = spells[slot];
        bool isLetterlock = LetterlockSpells.Matches(spell);
        if (progress < AkbConstants.CastFilling)
        {
            sawFilling[slot] = true;
        }
        bool finishedWithoutFilling = isLetterlock && !sawFilling[slot];
        float threshold = (isLetterlock ? AkbConstants.LetterlockFull : AkbConstants.CastFull);
        if (!finishedWithoutFilling && (progress < threshold || !sawFilling[slot]))
        {
            return;
        }
        bool timedByEntity = isLetterlock && TimedByEntity(now);
        int holdMs = (isLetterlock ? AkbConstants.LetterlockHoldMs : AkbConstants.CastHoldMs);
        if (!timedByEntity && (finishedWithoutFilling || !completed[slot]))
        {
            HoldFor(spell, now, now + holdMs);
        }
        if (completed[slot])
        {
            return;
        }
        completed[slot] = true;
        LastCompletedSpell = spell;
        LastCompletedAt = now;
        if (!timedByEntity)
        {
            RunLog.Debug("Holding keys for " + ((spell.Length > 0) ? spell : "an unnamed cast") + ".");
        }
    }

    private void HoldFor(string spell, long now, long until)
    {
        if (until <= holdUntil)
        {
            return;
        }
        if (now >= holdUntil)
        {
            holdFrom = now;
        }
        holdUntil = until;
        holdSpell = spell;
    }

    private int SlotFor(BattleSnapshot snapshot, int cast)
    {
        nint castId = snapshot.CastIds[cast];
        string castName = snapshot.CastNames[cast];
        int freeSlot = -1;
        for (int i = 0; i < castIds.Length; i++)
        {
            if (castIds[i] == castId)
            {
                if (!string.Equals(spells[i], castName, StringComparison.Ordinal))
                {
                    Claim(i, castId, snapshot, cast);
                }
                return i;
            }
            if (freeSlot < 0 && castIds[i] == Free)
            {
                freeSlot = i;
            }
        }
        if (freeSlot >= 0)
        {
            Claim(freeSlot, castId, snapshot, cast);
        }
        return freeSlot;
    }

    private void Claim(int slot, nint id, BattleSnapshot snapshot, int cast)
    {
        castIds[slot] = id;
        spells[slot] = snapshot.CastNames[cast];
        sawFilling[slot] = false;
        completed[slot] = false;
        var spellName = spells[slot].Length > 0 ? spells[slot] : "an unnamed spell";
        var hits = snapshot.CastHits[cast];
        var suffix = LetterlockSpells.Matches(spells[slot])
            ? " (Letterlock: keys stop when it completes)"
            : hits > 0 ? $" (interrupt with {hits} hits)" : string.Empty;
        RunLog.Info("Enemy casting " + spellName + suffix + ".");
    }

    private void Release(int slot)
    {
        castIds[slot] = Free;
        spells[slot] = string.Empty;
        sawFilling[slot] = false;
        completed[slot] = false;
    }
}
