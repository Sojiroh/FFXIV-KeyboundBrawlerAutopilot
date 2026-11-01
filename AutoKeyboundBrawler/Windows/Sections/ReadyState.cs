using System.Numerics;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Queue;
using AutoKeyboundBrawler.Core.Typing;
using Dalamud.Bindings.ImGui;

namespace AutoKeyboundBrawler.Windows.Sections;

internal static class ReadyState
{
    public enum Kind
    {
        Ready,
        Waiting,
        Paused,
        Typing,
        Stalled
    }

    public readonly record struct Info(Kind Kind, Vector4 Accent, Vector4 AccentSoft, string Title, string Detail);

    private static int cachedFrame = -1;

    private static Info cached;

    public static Info Resolve(AutoTyper typer)
    {
        int frameCount = ImGui.GetFrameCount();
        if (frameCount == cachedFrame)
        {
            return cached;
        }
        cached = Compute(typer);
        cachedFrame = frameCount;
        return cached;
    }

    private static Info Compute(AutoTyper typer)
    {
        AutoQueue queue = typer.Queue;
        if (!typer.Running)
        {
            if (!queue.Stopped)
            {
                return new Info(Kind.Ready, Styling.AccentMint, Styling.AccentMintSoft, Loc.T(L.Brawl.TitleIdle), Loc.T(L.Brawl.DetailIdle));
            }
            return new Info(Kind.Ready, Styling.AccentMint, Styling.AccentMintSoft, Loc.T(QueueStatus.Label(queue.Step)), Loc.T(L.Queue.DetailStopped));
        }
        return typer.Phase switch
        {
            TyperPhase.WaitingForMatch when queue.Active && QueueStatus.NeedsPlayer(queue.Step) => new Info(Kind.Paused, Styling.AccentAmber, Styling.AccentAmberSoft, Loc.T(QueueStatus.Label(queue.Step)), Loc.T(L.Queue.DetailRunning)),
            TyperPhase.WaitingForMatch when queue.Active => new Info(Kind.Waiting, Styling.AccentTeal, Styling.AccentTealSoft, Loc.T(QueueStatus.Label(queue.Step)), Loc.T(L.Queue.DetailRunning)),
            TyperPhase.WaitingForFocus when typer.Snapshot.OtherInputFocused => new Info(Kind.Paused, Styling.AccentAmber, Styling.AccentAmberSoft, Loc.T(L.Brawl.TitleElsewhere), Loc.T(L.Brawl.DetailElsewhere)),
            TyperPhase.WaitingForFocus => new Info(Kind.Paused, Styling.AccentAmber, Styling.AccentAmberSoft, Loc.T(L.Brawl.TitleFocus), Loc.T(L.Brawl.DetailFocus)),
            TyperPhase.WaitingForPhrases => new Info(Kind.Typing, Styling.AccentTeal, Styling.AccentTealSoft, Loc.T(L.Brawl.TitleWave), Loc.T(L.Brawl.DetailWave)),
            TyperPhase.Typing or TyperPhase.Clearing => new Info(Kind.Typing, Styling.AccentTeal, Styling.AccentTealSoft, Loc.T(L.Brawl.TitleTyping, typer.Target), Loc.T(L.Brawl.DetailTyping, Formatting.Phrases(typer.Session.PhrasesSubmitted))),
            TyperPhase.HoldingForCast when typer.HoldingSpell.Length > 0 => new Info(Kind.Paused, Styling.AccentAmber, Styling.AccentAmberSoft, Loc.T(L.Brawl.TitleHoldingSpell, typer.HoldingSpell), Loc.T(L.Brawl.DetailHolding)),
            TyperPhase.HoldingForCast => new Info(Kind.Paused, Styling.AccentAmber, Styling.AccentAmberSoft, Loc.T(L.Brawl.TitleHolding), Loc.T(L.Brawl.DetailHolding)),
            TyperPhase.Stalled => new Info(Kind.Stalled, Styling.AccentRose, Styling.AccentRoseSoft, Loc.T(L.Brawl.TitleStalled), Loc.T(L.Brawl.DetailStalled)),
            _ => new Info(Kind.Waiting, Styling.AccentTeal, Styling.AccentTealSoft, Loc.T(L.Brawl.TitleMatch), Loc.T(L.Brawl.DetailMatch)),
        };
    }

    public static string ShortLabel(Kind kind)
    {
        return Loc.T(kind switch
        {
            Kind.Typing => L.Shell.StatusTyping,
            Kind.Waiting => L.Shell.StatusWaiting,
            Kind.Paused => L.Shell.StatusPaused,
            Kind.Stalled => L.Shell.StatusStalled,
            _ => L.Shell.StatusReady,
        });
    }

    public static string PhaseLabel(TyperPhase phase)
    {
        return Loc.T(phase switch
        {
            TyperPhase.WaitingForMatch => L.Brawl.PhaseMatch,
            TyperPhase.WaitingForFocus => L.Brawl.PhaseFocus,
            TyperPhase.WaitingForPhrases => L.Brawl.PhaseWave,
            TyperPhase.HoldingForCast => L.Brawl.PhaseHolding,
            TyperPhase.Typing => L.Brawl.PhaseTyping,
            TyperPhase.Clearing => L.Brawl.PhaseClearing,
            TyperPhase.Stalled => L.Brawl.PhaseStalled,
            _ => L.Brawl.PhaseIdle,
        });
    }
}
