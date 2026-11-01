using Dalamud.Game.ClientState.Conditions;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Event;

namespace AutoKeyboundBrawler.Core.Queue;

internal static class QueueConditions
{
    public unsafe static bool InDuty
    {
        get
        {
            EventFramework* eventFramework = EventFramework.Instance();
            return (eventFramework != null && eventFramework->GetInstanceContentDirector() != null) || Svc.Condition[ConditionFlag.BoundByDuty] || Svc.Condition[ConditionFlag.BoundByDuty56];
        }
    }

    public static bool Queued => Svc.Condition[ConditionFlag.WaitingForDuty] || Svc.Condition[ConditionFlag.WaitingForDutyFinder] || Svc.Condition[ConditionFlag.InDutyQueue] || Svc.Condition[ConditionFlag.BoundByDuty95];

    public static bool ChangingAreas => Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51];

    public static bool CharacterBusy => GenericHelpers.IsOccupied();
}
