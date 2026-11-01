using System;
using AutoKeyboundBrawler.Core.Game;
using AutoKeyboundBrawler.Core.Queue;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;

namespace AutoKeyboundBrawler.Core.Typing;

public enum TyperPhase : byte
{
    Idle,
    WaitingForMatch,
    WaitingForFocus,
    WaitingForPhrases,
    HoldingForCast,
    Typing,
    Clearing,
    Stalled
}

internal sealed class AutoTyper : IDisposable
{
    private readonly Configuration configuration;

    private readonly BattleSnapshot snapshot = new BattleSnapshot();

    private readonly TextSlotCache textCache = new TextSlotCache(25);

    private readonly PhraseMemory phraseMemory = new PhraseMemory();

    private readonly TypingSession session = new TypingSession();

    private readonly CastWatch castWatch = new CastWatch();

    private readonly SnapshotRecorder snapshotRecorder = new SnapshotRecorder();

    private readonly BarrierMemory barrierMemory = new BarrierMemory();

    private readonly BrawlerAgent brawlerAgent = new BrawlerAgent();

    private readonly PlayerStatusLog statusLog = new PlayerStatusLog();

    private readonly RewardTracker rewards;

    private readonly AutoQueue queue;

    private string? target;

    private string? targetTyped;

    private long nextWakeKeyAt;

    private long nextTimerLogAt;

    private int wakeAttempts;

    private bool reactionPending;

    private string? lastSubmitted;

    private long lastSubmittedAt;

    private int repeatStreak;

    private float streakSignature;

    private string? demotedPhrase;

    private long demotedUntil;

    private long nextActionAt;

    private bool awaitingEcho;

    private string textAtSend = string.Empty;

    private long echoDeadline;

    private int consecutiveStalls;

    private long nextFocusAttemptAt;

    private int focusAttempts;

    private GameKey heldKey;

    private bool keyHeld;

    private bool heldKeyIsSystem;

    private long keyReleaseAt;

    private bool directFallback;

    private bool wasDirect;

    private string? waitingFor;

    private long waitStartedAt;

    private long submitAt;

    private int rejections;

    private long unlinkedSince;

    public bool Running { get; private set; }

    public TyperPhase Phase { get; private set; }

    public string Target => target ?? string.Empty;

    public string HoldingSpell => castWatch.HoldingSpell;

    public float HoldRemaining => castWatch.HoldRemaining(Environment.TickCount64);

    public BattleSnapshot Snapshot => snapshot;

    public TypingSession Session => session;

    public bool SubmitsDirectly => configuration.InputMethod == InputMethod.Direct && !directFallback;

    public float SubmitProgress
    {
        get
        {
            long waitDuration = submitAt - waitStartedAt;
            if (waitingFor != null && waitDuration > 0)
            {
                return Math.Clamp((float)(Environment.TickCount64 - waitStartedAt) / (float)waitDuration, 0f, 1f);
            }
            return 0f;
        }
    }

    public AutoQueue Queue => queue;

    public AutoTyper(Configuration configuration)
    {
        this.configuration = configuration;
        rewards = new RewardTracker(configuration, session);
        queue = new AutoQueue(configuration, session, rewards);
        Svc.Framework.Update += OnUpdate;
    }

    public void Dispose()
    {
        Svc.Framework.Update -= OnUpdate;
        ReleaseHeldKey();
        queue.Halt();
    }

    public void Start()
    {
        if (Running)
        {
            return;
        }
        long tickCount = Environment.TickCount64;
        Running = true;
        consecutiveStalls = 0;
        directFallback = false;
        session.Begin(tickCount);
        snapshotRecorder.Begin();
        castWatch.Reset();
        statusLog.Reset();
        phraseMemory.Clear();
        barrierMemory.Clear();
        rewards.Reset();
        queue.Begin();
        repeatStreak = 0;
        demotedPhrase = null;
        ResetTyping();
        Phase = TyperPhase.WaitingForMatch;
        RunLog.Info($"Auto typing on at {configuration.Speed} speed{((configuration.InputMethod == InputMethod.Direct) ? ", submitting words directly" : string.Empty)}.");
    }

    public void Stop()
    {
        if (!Running)
        {
            return;
        }
        Running = false;
        ReleaseHeldKey();
        ResetTyping();
        queue.Halt();
        rewards.Finish();
        session.End(Environment.TickCount64);
        Phase = TyperPhase.Idle;
        RunLog.Info($"Auto typing off: {session.PhrasesSubmitted} phrases submitted, {session.KeysSent} keys sent.");
    }

