using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.Interop;

namespace AutoKeyboundBrawler.Core.Queue;

internal static class Registrar
{
    private const uint BaseId = 1058751u;

    private const int LocalPlayerIndex = 0;

    public unsafe static GameObject* Find()
    {
        GameObjectManager* manager = GameObjectManager.Instance();
        if (manager == null)
        {
            return null;
        }
        Span<Pointer<GameObject>> objects = manager->Objects.IndexSorted;
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject* candidate = objects[i].Value;
            if (candidate != null && candidate->BaseId == BaseId && candidate->ObjectKind == ObjectKind.EventNpc)
            {
                return candidate;
            }
        }
        return null;
    }

    public unsafe static GameObject* LocalPlayer()
    {
        GameObjectManager* manager = GameObjectManager.Instance();
        if (manager != null)
        {
            return manager->Objects.IndexSorted[LocalPlayerIndex].Value;
        }
        return null;
    }

    public unsafe static float EdgeDistance(GameObject* from, GameObject* to)
    {
        float dx = to->Position.X - from->Position.X;
        float dz = to->Position.Z - from->Position.Z;
        return MathF.Max(0f, MathF.Sqrt(dx * dx + dz * dz) - from->HitboxRadius - to->HitboxRadius);
    }

    public unsafe static Vector3 Position(GameObject* registrar)
    {
        return new Vector3(registrar->Position.X, registrar->Position.Y, registrar->Position.Z);
    }

    public unsafe static bool IsTargeted(GameObject* registrar)
    {
        TargetSystem* targetSystem = TargetSystem.Instance();
        return targetSystem != null && targetSystem->Target == registrar;
    }

    public unsafe static void Target(GameObject* registrar)
    {
        TargetSystem* targetSystem = TargetSystem.Instance();
        if (targetSystem != null)
        {
            targetSystem->Target = registrar;
        }
    }

    public unsafe static ulong Interact(GameObject* registrar)
    {
        TargetSystem* targetSystem = TargetSystem.Instance();
        if (targetSystem != null)
        {
            return targetSystem->InteractWithObject(registrar, checkLineOfSight: false);
        }
        return 0uL;
    }
}
