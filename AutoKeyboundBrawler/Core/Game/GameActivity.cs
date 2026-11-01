using System;
using System.Globalization;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace AutoKeyboundBrawler.Core.Game;

internal static class GameActivity
{
    private const uint BrawlerHudAgent = 397u;

    private const uint BrawlerInputAgent = 398u;

    private const int InputAgentHudOffset = 40;

    private const int InputAgentIdleOffset = 80;

    private const int UnknownTimerOffset = 28;

    private static bool layoutWarned;

    public unsafe static void MarkInput()
    {
        ResetBrawlerIdle();
        var module = InputTimerModule.Instance();
        if (module == null)
        {
            return;
        }
        module->AfkTimer = 0f;
        module->ContentInputTimer = 0f;
        module->InputTimer = 0f;
        ResetTimers(module->ControllerInputTimers);
        ResetTimers(module->MouseInputTimers);
    }

    public unsafe static void LogTimers()
    {
        var module = InputTimerModule.Instance();
        if (module == null)
        {
            return;
        }
        var unknownTimer = *(float*)((byte*)module + UnknownTimerOffset);
        var brawlerIdle = BrawlerIdle();
        RunLog.Debug(string.Create(CultureInfo.InvariantCulture, $"Inactivity timers: brawler {brawlerIdle}, input {module->InputTimer:0.0}, content {module->ContentInputTimer:0.0}, afk {module->AfkTimer:0.0}, 0x1C {unknownTimer:0.0}, status {module->Status}, controller {Highest(module->ControllerInputTimers):0.0}, mouse {Highest(module->MouseInputTimers):0.0}."));
    }

    private unsafe static void ResetBrawlerIdle()
    {
        var inputAgent = BrawlerInputAgentChecked();
        if (inputAgent != null)
        {
            *(long*)(inputAgent + InputAgentIdleOffset) = 0L;
        }
    }

    private unsafe static string BrawlerIdle()
    {
        var inputAgent = BrawlerInputAgentChecked();
        if (inputAgent != null)
        {
            return ((ulong*)(inputAgent + InputAgentIdleOffset))->ToString(CultureInfo.InvariantCulture);
        }
        return "?";
    }

    private unsafe static byte* BrawlerInputAgentChecked()
    {
        var agentModule = AgentModule.Instance();
        if (agentModule == null)
        {
            return null;
        }
        var hudAgent = agentModule->GetAgentByInternalId((AgentId)BrawlerHudAgent);
        var inputAgent = (byte*)agentModule->GetAgentByInternalId((AgentId)BrawlerInputAgent);
        if (hudAgent == null || inputAgent == null)
        {
            return null;
        }
        if (*(IntPtr*)(inputAgent + InputAgentHudOffset) == (IntPtr)hudAgent)
        {
            return inputAgent;
        }
        if (!layoutWarned)
        {
            layoutWarned = true;
            RunLog.Warning("The brawler's input agent has an unexpected layout, so its inactivity timer is not reset; please send this log.");
        }
        return null;
    }

    private static void ResetTimers(Span<InputTimerModule.InputTimerData> timers)
    {
        for (int i = 0; i < timers.Length; i++)
        {
            timers[i].Timer = 0f;
        }
    }

    private static float Highest(Span<InputTimerModule.InputTimerData> timers)
    {
        var highest = 0f;
        for (int i = 0; i < timers.Length; i++)
        {
            highest = MathF.Max(highest, timers[i].Timer);
        }
        return highest;
    }
}