    public void Toggle()
    {
        if (Running)
        {
            Stop();
        }
        else
        {
            Start();
        }
    }

    private void OnUpdate(IFramework framework)
    {
        long tickCount = Environment.TickCount64;
        if (keyHeld && tickCount >= keyReleaseAt)
        {
            ReleaseHeldKey();
        }
        brawlerAgent.Read();
        KeyboundUi.Read(snapshot, textCache, phraseMemory, brawlerAgent);
        barrierMemory.Apply(snapshot, tickCount);
        if (!Running)
        {
            return;
        }
        rewards.Update(snapshot, tickCount);
        queue.Update(snapshot, tickCount);
        if (queue.StopRequested)
        {
            Stop();
            return;
        }
        if (!snapshot.InputVisible)
        {
            Wait(TyperPhase.WaitingForMatch, tickCount);
            return;
        }
        if (tickCount >= nextTimerLogAt)
        {
            nextTimerLogAt = tickCount + AkbConstants.TimerLogSpacingMs;
            GameActivity.LogTimers();
        }
        GameActivity.MarkInput();
        castWatch.Update(snapshot, brawlerAgent, tickCount);
        statusLog.Update();
        snapshotRecorder.Observe(configuration, snapshot, castWatch.Holding, tickCount);
        bool guardHolding = configuration.LetterlockGuard && castWatch.Holding;
        bool submitsDirectly = SubmitsDirectly;
        if (submitsDirectly != wasDirect)
        {
            wasDirect = submitsDirectly;
            ResetTyping();
        }
        if (submitsDirectly)
        {
            UpdateDirect(guardHolding, tickCount);
            return;
        }
        if (!snapshot.InputFocused)
        {
            Wait(TyperPhase.WaitingForFocus, tickCount);
            if (!guardHolding)
            {
                TryRefocus(tickCount);
            }
            return;
        }
        focusAttempts = 0;
        wakeAttempts = 0;
        session.ResumeClock(tickCount);
        if (guardHolding)
        {
            Phase = TyperPhase.HoldingForCast;
            return;
        }
        if (keyHeld || tickCount < nextActionAt || IsAwaitingEcho(tickCount))
        {
            return;
        }
        Act(tickCount);
    }

    private void Wait(TyperPhase phase, long now)
    {
        if (Phase != phase && phase != TyperPhase.WaitingForFocus)
        {
            RunLog.Debug($"Waiting: {phase}.");
        }
        session.PauseClock(now);
        ResetTyping();
        Phase = phase;
    }

    private void TryRefocus(long now)
    {
        if (!configuration.AutoFocusInput || snapshot.OtherInputFocused || now < nextFocusAttemptAt)
        {
            return;
        }
        nextFocusAttemptAt = now + AkbConstants.FocusRetryMs;
        if (snapshot.PhraseCount > 0)
        {
            GameKeyboard.ReclaimForeground();
        }
        KeyboundUi.FocusInput();
        PressWakeKey(now);
        if (snapshot.PhraseCount == 0)
        {
            focusAttempts = 0;
            return;
        }
        focusAttempts++;
        if (focusAttempts == AkbConstants.FocusAttemptsBeforeWarning)
        {
            RunLog.Warning("The input bar has not taken focus for a few seconds while phrases are up. Click it once to carry on, and copy this log into a bug report.");
            BlameRecentCast(now);
        }
    }

    private void PressWakeKey(long now)
    {
        if (keyHeld || snapshot.PhraseCount == 0 || now < nextWakeKeyAt)
        {
            return;
        }
        var rules = new TargetRules(configuration.Order, configuration.CrowdedAt, null, null, LinkedOnly: false);
        int phraseIndex = TargetPicker.Choose(snapshot, string.Empty, in rules);
        if (phraseIndex == -1 || snapshot.PhraseTyped[phraseIndex].Length == 0 || (!GameKeyboard.TryKeyFor(snapshot.PhraseTyped[phraseIndex][0], out var key) && !GameKeyboard.TryKeyFor(' ', out key)))
        {
            return;
        }
        wakeAttempts++;
        nextWakeKeyAt = now + AkbConstants.WakeKeySpacingMs * Math.Min(1 + wakeAttempts / AkbConstants.WakeKeysPerStep, AkbConstants.MaxWakeKeyStep);
        if (!GameKeyboard.SendSystemKey(key, keyUp: false))
        {
            PressKey(key, now);
            return;
        }
        GameActivity.MarkInput();
        session.CountKey();
        heldKey = key;
        keyHeld = true;
        heldKeyIsSystem = true;
        keyReleaseAt = now + AkbConstants.KeyHoldMs;
    }

