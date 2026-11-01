using System;
using System.Globalization;
using AutoKeyboundBrawler.Core.Game;
using AutoKeyboundBrawler.Core.Ipc;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace AutoKeyboundBrawler.Core.Queue;

public enum QueueStep : byte
{
    Off,
    InMatch,
    ClosingResults,
    ChangingAreas,
    CountingRewards,
    Entering,
    InQueue,
    FindingRegistrar,
    WaitingForNavmesh,
    Walking,
    MoveCloser,
    Busy,
    Talking,
    Registering,
    Retrying,
    StoppedMatchLimit,
    StoppedMgcCap,
    StoppedRegistration
}

internal sealed class AutoQueue
{
    private enum QueueAction : byte
    {
        None,
        Leave,
        Commence,
        Target,
        Interact,
        Advance,
        Answer,
        Choose,
        Register,
        Walk
    }

    private const int TickMs = 100;

    private const int ResultsPauseMinMs = 2000;

    private const int ResultsPauseMaxMs = 4000;

    private const int ResultsPressesBeforeWarning = 3;

    private const int StepPauseMinMs = 400;

    private const int StepPauseMaxMs = 1100;

    private const int DialogueTimeoutMs = 8000;

    private const int MaxDialogueClicks = 12;

    private const int MaxRegisterPresses = 3;

    private const int QueueAppearTimeoutMs = 30000;

    private const int RetryBackoffMs = 5000;

    private const int MaxRegisterAttempts = 5;

    private const float InteractReach = 3f;

    private const float ArriveReach = 2.5f;

    private const int PathfindGraceMs = 1500;

    private const int WalkTimeoutMs = 45000;

    private const int MaxPathRequests = 3;

    private const int EstimatedMatchMgc = 300;

    private const int MgcStopAt = 9700;

    private readonly Configuration configuration;

    private readonly TypingSession session;

    private readonly RewardTracker rewards;

    private readonly NavmeshIpc navmesh;

    private long nextTickAt;

    private QueueAction pausedFor;

    private long pauseUntil;

    private int resultsPresses;

    private bool registered;

    private bool queueJoined;

    private long dialogueUntil;

    private long queueUntil;

    private int dialogueClicks;

    private int registerPresses;

    private int attempts;

    private long retryAt;

    private bool ownsPath;

    private long walkStartedAt;

    private long walkCheckAt;

    private int pathRequests;

    public bool Enabled => configuration.AutoQueue;

    public QueueStep Step { get; private set; }

    public bool StopRequested { get; private set; }

    public bool Active => Step != QueueStep.Off && !Stopped;

    public bool Stopped => Step is QueueStep.StoppedMatchLimit or QueueStep.StoppedMgcCap or QueueStep.StoppedRegistration;

    public AutoQueue(Configuration configuration, TypingSession session, RewardTracker rewards)
    {
        this.configuration = configuration;
        this.session = session;
        this.rewards = rewards;
        navmesh = new NavmeshIpc(Svc.PluginInterface);
    }

    public void Begin()
    {
        ResetCycle();
        StopRequested = false;
        Step = QueueStep.Off;
        resultsPresses = 0;
        nextTickAt = 0L;
    }

    public void Halt()
    {
        ResetCycle();
        if (!Stopped)
        {
            Step = QueueStep.Off;
        }
    }

    public void Update(BattleSnapshot snapshot, long now)
    {
        if (!configuration.AutoQueue)
        {
            if (Active)
            {
                RunLog.Debug("Auto queue: switched off.");
                Halt();
            }
            return;
        }
        if (now < nextTickAt)
        {
            return;
        }
        nextTickAt = now + TickMs;
        Decide(snapshot, now);
    }

