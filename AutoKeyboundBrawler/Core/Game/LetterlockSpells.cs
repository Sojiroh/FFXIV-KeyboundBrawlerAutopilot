using System;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace AutoKeyboundBrawler.Core.Game;

internal static class LetterlockSpells
{
    private static readonly uint[] actionIds = new uint[3] { 49529u, 49532u, 49702u };

    private const uint WatchStatusId = 5346u;

    private static string[]? names;

    public static ReadOnlySpan<uint> ActionIds => actionIds;

    public static bool IsLetterlock(uint actionId) => actionIds.AsSpan().Contains(actionId);

    public unsafe static bool PlayerWatched()
    {
        var localPlayer = Control.GetLocalPlayer();
        if (localPlayer == null)
        {
            return false;
        }
        var statusManager = localPlayer->GetStatusManager();
        return statusManager != null && statusManager->HasStatus(WatchStatusId);
    }

    public static string NameOf(uint actionId)
    {
        var index = actionIds.AsSpan().IndexOf(actionId);
        if (index >= 0)
        {
            return Names()[index];
        }
        return string.Empty;
    }

    public static bool Matches(string spell)
    {
        if (spell.Length == 0)
        {
            return false;
        }
        var spellNames = Names();
        for (int i = 0; i < spellNames.Length; i++)
        {
            if (spellNames[i].Length > 0 && spellNames[i].Equals(spell, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static string[] Names() => names ??= LoadNames();

    private static string[] LoadNames()
    {
        var actionSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>();
        var result = new string[actionIds.Length];
        for (int i = 0; i < actionIds.Length; i++)
        {
            result[i] = actionSheet.GetRowOrDefault(actionIds[i]) is { } row ? row.Name.ExtractText() : string.Empty;
        }
        return result;
    }
}