    private bool IsAwaitingEcho(long now)
    {
        if (!awaitingEcho)
        {
            return false;
        }
        if (!string.Equals(snapshot.InputText, textAtSend, StringComparison.Ordinal))
        {
            awaitingEcho = false;
            consecutiveStalls = 0;
            return false;
        }
        if (now < echoDeadline)
        {
            return true;
        }
        awaitingEcho = false;
        consecutiveStalls++;
        nextActionAt = now + AkbConstants.StallBackoffMs;
        Phase = TyperPhase.Stalled;
        if (consecutiveStalls == 1)
        {
            RunLog.Warning("The input bar did not take the last key; retrying in a moment (inputs may be disabled).");
            BlameRecentCast(now);
        }
        if (consecutiveStalls == AkbConstants.MaxConsecutiveStalls)
        {
            RunLog.Error($"The input bar ignored {consecutiveStalls} keys in a row. Still retrying; copy the console log into a bug report.");
        }
        return true;
    }

    private void BlameRecentCast(long now)
    {
        string lastCompletedSpell = castWatch.LastCompletedSpell;
        if (lastCompletedSpell.Length > 0 && now - castWatch.LastCompletedAt < AkbConstants.LockoutBlameWindowMs)
        {
            RunLog.Info("Keys stopped landing right after " + lastCompletedSpell + " completed; it may be a Letterlock spell.");
        }
    }

    private void Act(long now)
    {
        string inputText = snapshot.InputText;
        PickTarget(inputText, now, linkedOnly: false);
        if (target == null)
        {
            if (inputText.Length > 0)
            {
                PressBackspaces(inputText.Length, now);
            }
            else
            {
                Phase = TyperPhase.WaitingForPhrases;
            }
            return;
        }
        string expected = targetTyped ?? target;
        if (inputText.Equals(expected, StringComparison.OrdinalIgnoreCase))
        {
            Submit(now);
            return;
        }
        if (!expected.StartsWith(inputText, StringComparison.OrdinalIgnoreCase))
        {
            PressBackspaces(inputText.Length - SharedPrefixLength(inputText, expected), now);
            return;
        }
        if (TryReact(inputText, now))
        {
            return;
        }
        char nextChar = expected[inputText.Length];
        char typoChar = ((inputText.Length == 0) ? '\0' : Humanizer.RollTypo(configuration, nextChar));
        if (typoChar != '\0')
        {
            RunLog.Debug($"Humanizer slip: '{typoChar}' instead of '{nextChar}'.");
            TypeCharacter(typoChar, completesPhrase: false, now);
        }
        else
        {
            TypeCharacter(nextChar, inputText.Length + 1 == expected.Length, now);
        }
    }

    private void PickTarget(string input, long now, bool linkedOnly)
    {
        string? skipped = ((now - lastSubmittedAt < AkbConstants.RecentSubmissionMs) ? lastSubmitted : null);
        var rules = new TargetRules(configuration.Order, configuration.CrowdedAt, (now < demotedUntil) ? demotedPhrase : null, skipped, linkedOnly);
        int currentIndex = ((target == null) ? (-1) : TargetPicker.IndexOf(snapshot, target, linkedOnly));
        int bestIndex = TargetPicker.Choose(snapshot, input, in rules);
        if (currentIndex != -1 && (bestIndex == -1 || TargetPicker.Rank(snapshot, bestIndex, in rules) <= TargetPicker.Rank(snapshot, currentIndex, in rules)))
        {
            return;
        }
        if (currentIndex != -1 && IsNearlyTyped(currentIndex, input))
        {
            return;
        }
        target = ((bestIndex == -1) ? null : snapshot.Phrases[bestIndex]);
        targetTyped = ((bestIndex == -1) ? null : snapshot.PhraseTyped[bestIndex]);
        reactionPending = target != null;
        if (bestIndex != -1 && TargetPicker.IsPriority(snapshot, bestIndex, in rules))
        {
            RunLog.Debug($"Priority target \"{target}\" ({(snapshot.PhraseOnBarrier[bestIndex] ? "barrier" : (snapshot.IsInterruptible(bestIndex) ? "interrupt" : "casting"))}).");
        }
    }