    private void Decide(BattleSnapshot snapshot, long now)
    {
        if (QueueWindows.ResultsOpen)
        {
            CloseResults(now);
            return;
        }
        resultsPresses = 0;
        if (snapshot.InputVisible || QueueConditions.InDuty)
        {
            if (Step != QueueStep.InMatch)
            {
                ResetCycle();
            }
            Enter(QueueStep.InMatch);
        }
        else if (QueueConditions.ChangingAreas)
        {
            Enter(QueueStep.ChangingAreas);
        }
        else if (registered && QueueWindows.ConfirmOpen)
        {
            Commence(now);
        }
        else
        {
            if (Converse(now))
            {
                return;
            }
            if (QueueConditions.Queued)
            {
                WaitInQueue();
                return;
            }
            if (rewards.Settling)
            {
                Enter(QueueStep.CountingRewards);
                return;
            }
            if (LimitReached())
            {
                return;
            }
            Approach(now);
        }
    }

    private void CloseResults(long now)
    {
        StopOwnPath();
        Enter(QueueStep.ClosingResults);
        if (Paused(QueueAction.Leave, now, ResultsPauseMinMs, ResultsPauseMaxMs))
        {
            return;
        }
        Acted();
        if (resultsPresses == ResultsPressesBeforeWarning)
        {
            RunLog.Warning($"The results window is still open after {resultsPresses} tries at its leave button; please send this log.");
        }
        resultsPresses++;
        RunLog.Debug(QueueWindows.PressLeave() ? "Pressed the results window's leave button." : "The results window's leave button is not ready yet.");
    }

    private void Commence(long now)
    {
        Enter(QueueStep.Entering);
        if (Paused(QueueAction.Commence, now, StepPauseMinMs, StepPauseMaxMs) || !QueueWindows.PressCommence())
        {
            return;
        }
        Acted();
        RunLog.Info("Pressed Commence on the duty confirmation.");
    }

    private void WaitInQueue()
    {
        if (!queueJoined)
        {
            RunLog.Info("Registered; waiting in the queue for the next match.");
        }
        StopOwnPath();
        queueJoined = true;
        attempts = 0;
        retryAt = 0L;
        Enter(QueueStep.InQueue);
    }

    private bool Converse(long now)
    {
        if (now >= dialogueUntil && now >= queueUntil)
        {
            return false;
        }
        if (QueueWindows.EntryOpen)
        {
            PressRegister(now);
            return true;
        }
        if (QueueWindows.TalkOpen)
        {
            Answer(QueueAction.Advance, now);
            return true;
        }
        if (QueueWindows.YesnoOpen)
        {
            Answer(QueueAction.Answer, now);
            return true;
        }
        if (QueueWindows.MenuOpen)
        {
            Answer(QueueAction.Choose, now);
            return true;
        }
        return false;
    }

    private void PressRegister(long now)
    {
        Enter(QueueStep.Registering);
        if (registerPresses >= MaxRegisterPresses)
        {
            Fail("The Register button did not respond.", now);
            return;
        }
        if (Paused(QueueAction.Register, now, StepPauseMinMs, StepPauseMaxMs) || !QueueWindows.PressRegister())
        {
            return;
        }
        Acted();
        registerPresses++;
        registered = true;
        dialogueUntil = 0L;
        queueUntil = now + QueueAppearTimeoutMs;
        RunLog.Debug("Pressed Register.");
    }

    private void Answer(QueueAction action, long now)
    {
        Enter(QueueStep.Talking);
        if (dialogueClicks >= MaxDialogueClicks)
        {
            Fail("The registrar's conversation never reached the registration window.", now);
            return;
        }
        if (Paused(action, now, StepPauseMinMs, StepPauseMaxMs) || !Perform(action))
        {
            return;
        }
        Acted();
        dialogueClicks++;
        if (dialogueUntil > 0)
        {
            dialogueUntil = now + DialogueTimeoutMs;
        }
    }

