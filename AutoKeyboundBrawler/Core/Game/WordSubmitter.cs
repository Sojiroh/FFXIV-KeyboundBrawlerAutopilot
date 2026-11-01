using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace AutoKeyboundBrawler.Core.Game;

internal static class WordSubmitter
{
    private const string Signature = "48 89 5C 24 18 56 57 41 56 48 83 EC 40 48 8B 1D ?? ?? ?? ?? 4C 8B F2 48 8B F1";

    private const byte Confirm = 1;

    private static nint address;

    private static bool resolved;

    public static bool Available
    {
        get
        {
            Resolve();
            return address != 0;
        }
    }

    public unsafe static bool TrySubmit(nint director, uint entityId)
    {
        if (!Available || director == 0 || entityId == 0)
        {
            return false;
        }
        var objectManager = GameObjectManager.Instance();
        var target = objectManager == null ? null : objectManager->Objects.GetObjectByEntityId(entityId);
        if (target == null)
        {
            return false;
        }
        return ((delegate* unmanaged<nint, GameObject*, byte, byte>)address)(director, target, Confirm) != 0;
    }

    private static void Resolve()
    {
        if (resolved)
        {
            return;
        }
        resolved = true;
        if (!Svc.SigScanner.TryScanText(Signature, out address))
        {
            address = 0;
        }
    }
}