    private bool IsNearlyTyped(int index, string input)
    {
        string typed = snapshot.PhraseTyped[index];
        return input.Length > 0 && typed.StartsWith(input, StringComparison.OrdinalIgnoreCase) && typed.Length - input.Length <= AkbConstants.FinishBeforeSwitchLetters;
    }

    private bool TryReact(string input, long now)
    {
        if (!reactionPending)
        {
            return false;
        }
        reactionPending = false;
        int reactDelay = ((input.Length == 0) ? Humanizer.DrawDelayMs(configuration, HumanAction.React) : 0);
        if (reactDelay <= 0)
        {
            return false;
        }
        Phase = TyperPhase.Typing;
        nextActionAt = now + reactDelay;
        return true;
    }

    private void TypeCharacter(char character, bool completesPhrase, long now)
    {
        Phase = TyperPhase.Typing;
        if (!GameKeyboard.PostCharacter(character))
        {
            nextActionAt = now + 1000;
            return;
        }
        GameActivity.MarkInput();
        session.CountCharacter();
        ExpectEcho(now);
        TypingPace pace = TypingPaces.Of(configuration.Speed);
        nextActionAt = now + (completesPhrase ? SubmitDelay(pace) : NextKeyDelay(pace));
    }

    private int SubmitDelay(TypingPace pace)
    {
        if (!configuration.HumanizerEnabled)
        {
            return pace.SubmitDelayMs;
        }
        return Humanizer.DrawDelayMs(configuration, HumanAction.Submit);
    }

    private void PressBackspaces(int count, long now)
    {
        Phase = TyperPhase.Clearing;
        int presses = Math.Clamp(count, 1, AkbConstants.MaxBackspacesPerBurst);
        PressKey(GameKey.Backspace, now);
        for (int i = 1; i < presses; i++)
        {
            GameKeyboard.PostKeyRepeat(GameKey.Backspace);
            session.CountKey();
        }
        ExpectEcho(now);
        nextActionAt = now + BackspaceDelay(TypingPaces.Of(configuration.Speed));
    }

    private static int SharedPrefixLength(string input, string typed)
    {
        int limit = Math.Min(input.Length, typed.Length);
        for (int i = 0; i < limit; i++)
        {
            if (char.ToUpperInvariant(input[i]) != char.ToUpperInvariant(typed[i]))
            {
                return i;
            }
        }
        return limit;
    }

    private void Submit(long now)
    {
        Phase = TyperPhase.Typing;
        PressKey(GameKey.Enter, now);
        session.CountPhrase();
        CompleteSubmission(now);
    }

    private void CompleteSubmission(long now)
    {
        RunLog.Info("Submitted \"" + target + "\".");
        TrackRepeats(target!, now);
        RememberSubmission(now);
        lastSubmitted = target;
        target = null;
        targetTyped = null;
        waitingFor = null;
        nextActionAt = now + AkbConstants.AfterSubmitMs;
    }

    private void UpdateDirect(bool holding, long now)
    {
        focusAttempts = 0;
        wakeAttempts = 0;
        session.ResumeClock(now);
        if (holding)
        {
            Phase = TyperPhase.HoldingForCast;
            return;
        }
        if (now < nextActionAt)
        {
            return;
        }
        if (!WordSubmitter.Available)
        {
            FallBackToTyping("the game's word function was not found");
            return;
        }
        ActDirect(now);
    }

    private void ActDirect(long now)
    {
        PickTarget(string.Empty, now, linkedOnly: true);
        if (target == null)
        {
            waitingFor = null;
            Phase = TyperPhase.WaitingForPhrases;
            WatchUnlinked(now);
            return;
        }
        unlinkedSince = 0L;
        if ((object)target != waitingFor)
        {
            BeginWait(now);
        }
        Phase = TyperPhase.Typing;
        if (now >= submitAt)
        {
            SubmitDirect(now);
        }
    }

    private void BeginWait(long now)
    {
        var typedText = targetTyped ?? target!;
        TypingPace pace = TypingPaces.Of(configuration.Speed);
        int totalDelay = Humanizer.DrawDelayMs(configuration, HumanAction.React) + SubmitDelay(pace);
        for (int i = 0; i < typedText.Length; i++)
        {
            totalDelay += NextKeyDelay(pace);
        }
        waitingFor = target;
        waitStartedAt = now;
        submitAt = now + totalDelay;
        rejections = 0;
        reactionPending = false;
    }