    private static bool Perform(QueueAction action)
    {
        switch (action)
        {
            case QueueAction.Advance:
                if (!QueueWindows.AdvanceTalk())
                {
                    return false;
                }
                RunLog.Debug("Advanced the registrar's dialogue.");
                return true;
            case QueueAction.Answer:
                {
                    if (!QueueWindows.AnswerYes(out string prompt))
                    {
                        return false;
                    }
                    RunLog.Info("Answered Yes to \"" + prompt + "\".");
                    return true;
                }
            default:
                {
                    if (!QueueWindows.ChooseFirstEntry(out string entry))
                    {
                        return false;
                    }
                    RunLog.Info("Chose \"" + entry + "\" from the registrar's menu.");
                    return true;
                }
        }
    }

    private bool LimitReached()
    {
        int stopAfterMatches = configuration.StopAfterMatches;
        if (stopAfterMatches > 0 && session.MatchesCompleted >= stopAfterMatches)
        {
            RequestStop(QueueStep.StoppedMatchLimit, $"Match limit reached: {session.MatchesCompleted} of {stopAfterMatches} played. Auto queue stops here and auto typing turns off.");
            return true;
        }
        if (!configuration.StopAtMgcCap)
        {
            return false;
        }
        int mgc = Wallet.Mgc;
        if (mgc < MgcStopAt)
        {
            return false;
        }
        RequestStop(QueueStep.StoppedMgcCap, string.Create(CultureInfo.InvariantCulture, $"MGC cap reached: {mgc:N0} of {Wallet.MgcCap:N0}, and another match could pass the cap. Auto queue stops here and auto typing turns off."));
        return true;
    }

    private unsafe void Approach(long now)
    {
        if (queueUntil > 0)
        {
            if (now < queueUntil)
            {
                Enter(QueueStep.Registering);
                return;
            }
            queueUntil = 0L;
            if (!queueJoined)
            {
                Fail("Register was pressed, but the queue never started.", now);
                return;
            }
        }
        if (queueJoined)
        {
            queueJoined = false;
            RunLog.Info("The queue ended without a match; registering again.");
        }
        if (dialogueUntil > 0)
        {
            if (now < dialogueUntil)
            {
                Enter(QueueStep.Talking);
            }
            else
            {
                Fail("The registrar did not answer.", now);
            }
            return;
        }
        if (now < retryAt)
        {
            Enter(QueueStep.Retrying);
            return;
        }
        GameObject* registrar = Registrar.Find();
        GameObject* localPlayer = Registrar.LocalPlayer();
        if (registrar == null || localPlayer == null)
        {
            StopOwnPath();
            Enter(QueueStep.FindingRegistrar);
            return;
        }
        float distance = Registrar.EdgeDistance(localPlayer, registrar);
        if (distance > (ownsPath ? ArriveReach : InteractReach))
        {
            Walk(registrar, distance, now);
            return;
        }
        if (ownsPath)
        {
            StopOwnPath();
            RunLog.Debug("Reached the registrar.");
        }
        pathRequests = 0;
        if (QueueConditions.CharacterBusy || !localPlayer->GetIsTargetable())
        {
            Enter(QueueStep.Busy);
        }
        else
        {
            TalkTo(registrar, now);
        }
    }

    private unsafe void TalkTo(GameObject* registrar, long now)
    {
        Enter(QueueStep.Talking);
        if (!Registrar.IsTargeted(registrar))
        {
            if (Paused(QueueAction.Target, now, StepPauseMinMs, StepPauseMaxMs))
            {
                return;
            }
            Acted();
            Registrar.Target(registrar);
            RunLog.Debug("Targeted the registrar.");
            return;
        }
        if (Paused(QueueAction.Interact, now, StepPauseMinMs, StepPauseMaxMs))
        {
            return;
        }
        Acted();
        ulong interactResult = Registrar.Interact(registrar);
        dialogueUntil = now + DialogueTimeoutMs;
        dialogueClicks = 0;
        registerPresses = 0;
        RunLog.Debug($"Talking to the registrar (interaction returned {interactResult}).");
    }

