using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace AutoKeyboundBrawler.Core.Game;

internal sealed class BrawlerAgent
{
    [StructLayout(LayoutKind.Explicit)]
    private struct MapNode
    {
        [FieldOffset(0)]
        public unsafe MapNode* Left;

        [FieldOffset(8)]
        public unsafe MapNode* Parent;

        [FieldOffset(16)]
        public unsafe MapNode* Right;

        [FieldOffset(25)]
        public bool IsNil;

        [FieldOffset(32)]
        public uint EntityId;
    }

    public const int MaxEntries = 64;

    private const uint HudAgentId = 397u;

    private const int WordMapOffset = 40;

    private const int WordMapCountOffset = 48;

    private const int BubbleTypeOffset = 4;

    private const int WordOffset = 8;

    private const int AlternateWordOffset = 112;

    private const int CastProgressOffset = 248;

    private const int NodeValueOffset = 40;

    private const int DirectorContentTypeOffset = 3534;

    private const byte KeyboundContentType = 23;

    private const int DirectorRunningOffset = 8128;

    private const uint BarrierBubble = 2u;

    private const uint SkippedBubble = 3u;

    private const uint PriorityBubble = 4u;

    private const long MaxWordBytes = 256L;

    private const int TextsPerSlot = 2;

    private const int NoEntry = -1;

    public readonly uint[] CastActionIds = new uint[MaxEntries];

    public readonly float[] CastRemainingSeconds = new float[MaxEntries];

    private readonly uint[] entityIds = new uint[MaxEntries];

    private readonly int[] entrySlots = new int[MaxEntries];

    private readonly uint[] bubbleTypes = new uint[MaxEntries];

    private readonly float[] agentCastProgress = new float[MaxEntries];

    private readonly bool[] dead = new bool[MaxEntries];

    private readonly float[] distances = new float[MaxEntries];

    private readonly float[] castFractions = new float[MaxEntries];

    private readonly bool[] claimed = new bool[MaxEntries];

    private readonly uint[] slotEntities = new uint[MaxEntries];

    private readonly bool[] slotSeen = new bool[MaxEntries];

    private readonly string[] slotWords = new string[MaxEntries];

    private readonly string[] slotAlternates = new string[MaxEntries];

    private readonly string[] slotComparableWords = new string[MaxEntries];

    private readonly string[] slotComparableAlternates = new string[MaxEntries];

    private readonly TextSlotCache textCache = new TextSlotCache(MaxEntries * TextsPerSlot);

    private bool layoutWarned;

    private bool readLogged;

    private bool linkLogged;

    public int Count { get; private set; }

    public bool Valid { get; private set; }

    public nint Director { get; private set; }

    public BrawlerAgent()
    {
        Array.Fill(slotWords, string.Empty);
        Array.Fill(slotAlternates, string.Empty);
        Array.Fill(slotComparableWords, string.Empty);
        Array.Fill(slotComparableAlternates, string.Empty);
    }

    public void Read()
    {
        Count = 0;
        Director = ActiveDirector();
        Valid = Director != 0 && ReadMap();
        if (!Valid)
        {
            Count = 0;
        }
        ReleaseUnseenSlots();
        LogFirstRead();
    }

