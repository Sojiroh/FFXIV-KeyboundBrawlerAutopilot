using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoKeyboundBrawler.Core.Game;

internal static class KeyboundUi
{
    private const int MaxAddonInstances = 16;

    private const ushort FirstComponentNodeType = 1000;

    private const byte PayloadStart = 2;

    private const byte PayloadEnd = 3;

    private const uint LargePhraseTextNodeId = 20u;

    private const uint SmallPhraseTextNodeId = 22u;

    private const uint ObscuringOverlayNodeId = 10u;

    private const uint CastInterruptedTextNodeId = 3u;

    private const uint CastNameTextNodeId = 4u;

    private const uint CastHitsTextNodeId = 5u;

    private const uint CastFillImageNodeId = 6u;

    private const float CastFillFullWidth = 128f;

    private const float GaugeReachX = 90f;

    private const float GaugeReachAbove = 30f;

    private const float GaugeReachBelow = 150f;

    private const float CastReach = 450f;

    private const float BarrierZoneAbove = 260f;

    private const float BarrierZoneBelow = 120f;

    public unsafe static void Read(BattleSnapshot snapshot, TextSlotCache cache, PhraseMemory memory, BrawlerAgent agent)
    {
        snapshot.Clear();
        var unitManager = RaptureAtkUnitManager.Instance();
        if (unitManager == null)
        {
            return;
        }
        ReadInput(unitManager, snapshot, cache);
        ReadPhrases(unitManager, snapshot, cache, memory);
        ReadCasts(unitManager, snapshot, cache);
        ReadGauges(unitManager, snapshot);
        LinkPhrases(snapshot, agent);
    }

    private unsafe static void ReadInput(RaptureAtkUnitManager* unitManager, BattleSnapshot snapshot, TextSlotCache cache)
    {
        var addon = unitManager->GetAddonByName("KTGInput"u8);
        if (addon == null || !addon->IsVisible)
        {
            return;
        }
        var textInput = FindTextInput(addon);
        if (textInput == null)
        {
            return;
        }
        snapshot.InputVisible = true;
        snapshot.InputScreenY = addon->RootNode == null ? 0f : CenterOf(addon->RootNode, out _).Y;
        snapshot.InputText = cache.Decode(BattleSnapshot.InputSlot, textInput->RawString.AsSpan(), singleLine: false);
        ReadFocus(textInput, snapshot);
    }

    private unsafe static void ReadFocus(AtkComponentTextInput* textInput, BattleSnapshot snapshot)
    {
        if (ImGui.GetIO().WantTextInput)
        {
            snapshot.OtherInputFocused = true;
            return;
        }
        var atkModule = RaptureAtkModule.Instance();
        if (atkModule == null || !atkModule->IsTextInputActive())
        {
            return;
        }
        var focused = atkModule->AtkModule.TextInput.TargetTextInputEventInterface == &textInput->AtkTextInputEventInterface;
        snapshot.InputFocused = focused;
        snapshot.OtherInputFocused = !focused;
    }

    public unsafe static bool FocusInput()
    {
        var unitManager = RaptureAtkUnitManager.Instance();
        var stage = AtkStage.Instance();
        if (unitManager == null || stage == null || stage->AtkInputManager == null)
        {
            return false;
        }
        var addon = unitManager->GetAddonByName("KTGInput"u8);
        if (addon == null || !addon->IsVisible || !addon->IsReady)
        {
            return false;
        }
        var textInput = FindTextInput(addon);
        if (textInput == null || textInput->CollisionNode == null)
        {
            return false;
        }
        var focusNode = &textInput->CollisionNode->AtkResNode;
        addon->Focus();
        addon->SetFocusNode(focusNode);
        addon->SetComponentFocusNode((AtkComponentBase*)textInput);
        var result = stage->AtkInputManager->SetFocus(focusNode, addon, 0);
        textInput->IsActive = true;
        return result;
    }

    private unsafe static AtkComponentTextInput* FindTextInput(AtkUnitBase* addon)
    {
        var nodeListCount = addon->UldManager.NodeListCount;
        for (int i = 0; i < nodeListCount; i++)
        {
            var component = ComponentOf(addon->UldManager.NodeList[i]);
            if (component != null && component->GetComponentType() == ComponentType.TextInput)
            {
                return (AtkComponentTextInput*)component;
            }
        }
        return null;
    }