    private unsafe void Walk(GameObject* registrar, float distance, long now)
    {
        if (!configuration.WalkToRegistrar || !navmesh.Available || pathRequests >= MaxPathRequests)
        {
            if (pathRequests >= MaxPathRequests && Step != QueueStep.MoveCloser)
            {
                RunLog.Warning($"Could not walk to the registrar after {pathRequests} tries; move within {InteractReach:0} yalms and auto queue carries on.");
            }
            StopOwnPath();
            Enter(QueueStep.MoveCloser);
            return;
        }
        if (ownsPath)
        {
            FollowPath(distance, now);
            return;
        }
        if (navmesh.IsRunning())
        {
            Enter(QueueStep.Walking);
            return;
        }
        if (!navmesh.IsReady())
        {
            Enter(QueueStep.WaitingForNavmesh);
            return;
        }
        Enter(QueueStep.Walking);
        if (Paused(QueueAction.Walk, now, StepPauseMinMs, StepPauseMaxMs))
        {
            return;
        }
        Acted();
        pathRequests++;
        if (!navmesh.PathfindAndMoveTo(Registrar.Position(registrar)))
        {
            RunLog.Warning(string.Create(CultureInfo.InvariantCulture, $"vnavmesh found no path to the registrar, {distance:0.0} yalms away."));
            Enter(QueueStep.MoveCloser);
            return;
        }
        ownsPath = true;
        walkStartedAt = now;
        walkCheckAt = now + PathfindGraceMs;
        RunLog.Info(string.Create(CultureInfo.InvariantCulture, $"Walking to the registrar, {distance:0.0} yalms away."));
    }

    private void FollowPath(float distance, long now)
    {
        Enter(QueueStep.Walking);
        if (now - walkStartedAt > WalkTimeoutMs)
        {
            RunLog.Warning("Walking to the registrar is taking too long; asking vnavmesh for a new path.");
            StopOwnPath();
            return;
        }
        if (now < walkCheckAt || navmesh.IsRunning())
        {
            return;
        }
        ownsPath = false;
        RunLog.Debug(string.Create(CultureInfo.InvariantCulture, $"The path ended {distance:0.0} yalms from the registrar; asking for a new one."));
    }

    private void Fail(string reason, long now)
    {
        attempts++;
        dialogueUntil = 0L;
        queueUntil = 0L;
        if (attempts >= MaxRegisterAttempts)
        {
            RunLog.Warning($"{reason} Giving up after {attempts} tries.");
            RequestStop(QueueStep.StoppedRegistration, "Could not register for a match. Auto queue stops here and auto typing turns off.");
        }
        else
        {
            retryAt = now + RetryBackoffMs * attempts;
            Enter(QueueStep.Retrying);
            RunLog.Info($"{reason} Trying again ({attempts} of {MaxRegisterAttempts}).");
        }
    }

    private void RequestStop(QueueStep reason, string message)
    {
        ResetCycle();
        Step = reason;
        StopRequested = true;
        RunLog.Info(message);
        Svc.Chat.Print(Loc.T(QueueStatus.Label(reason)), AkbConstants.ChatTag);
    }

    private void Enter(QueueStep step)
    {
        if (Step == step)
        {
            return;
        }
        Step = step;
        RunLog.Debug($"Auto queue: {step}.");
    }

    private bool Paused(QueueAction action, long now, int minimumMs, int maximumMs)
    {
        if (pausedFor != action)
        {
            pausedFor = action;
            pauseUntil = now + Random.Shared.Next(minimumMs, maximumMs + 1);
        }
        return now < pauseUntil;
    }

    private void Acted()
    {
        pausedFor = QueueAction.None;
    }

    private void StopOwnPath()
    {
        if (!ownsPath)
        {
            return;
        }
        ownsPath = false;
        navmesh.Stop();
    }

    private void ResetCycle()
    {
        StopOwnPath();
        registered = false;
        queueJoined = false;
        dialogueUntil = 0L;
        queueUntil = 0L;
        dialogueClicks = 0;
        registerPresses = 0;
        attempts = 0;
        retryAt = 0L;
        pathRequests = 0;
        pausedFor = QueueAction.None;
    }
}