    public void Link(BattleSnapshot snapshot)
    {
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            snapshot.PhraseEntityId[i] = BattleSnapshot.NoEntity;
            snapshot.PhraseEntityCasting[i] = false;
            snapshot.PhraseEntityCastProgress[i] = 0f;
            snapshot.PhraseDefeated[i] = false;
            snapshot.PhraseDistance[i] = BattleSnapshot.NoDistance;
        }
        if (!Valid || Count == 0)
        {
            return;
        }
        Array.Clear(claimed, 0, Count);
        var linked = 0;
        for (int j = 0; j < snapshot.PhraseCount; j++)
        {
            var entry = EntryShowing(PhraseReadings.Comparable(snapshot.Phrases[j]));
            if (entry != NoEntry)
            {
                claimed[entry] = true;
                linked++;
                Apply(snapshot, j, entry);
            }
        }
        LogFirstLink(snapshot.PhraseCount, linked);
    }

    private void Apply(BattleSnapshot snapshot, int phrase, int entry)
    {
        var bubbleType = bubbleTypes[entry];
        var slot = entrySlots[entry];
        snapshot.PhraseEntityId[phrase] = entityIds[entry];
        snapshot.PhraseOnBarrier[phrase] = bubbleType == BarrierBubble;
        snapshot.PhraseEntityCasting[phrase] = agentCastProgress[entry] > 0f || bubbleType == PriorityBubble || CastActionIds[entry] != 0;
        snapshot.PhraseEntityCastProgress[phrase] = castFractions[entry];
        snapshot.PhraseDefeated[phrase] = IsDefeated(entry);
        snapshot.PhraseDistance[phrase] = distances[entry];
        if (PhraseReadings.ClientIsJapanese && slotComparableAlternates[slot].Length > 0)
        {
            snapshot.PhraseTyped[phrase] = slotComparableAlternates[slot];
        }
    }

    private int EntryShowing(string shown)
    {
        var firstDefeated = NoEntry;
        for (int i = 0; i < Count; i++)
        {
            if (!claimed[i] && bubbleTypes[i] != SkippedBubble && Shows(i, shown))
            {
                if (!IsDefeated(i))
                {
                    return i;
                }
                if (firstDefeated == NoEntry)
                {
                    firstDefeated = i;
                }
            }
        }
        return firstDefeated;
    }

    private bool Shows(int entry, string shown)
    {
        var slot = entrySlots[entry];
        return SameText(slotComparableWords[slot], shown) || SameText(slotComparableAlternates[slot], shown);
    }

    private static bool SameText(string word, string shown) => word.Length > 0 && word.Equals(shown, StringComparison.OrdinalIgnoreCase);

    private bool IsDefeated(int entry) => dead[entry] && bubbleTypes[entry] != BarrierBubble;

    private unsafe static nint ActiveDirector()
    {
        var eventFramework = EventFramework.Instance();
        var director = (byte*)(eventFramework == null ? null : eventFramework->GetInstanceContentDirector());
        if (director == null || director[DirectorContentTypeOffset] != KeyboundContentType || director[DirectorRunningOffset] == 0)
        {
            return 0;
        }
        return (nint)director;
    }

    private unsafe bool ReadMap()
    {
        var agentModule = AgentModule.Instance();
        var hudAgent = (byte*)(agentModule == null ? null : agentModule->GetAgentByInternalId((AgentId)HudAgentId));
        if (hudAgent == null)
        {
            return Fail("the HUD agent is missing");
        }
        var head = *(MapNode**)(hudAgent + WordMapOffset);
        var size = *(long*)(hudAgent + WordMapCountOffset);
        if (head == null)
        {
            if (size != 0L)
            {
                return Fail("the word map has entries but no head");
            }
            return true;
        }
        if (!head->IsNil || size < 0 || size > MaxEntries)
        {
            return Fail("the word map header is out of range");
        }
        if (!Walk(head, (int)size))
        {
            return Fail("an entry of the word map is unreadable");
        }
        return true;
    }

    private unsafe bool Walk(MapNode* head, int size)
    {
        var localPlayer = (GameObject*)Control.GetLocalPlayer();
        var objects = GameObjectManager.Instance();
        Span<nint> stack = stackalloc nint[MaxEntries];
        var depth = 0;
        var node = head->Parent;
        while (true)
        {
            while (node != null && !node->IsNil)
            {
                if (depth == MaxEntries)
                {
                    return false;
                }
                stack[depth++] = (nint)node;
                node = node->Left;
            }
            if (node == null)
            {
                return false;
            }
            if (depth == 0)
            {
                return Count == size;
            }
            node = (MapNode*)stack[--depth];
            if (Count == size || !ReadEntry(node, objects, localPlayer))
            {
                return false;
            }
            node = node->Right;
        }
    }

    private unsafe bool ReadEntry(MapNode* node, GameObjectManager* objects, GameObject* player)
    {
        uint entityId = node->EntityId;
        var entryValue = (byte*)node + NodeValueOffset;
        var word = (Utf8String*)(entryValue + WordOffset);
        var alternate = (Utf8String*)(entryValue + AlternateWordOffset);
        if (entityId == 0 || !IsReadable(word) || !IsReadable(alternate))
        {
            return false;
        }
        var entry = Count++;
        var slot = SlotFor(entityId);
        slotSeen[slot] = true;
        entityIds[entry] = entityId;
        entrySlots[entry] = slot;
        bubbleTypes[entry] = *(uint*)(entryValue + BubbleTypeOffset);
        agentCastProgress[entry] = *(float*)(entryValue + CastProgressOffset);
        StoreText(slot * TextsPerSlot, word, slotWords, slotComparableWords, slot);
        StoreText(slot * TextsPerSlot + 1, alternate, slotAlternates, slotComparableAlternates, slot);
        ReadObject(entry, objects == null ? null : objects->Objects.GetObjectByEntityId(entityId), player);
        return true;
    }

    private unsafe static bool IsReadable(Utf8String* text)
    {
        long bufUsed = text->BufUsed;
        return bufUsed >= 0 && bufUsed <= MaxWordBytes && bufUsed <= text->BufSize && (bufUsed == 0L || (byte*)text->StringPtr != null);
    }

    private unsafe void StoreText(int textSlot, Utf8String* text, string[] decoded, string[] comparable, int slot)
    {
        var decodedText = textCache.Decode(textSlot, text->AsSpan(), singleLine: true);
        if ((object)decodedText == decoded[slot])
        {
            return;
        }
        decoded[slot] = decodedText;
        comparable[slot] = PhraseReadings.Comparable(decodedText);
    }

    private unsafe void ReadObject(int entry, GameObject* gameObject, GameObject* player)
    {
        dead[entry] = false;
        distances[entry] = BattleSnapshot.NoDistance;
        CastActionIds[entry] = 0u;
        CastRemainingSeconds[entry] = 0f;
        castFractions[entry] = 0f;
        if (gameObject == null)
        {
            return;
        }
        dead[entry] = gameObject->IsDead();
        if (player != null)
        {
            distances[entry] = Vector3.Distance(player->Position, gameObject->Position);
        }
        if (gameObject->ObjectKind != ObjectKind.BattleNpc)
        {
            return;
        }
        var castInfo = ((Character*)gameObject)->GetCastInfo();
        if (castInfo == null || !castInfo->IsCasting || castInfo->TotalCastTime <= 0f)
        {
            return;
        }
        CastActionIds[entry] = castInfo->ActionId;
        CastRemainingSeconds[entry] = MathF.Max(0f, castInfo->TotalCastTime - castInfo->CurrentCastTime);
        castFractions[entry] = Math.Clamp(castInfo->CurrentCastTime / castInfo->TotalCastTime, 0f, 1f);
    }

    private int SlotFor(uint entityId)
    {
        var firstFree = -1;
        var firstUnseen = -1;
        for (int i = 0; i < MaxEntries; i++)
        {
            if (slotEntities[i] == entityId)
            {
                return i;
            }
            if (firstFree == -1 && slotEntities[i] == 0)
            {
                firstFree = i;
            }
            else if (firstUnseen == -1 && !slotSeen[i])
            {
                firstUnseen = i;
            }
        }
        var slot = firstFree != -1 ? firstFree : firstUnseen;
        slotEntities[slot] = entityId;
        return slot;
    }

    private void ReleaseUnseenSlots()
    {
        for (int i = 0; i < MaxEntries; i++)
        {
            if (!slotSeen[i])
            {
                slotEntities[i] = 0u;
            }
            slotSeen[i] = false;
        }
    }

    private bool Fail(string reason)
    {
        if (!layoutWarned)
        {
            layoutWarned = true;
            RunLog.Warning("The game's word data does not read as expected (" + reason + "), so phrases are read from the screen only. Please send this log.");
        }
        return false;
    }

    private void LogFirstRead()
    {
        if (Director == 0)
        {
            readLogged = false;
            linkLogged = false;
            return;
        }
        if (readLogged || !Valid || Count == 0)
        {
            return;
        }
        readLogged = true;
        var builder = new StringBuilder();
        for (int i = 0; i < Count; i++)
        {
            var slot = entrySlots[i];
            builder.Append(CultureInfo.InvariantCulture, $"{(i > 0 ? "; " : string.Empty)}{entityIds[i]:X8} bubble {bubbleTypes[i]} \"{slotWords[slot]}\"/\"{slotAlternates[slot]}\" cast {agentCastProgress[i]:0.##} action {CastActionIds[i]}");
        }
        RunLog.Debug($"Game word data: {Count} entries: {builder}.");
    }

    private void LogFirstLink(int phrases, int linked)
    {
        if (linkLogged || phrases == 0)
        {
            return;
        }
        linkLogged = true;
        RunLog.Debug($"Matched {linked} of {phrases} phrases on screen to the game's word data.");
    }
}
