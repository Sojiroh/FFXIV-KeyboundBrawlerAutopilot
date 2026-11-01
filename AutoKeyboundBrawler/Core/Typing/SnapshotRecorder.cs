using System;
using AutoKeyboundBrawler.Core.Game;

namespace AutoKeyboundBrawler.Core.Typing;

internal sealed class SnapshotRecorder
{
    private const int MaxSpells = 32;

    private readonly string[] capturedSpells = new string[MaxSpells];

    private int capturedSpellCount;

    private string? pendingSpell;

    private long pendingAt;

    private long lastWrittenAt;

    private int written;

    private bool barrierShown;

    private bool holdShown;

    public void Begin()
    {
        Array.Clear(capturedSpells);
        capturedSpellCount = 0;
        pendingSpell = null;
        lastWrittenAt = 0L;
        written = 0;
        barrierShown = false;
        holdShown = false;
    }

    public void Observe(Configuration configuration, BattleSnapshot snapshot, bool holding, long now)
    {
        bool barrierPresent = AnyBarrierPhrase(snapshot);
        if (configuration.RecordSnapshots)
        {
            ObserveSpells(snapshot, now);
            if (holding && !holdShown)
            {
                Capture("a cast completed", now);
            }
            else if (barrierPresent && !barrierShown)
            {
                Capture("a barrier phrase appeared", now);
            }
        }
        barrierShown = barrierPresent;
        holdShown = holding;
    }

    private void ObserveSpells(BattleSnapshot snapshot, long now)
    {
        if (pendingSpell != null)
        {
            if (now >= pendingAt && Capture("first " + pendingSpell + " cast", now))
            {
                pendingSpell = null;
            }
            return;
        }
        for (int i = 0; i < snapshot.CastCount; i++)
        {
            string spellName = snapshot.CastNames[i];
            if (spellName.Length != 0 && !WasCaptured(spellName) && capturedSpellCount != MaxSpells)
            {
                capturedSpells[capturedSpellCount++] = spellName;
                pendingSpell = spellName;
                pendingAt = now + AkbConstants.SnapshotCastDelayMs;
                break;
            }
        }
    }

    private bool Capture(string reason, long now)
    {
        if (written >= AkbConstants.MaxSnapshotsPerSession || now - lastWrittenAt < AkbConstants.SnapshotSpacingMs)
        {
            return false;
        }
        written++;
        lastWrittenAt = now;
        UiSnapshot.Write(reason);
        return true;
    }

    private bool WasCaptured(string spell)
    {
        for (int i = 0; i < capturedSpellCount; i++)
        {
            if (string.Equals(capturedSpells[i], spell, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private static bool AnyBarrierPhrase(BattleSnapshot snapshot)
    {
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            if (snapshot.PhraseOnBarrier[i])
            {
                return true;
            }
        }
        return false;
    }
}
