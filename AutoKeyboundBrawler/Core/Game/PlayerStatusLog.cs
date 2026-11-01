using System;
using System.Globalization;
using System.Text;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace AutoKeyboundBrawler.Core.Game;

internal sealed class PlayerStatusLog
{
    private const int MaxStatuses = 60;

    private const ushort NoStatus = 0;

    private readonly ushort[] lastIds = new ushort[MaxStatuses];

    private int lastCount;

    public void Reset()
    {
        Array.Clear(lastIds);
        lastCount = 0;
    }

    public unsafe void Update()
    {
        var localPlayer = Control.GetLocalPlayer();
        if (localPlayer == null)
        {
            return;
        }
        var statusManager = localPlayer->GetStatusManager();
        if (statusManager == null)
        {
            return;
        }
        var status = statusManager->Status;
        if (!Changed(status))
        {
            return;
        }
        Remember(status);
        RunLog.Debug(Describe(status));
    }

    private bool Changed(Span<FFXIVClientStructs.FFXIV.Client.Game.Status> statuses)
    {
        var matched = 0;
        for (int i = 0; i < statuses.Length; i++)
        {
            ushort statusId = statuses[i].StatusId;
            if (statusId != NoStatus)
            {
                if (matched >= lastCount || lastIds[matched] != statusId)
                {
                    return true;
                }
                matched++;
            }
        }
        return matched != lastCount;
    }

    private void Remember(Span<FFXIVClientStructs.FFXIV.Client.Game.Status> statuses)
    {
        lastCount = 0;
        for (int i = 0; i < statuses.Length && lastCount < lastIds.Length; i++)
        {
            ushort statusId = statuses[i].StatusId;
            if (statusId != NoStatus)
            {
                lastIds[lastCount++] = statusId;
            }
        }
    }

    private static string Describe(Span<FFXIVClientStructs.FFXIV.Client.Game.Status> statuses)
    {
        var statusSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>();
        var builder = new StringBuilder("Your statuses changed:");
        var any = false;
        for (int i = 0; i < statuses.Length; i++)
        {
            var status = statuses[i];
            if (status.StatusId != NoStatus)
            {
                var name = statusSheet.GetRowOrDefault(status.StatusId) is { } row ? row.Name.ExtractText() : string.Empty;
                builder.Append(any ? ", " : " ").Append(status.StatusId.ToString(CultureInfo.InvariantCulture));
                if (name.Length > 0)
                {
                    builder.Append(' ').Append(name);
                }
                builder.Append(string.Create(CultureInfo.InvariantCulture, $" ({status.RemainingTime:0.0} s)"));
                any = true;
            }
        }
        if (!any)
        {
            return "Your statuses changed: none left.";
        }
        return builder.Append('.').ToString();
    }
}
