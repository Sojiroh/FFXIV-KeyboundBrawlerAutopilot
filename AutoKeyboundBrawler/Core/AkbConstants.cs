namespace AutoKeyboundBrawler.Core;

internal static class AkbConstants
{
    internal static class ThrottleKeys
    {
        public const string Save = "AutoKeyboundBrawler.Save";
    }

    public const string LogPrefix = "[KBA]";

    public const string PrimaryCommand = "/kba";

    public const string AliasCommand = "/keyboundautopilot";

    public const string ChatTag = "KBA";

    public const int EchoTimeoutMs = 400;

    public const int StallBackoffMs = 1000;

    public const int MaxConsecutiveStalls = 15;

    public const float BackspaceDelayFactor = 0.4f;

    public const int MinBackspaceDelayMs = 15;

    public const int MaxBackspacesPerBurst = 40;

    public const int WakeKeySpacingMs = 300;

    public const int WakeKeysPerStep = 3;

    public const int MaxWakeKeyStep = 4;

    public const int LockoutBlameWindowMs = 6000;

    public const int FocusRetryMs = 120;

    public const int FocusAttemptsBeforeWarning = 25;

    public const int KeyHoldMs = 35;

    public const int AfterSubmitMs = 160;

    public const float CastFilling = 0.9f;

    public const float CastFull = 0.97f;

    public const int CastHoldMs = 800;

    public const float LetterlockFull = 0.95f;

    public const int LetterlockHoldMs = 3500;

    public const int LetterlockLeadMs = 600;

    public const int LetterlockAfterMs = 3500;

    public const int LetterlockStatusMarginMs = 300;

    public const int DirectRetryMs = 250;

    public const int DirectRejectionsBeforeSkip = 3;

    public const int DirectUnlinkedGraceMs = 2000;

    public const int RecentSubmissionMs = 1500;

    public const int RepeatsWithoutProgress = 3;

    public const int BarrierSettleMs = 500;

    public const int FinishBeforeSwitchLetters = 6;

    public const float BarrierSpotReachX = 120f;

    public const float BarrierSpotReachY = 40f;

    public const int BarrierSpotMemoryMs = 8000;

    public const int DemoteMs = 4000;

    public const float ProgressEpsilon = 0.5f;

    public const int SnapshotSpacingMs = 1000;

    public const int SnapshotCastDelayMs = 1000;

    public const int MaxSnapshotsPerSession = 20;

    public const int TimerLogSpacingMs = 15000;

    public const int SaveThrottleMs = 500;
}
