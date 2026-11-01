using System;
using System.Globalization;
using AutoKeyboundBrawler.Core.Game;
using AutoKeyboundBrawler.Core.Typing;
using ECommons.DalamudServices;

namespace AutoKeyboundBrawler.Core.Queue;

internal sealed class RewardTracker
{
    private const int SettleMs = 10000;

    private const int AfterArrivalMs = 3000;

    private const int PollSpacingMs = 250;

    private readonly Configuration configuration;

    private readonly TypingSession session;

    private bool matchSeen;

    private bool resultSeen;

    private uint matchTerritory;

    private int mgcBefore;

    private int mgpBefore;

    private int mgcGained;

    private int mgpGained;

    private long settleUntil;

    private long nextPollAt;

    public bool Settling => resultSeen;

    public RewardTracker(Configuration configuration, TypingSession session)
    {
        this.configuration = configuration;
        this.session = session;
    }

    public void Reset()
    {
        matchSeen = false;
        resultSeen = false;
    }

    public void Update(BattleSnapshot snapshot, long now)
    {
        if (resultSeen)
        {
            Settle(now);
            return;
        }
        if (!matchSeen)
        {
            if (snapshot.InputVisible)
            {
                BeginMatch();
            }
            return;
        }
        if (QueueWindows.ResultsOpen)
        {
            EndMatch(now);
            return;
        }
        if (!snapshot.InputVisible && !QueueConditions.ChangingAreas && Svc.ClientState.TerritoryType != matchTerritory)
        {
            RunLog.Info("Left the match before its results; its rewards are not counted.");
            Reset();
        }
    }

    public void Finish()
    {
        if (!resultSeen)
        {
            Reset();
            return;
        }
        Poll();
        session.CountMatch(mgcGained, mgpGained);
        configuration.LifetimeMatches++;
        configuration.LifetimeMgc += mgcGained;
        configuration.LifetimeMgp += mgpGained;
        configuration.SaveDebounced();
        RunLog.Info(string.Create(CultureInfo.InvariantCulture, $"Match done: +{mgcGained:N0} MGC, +{mgpGained:N0} MGP ({session.MatchesCompleted} this session, {session.MgcEarned:N0} MGC and {session.MgpEarned:N0} MGP in all)."));
        Reset();
    }

    private void BeginMatch()
    {
        matchSeen = true;
        matchTerritory = Svc.ClientState.TerritoryType;
        mgcBefore = Wallet.Mgc;
        mgpBefore = Wallet.Mgp;
        RunLog.Debug(string.Create(CultureInfo.InvariantCulture, $"Match started holding {mgcBefore:N0} MGC and {mgpBefore:N0} MGP."));
    }

    private void EndMatch(long now)
    {
        resultSeen = true;
        mgcGained = 0;
        mgpGained = 0;
        settleUntil = now + SettleMs;
        nextPollAt = now;
        RunLog.Debug("Results are up; watching the wallet for the rewards.");
    }

    private void Settle(long now)
    {
        if (QueueConditions.ChangingAreas)
        {
            settleUntil = Math.Max(settleUntil, now + AfterArrivalMs);
        }
        if (now >= nextPollAt)
        {
            nextPollAt = now + PollSpacingMs;
            Poll();
        }
        if (now >= settleUntil)
        {
            Finish();
        }
    }

    private void Poll()
    {
        mgcGained = Math.Max(mgcGained, Gained(mgcBefore, Wallet.Mgc));
        mgpGained = Math.Max(mgpGained, Gained(mgpBefore, Wallet.Mgp));
    }

    private static int Gained(int before, int after)
    {
        if (before != -1 && after != -1)
        {
            return Math.Max(0, after - before);
        }
        return 0;
    }
}
