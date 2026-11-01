using AutoKeyboundBrawler.Core.Localization;

namespace AutoKeyboundBrawler.Core.Queue;

internal static class QueueStatus
{
    public static LocString Label(QueueStep step)
    {
        return step switch
        {
            QueueStep.InMatch => L.Queue.InMatch,
            QueueStep.ClosingResults => L.Queue.ClosingResults,
            QueueStep.ChangingAreas => L.Queue.ChangingAreas,
            QueueStep.CountingRewards => L.Queue.CountingRewards,
            QueueStep.Entering => L.Queue.Entering,
            QueueStep.InQueue => L.Queue.InQueue,
            QueueStep.FindingRegistrar => L.Queue.FindingRegistrar,
            QueueStep.WaitingForNavmesh => L.Queue.WaitingForNavmesh,
            QueueStep.Walking => L.Queue.Walking,
            QueueStep.MoveCloser => L.Queue.MoveCloser,
            QueueStep.Busy => L.Queue.Busy,
            QueueStep.Talking => L.Queue.Talking,
            QueueStep.Registering => L.Queue.Registering,
            QueueStep.Retrying => L.Queue.Retrying,
            QueueStep.StoppedMatchLimit => L.Queue.StoppedMatchLimit,
            QueueStep.StoppedMgcCap => L.Queue.StoppedMgcCap,
            QueueStep.StoppedRegistration => L.Queue.StoppedRegistration,
            _ => L.Brawl.TitleMatch,
        };
    }

    public static bool NeedsPlayer(QueueStep step) => step is QueueStep.FindingRegistrar or QueueStep.MoveCloser or QueueStep.Busy;
}
