using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoKeyboundBrawler.Core.Game;

internal static class Wallet
{
    public const int Unknown = -1;

    public const int MgcCap = 10000;

    private const uint MgpItemId = 29u;

    private const uint MgcItemId = 52322u;

    public static int Mgp => Count(MgpItemId);

    public static int Mgc => Count(MgcItemId);

    private unsafe static int Count(uint itemId)
    {
        var inventoryManager = InventoryManager.Instance();
        if (inventoryManager != null)
        {
            return inventoryManager->GetInventoryItemCount(itemId, isHq: false, checkEquipped: false, checkArmory: false, 0);
        }
        return Unknown;
    }
}