    private unsafe static void ReadPhrases(RaptureAtkUnitManager* unitManager, BattleSnapshot snapshot, TextSlotCache cache, PhraseMemory memory)
    {
        Span<nint> seenAddons = stackalloc nint[MaxAddonInstances];
        for (int i = 1; i <= MaxAddonInstances; i++)
        {
            var addon = unitManager->GetAddonByName("KTGEWord"u8, i);
            if (addon == null || seenAddons.Slice(0, i - 1).Contains((nint)addon))
            {
                return;
            }
            seenAddons[i - 1] = (nint)addon;
            if (addon->IsVisible && !ReadAddonPhrases(addon, snapshot, cache, memory))
            {
                return;
            }
        }
    }

    private unsafe static bool ReadAddonPhrases(AtkUnitBase* addon, BattleSnapshot snapshot, TextSlotCache cache, PhraseMemory memory)
    {
        var nodeListCount = addon->UldManager.NodeListCount;
        for (int i = 0; i < nodeListCount; i++)
        {
            var component = ComponentOf(addon->UldManager.NodeList[i]);
            if (component == null)
            {
                continue;
            }
            var phraseCount = snapshot.PhraseCount;
            var textNode = VisiblePhraseNode(component);
            string? text;
            if (textNode != null)
            {
                text = cache.Decode(phraseCount, textNode->NodeText.AsSpan(), singleLine: true);
                memory.Remember((nint)component, text);
            }
            else
            {
                if (!IsObscured(component))
                {
                    continue;
                }
                textNode = HiddenPhraseNode(component);
                text = textNode != null ? cache.Decode(phraseCount, textNode->NodeText.AsSpan(), singleLine: true) : memory.Recall((nint)component);
                if (textNode == null)
                {
                    textNode = component->GetTextNodeById(LargePhraseTextNodeId);
                }
            }
            if (textNode != null && !string.IsNullOrEmpty(text))
            {
                var center = CenterOf(&textNode->AtkResNode, out var scale);
                snapshot.Phrases[phraseCount] = text;
                snapshot.PhraseTyped[phraseCount] = PhraseReadings.TypedForm(text);
                snapshot.PhraseScreenX[phraseCount] = center.X;
                snapshot.PhraseScreenY[phraseCount] = center.Y;
                snapshot.PhraseScale[phraseCount] = scale;
                snapshot.PhraseCount++;
                if (snapshot.PhraseCount == BattleSnapshot.MaxPhrases)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private unsafe static bool IsObscured(AtkComponentBase* component)
    {
        var overlay = component->GetNodeById(ObscuringOverlayNodeId);
        return overlay != null && IsShown(overlay);
    }

    private unsafe static AtkTextNode* HiddenPhraseNode(AtkComponentBase* component)
    {
        var largeNode = component->GetTextNodeById(LargePhraseTextNodeId);
        if (HoldsText(largeNode))
        {
            return largeNode;
        }
        var smallNode = component->GetTextNodeById(SmallPhraseTextNodeId);
        return HoldsText(smallNode) ? smallNode : null;
    }

    private unsafe static bool HoldsText(AtkTextNode* textNode) => textNode != null && !textNode->NodeText.AsSpan().IsEmpty;

    private unsafe static AtkTextNode* VisiblePhraseNode(AtkComponentBase* component)
    {
        var largeNode = component->GetTextNodeById(LargePhraseTextNodeId);
        if (IsShown(largeNode))
        {
            return largeNode;
        }
        var smallNode = component->GetTextNodeById(SmallPhraseTextNodeId);
        return IsShown(smallNode) ? smallNode : null;
    }

    private unsafe static void ReadCasts(RaptureAtkUnitManager* unitManager, BattleSnapshot snapshot, TextSlotCache cache)
    {
        Span<nint> seenAddons = stackalloc nint[MaxAddonInstances];
        for (int i = 1; i <= MaxAddonInstances; i++)
        {
            var addon = unitManager->GetAddonByName("KTGEnemyCastBar"u8, i);
            if (addon == null || seenAddons.Slice(0, i - 1).Contains((nint)addon))
            {
                return;
            }
            seenAddons[i - 1] = (nint)addon;
            if (!addon->IsVisible)
            {
                continue;
            }
            var nodeListCount = addon->UldManager.NodeListCount;
            for (int j = 0; j < nodeListCount; j++)
            {
                var component = ComponentOf(addon->UldManager.NodeList[j]);
                if (component != null && !ReadCast(component, snapshot, cache))
                {
                    return;
                }
            }
        }
    }

    private unsafe static bool ReadCast(AtkComponentBase* component, BattleSnapshot snapshot, TextSlotCache cache)
    {
        var fillImage = component->GetImageNodeById(CastFillImageNodeId);
        if (fillImage == null || !IsShown(&fillImage->AtkResNode))
        {
            return true;
        }
        var castCount = snapshot.CastCount;
        var center = CenterOf(&fillImage->AtkResNode, out _);
        snapshot.CastIds[castCount] = (nint)component;
        snapshot.CastNames[castCount] = ReadText(component->GetTextNodeById(CastNameTextNodeId), BattleSnapshot.FirstCastSlot + castCount, cache);
        snapshot.CastScreenX[castCount] = center.X;
        snapshot.CastScreenY[castCount] = center.Y;
        snapshot.CastProgress[castCount] = Math.Clamp(fillImage->AtkResNode.Width * fillImage->AtkResNode.ScaleX / CastFillFullWidth, 0f, 1f);
        snapshot.CastHits[castCount] = ReadNumber(component->GetTextNodeById(CastHitsTextNodeId));
        snapshot.CastInterrupted[castCount] = IsShown(component->GetTextNodeById(CastInterruptedTextNodeId));
        snapshot.CastCount++;
        return snapshot.CastCount < BattleSnapshot.MaxCasts;
    }

    private unsafe static void ReadGauges(RaptureAtkUnitManager* unitManager, BattleSnapshot snapshot)
    {
        Span<nint> seenAddons = stackalloc nint[MaxAddonInstances];
        for (int i = 1; i <= MaxAddonInstances; i++)
        {
            var addon = unitManager->GetAddonByName("KTGIGaugeHp"u8, i);
            if (addon == null || seenAddons.Slice(0, i - 1).Contains((nint)addon))
            {
                return;
            }
            seenAddons[i - 1] = (nint)addon;
            if (!addon->IsVisible)
            {
                continue;
            }
            var nodeListCount = addon->UldManager.NodeListCount;
            for (int j = 0; j < nodeListCount; j++)
            {
                var node = addon->UldManager.NodeList[j];
                var component = ComponentOf(node);
                if (component != null && IsShown(node))
                {
                    var gaugeCount = snapshot.GaugeCount;
                    var center = CenterOf(node, out _);
                    snapshot.GaugeScreenX[gaugeCount] = center.X;
                    snapshot.GaugeScreenY[gaugeCount] = center.Y;
                    snapshot.GaugeSignature[gaugeCount] = DrawnSignature(component);
                    snapshot.GaugeCount++;
                    if (snapshot.GaugeCount == BattleSnapshot.MaxGauges)
                    {
                        return;
                    }
                }
            }
        }
    }

    private static void LinkPhrases(BattleSnapshot snapshot, BrawlerAgent agent)
    {
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            float scale = snapshot.PhraseScale[i];
            snapshot.PhraseGauge[i] = GaugeUnder(snapshot, i, scale);
            snapshot.PhraseOnBarrier[i] = snapshot.GaugeCount > 0 && snapshot.PhraseGauge[i] == BattleSnapshot.NoGauge && InBarrierZone(snapshot, i);
            snapshot.PhraseCast[i] = BattleSnapshot.NoCast;
        }
        agent.Link(snapshot);
        for (int j = 0; j < snapshot.CastCount; j++)
        {
            LinkCast(snapshot, j);
        }
    }

    private static void LinkCast(BattleSnapshot snapshot, int cast)
    {
        var nearest = NearestPhrase(snapshot, cast, knownCastersOnly: true);
        if (nearest < 0)
        {
            nearest = NearestPhrase(snapshot, cast, knownCastersOnly: false);
        }
        if (nearest >= 0 && (snapshot.PhraseCast[nearest] == BattleSnapshot.NoCast || IsCloser(snapshot, cast, snapshot.PhraseCast[nearest], nearest)))
        {
            snapshot.PhraseCast[nearest] = cast;
        }
    }

    private static int NearestPhrase(BattleSnapshot snapshot, int cast, bool knownCastersOnly)
    {
        var castPosition = new Vector2(snapshot.CastScreenX[cast], snapshot.CastScreenY[cast]);
        var nearest = -1;
        var nearestDistance = float.MaxValue;
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            if (!snapshot.PhraseOnBarrier[i] && (!knownCastersOnly || snapshot.PhraseEntityCasting[i]))
            {
                var distance = Vector2.Distance(castPosition, new Vector2(snapshot.PhraseScreenX[i], snapshot.PhraseScreenY[i]));
                if (distance <= CastReach * snapshot.PhraseScale[i] && distance < nearestDistance)
                {
                    nearest = i;
                    nearestDistance = distance;
                }
            }
        }
        return nearest;
    }

    private static bool IsCloser(BattleSnapshot snapshot, int cast, int other, int phrase)
    {
        var phrasePosition = new Vector2(snapshot.PhraseScreenX[phrase], snapshot.PhraseScreenY[phrase]);
        return Vector2.Distance(new Vector2(snapshot.CastScreenX[cast], snapshot.CastScreenY[cast]), phrasePosition) < Vector2.Distance(new Vector2(snapshot.CastScreenX[other], snapshot.CastScreenY[other]), phrasePosition);
    }

    public static bool InBarrierZone(BattleSnapshot snapshot, int phrase)
    {
        var scale = snapshot.PhraseScale[phrase];
        if (!snapshot.InputVisible)
        {
            return false;
        }
        var offsetY = snapshot.PhraseScreenY[phrase] - snapshot.InputScreenY;
        return offsetY >= -BarrierZoneAbove * scale && offsetY <= BarrierZoneBelow * scale;
    }

    private static int GaugeUnder(BattleSnapshot snapshot, int phrase, float scale)
    {
        var phraseX = snapshot.PhraseScreenX[phrase];
        var phraseY = snapshot.PhraseScreenY[phrase];
        for (int i = 0; i < snapshot.GaugeCount; i++)
        {
            var offsetX = MathF.Abs(snapshot.GaugeScreenX[i] - phraseX);
            var offsetY = snapshot.GaugeScreenY[i] - phraseY;
            if (offsetX <= GaugeReachX * scale && offsetY >= -GaugeReachAbove * scale && offsetY <= GaugeReachBelow * scale)
            {
                return i;
            }
        }
        return BattleSnapshot.NoGauge;
    }

    private unsafe static float DrawnSignature(AtkComponentBase* component)
    {
        if (component == null)
        {
            return 0f;
        }
        var signature = 0f;
        var nodeListCount = component->UldManager.NodeListCount;
        for (int i = 0; i < nodeListCount; i++)
        {
            var node = component->UldManager.NodeList[i];
            if (node != null && node->IsVisible())
            {
                signature += node->Width * node->ScaleX + node->Height * node->ScaleY;
            }
        }
        return signature;
    }

    private unsafe static string ReadText(AtkTextNode* textNode, int slot, TextSlotCache cache)
    {
        return IsShown(textNode) ? cache.Decode(slot, textNode->NodeText.AsSpan(), singleLine: true) : string.Empty;
    }

    private unsafe static int ReadNumber(AtkTextNode* textNode)
    {
        if (!IsShown(textNode))
        {
            return 0;
        }
        var text = textNode->NodeText.AsSpan();
        var number = 0;
        var inPayload = false;
        for (int i = 0; i < text.Length; i++)
        {
            var character = text[i];
            if (character == PayloadStart)
            {
                inPayload = true;
            }
            else if (character == PayloadEnd)
            {
                inPayload = false;
            }
            else if (!inPayload && character >= '0' && character <= '9')
            {
                number = number * 10 + (character - '0');
            }
        }
        return number;
    }

    private unsafe static bool IsShown(AtkTextNode* textNode)
    {
        return textNode != null && !textNode->NodeText.AsSpan().IsEmpty && IsShown(&textNode->AtkResNode);
    }

    private unsafe static bool IsShown(AtkResNode* node)
    {
        for (var current = node; current != null; current = current->ParentNode)
        {
            if (!current->IsVisible())
            {
                return false;
            }
        }
        return true;
    }

    private unsafe static Vector2 CenterOf(AtkResNode* node, out float scale)
    {
        var scaleX = 1f;
        var scaleY = 1f;
        for (var current = node; current != null; current = current->ParentNode)
        {
            scaleX *= current->ScaleX;
            scaleY *= current->ScaleY;
        }
        scale = scaleY;
        return new Vector2(node->ScreenX + node->Width * scaleX * 0.5f, node->ScreenY + node->Height * scaleY * 0.5f);
    }

    private unsafe static AtkComponentBase* ComponentOf(AtkResNode* node)
    {
        if (node == null || (int)node->Type < FirstComponentNodeType)
        {
            return null;
        }
        return ((AtkComponentNode*)node)->Component;
    }
}