    private void SubmitDirect(long now)
    {
        int phraseIndex = TargetPicker.IndexOf(snapshot, target!, linkedOnly: true);
        if (phraseIndex == -1 || !WordSubmitter.TrySubmit(brawlerAgent.Director, snapshot.PhraseEntityId[phraseIndex]))
        {
            Reject(now);
            return;
        }
        session.CountPhrase();
        session.CountTypedCharacters((targetTyped ?? target!).Length);
        CompleteSubmission(now);
    }

    private void Reject(long now)
    {
        rejections++;
        nextActionAt = now + AkbConstants.DirectRetryMs;
        if (rejections == 1)
        {
            RunLog.Debug("The game did not take \"" + target + "\"; offering it again.");
        }
        if (rejections < AkbConstants.DirectRejectionsBeforeSkip)
        {
            return;
        }
        RunLog.Info($"The game turned \"{target}\" down {rejections} times in a row; trying other phrases for a few seconds.");
        demotedPhrase = target;
        demotedUntil = now + AkbConstants.DemoteMs;
        rejections = 0;
    }

    private void WatchUnlinked(long now)
    {
        if (snapshot.PhraseCount == 0 || AnyPhraseLinked())
        {
            unlinkedSince = 0L;
        }
        else if (unlinkedSince == 0L)
        {
            unlinkedSince = now;
        }
        else if (now - unlinkedSince >= AkbConstants.DirectUnlinkedGraceMs)
        {
            FallBackToTyping("no phrase on screen was found in the game's word data");
        }
    }

    private bool AnyPhraseLinked()
    {
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            if (snapshot.IsLinked(i))
            {
                return true;
            }
        }
        return false;
    }

    private void FallBackToTyping(string reason)
    {
        directFallback = true;
        RunLog.Warning("Words cannot be submitted directly (" + reason + "), so they are typed for the rest of this session.");
    }

    private void RememberSubmission(long now)
    {
        lastSubmittedAt = now;
    }

    private void TrackRepeats(string submitted, long now)
    {
        int index = TargetPicker.IndexOf(snapshot, submitted);
        float signature = ((index == -1) ? 0f : snapshot.ProgressSignature(index));
        bool sameAsLast = string.Equals(submitted, lastSubmitted, StringComparison.OrdinalIgnoreCase);
        repeatStreak = ((sameAsLast && MathF.Abs(signature - streakSignature) < AkbConstants.ProgressEpsilon) ? (repeatStreak + 1) : 1);
        streakSignature = signature;
        if (repeatStreak < AkbConstants.RepeatsWithoutProgress)
        {
            return;
        }
        RunLog.Info($"\"{submitted}\" went in {repeatStreak} times with no visible effect; typing other phrases for a few seconds.");
        demotedPhrase = submitted;
        demotedUntil = now + AkbConstants.DemoteMs;
        repeatStreak = 0;
    }

    private void PressKey(GameKey key, long now)
    {
        if (!GameKeyboard.PostKeyDown(key))
        {
            return;
        }
        GameActivity.MarkInput();
        session.CountKey();
        heldKey = key;
        keyHeld = true;
        keyReleaseAt = now + AkbConstants.KeyHoldMs;
    }

    private void ReleaseHeldKey()
    {
        if (!keyHeld)
        {
            return;
        }
        keyHeld = false;
        if (heldKeyIsSystem)
        {
            heldKeyIsSystem = false;
            GameKeyboard.SendSystemKey(heldKey, keyUp: true);
            return;
        }
        GameKeyboard.PostKeyUp(heldKey);
    }

    private void ExpectEcho(long now)
    {
        awaitingEcho = true;
        textAtSend = snapshot.InputText;
        echoDeadline = now + AkbConstants.EchoTimeoutMs;
    }

    private static int NextKeyDelay(TypingPace pace) => pace.KeyDelayMs + Random.Shared.Next(0, pace.KeyJitterMs + 1);

    private static int BackspaceDelay(TypingPace pace) => Math.Max(AkbConstants.MinBackspaceDelayMs, (int)((float)pace.KeyDelayMs * AkbConstants.BackspaceDelayFactor)) + Random.Shared.Next(0, pace.KeyJitterMs / 2 + 1);

    private void ResetTyping()
    {
        target = null;
        targetTyped = null;
        reactionPending = false;
        awaitingEcho = false;
        nextActionAt = 0L;
        waitingFor = null;
        rejections = 0;
        unlinkedSince = 0L;
    }
}
