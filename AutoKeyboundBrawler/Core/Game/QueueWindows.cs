using System;
using ECommons;
using ECommons.Automation.UIInput;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoKeyboundBrawler.Core.Game;

internal static class QueueWindows
{
    private const uint RegisterButtonNodeId = 9u;

    private const uint LeaveButtonNodeId = 48u;

    private const int FirstEntry = 0;

    public unsafe static bool ResultsOpen => IsOpen(Find("KTGResult"u8));

    public unsafe static bool EntryOpen => IsOpen(Find("KTGEnterDialogue"u8));

    public unsafe static bool TalkOpen => IsOpen(Find("Talk"u8));

    public unsafe static bool YesnoOpen => IsOpen(Find("SelectYesno"u8));

    public unsafe static bool MenuOpen => IsOpen(Find("SelectString"u8));

    public unsafe static bool ConfirmOpen => IsOpen(Find("ContentsFinderConfirm"u8));

    public unsafe static bool PressLeave()
    {
        var addon = Find("KTGResult"u8);
        return IsReady(addon) && Press(addon, addon->GetComponentButtonById(LeaveButtonNodeId));
    }

    public unsafe static bool PressRegister()
    {
        var addon = Find("KTGEnterDialogue"u8);
        return IsReady(addon) && Press(addon, addon->GetComponentButtonById(RegisterButtonNodeId));
    }

    public unsafe static bool PressCommence()
    {
        var addon = Find("ContentsFinderConfirm"u8);
        return IsReady(addon) && Press(addon, ((AddonContentsFinderConfirm*)addon)->CommenceButton);
    }

    public unsafe static bool AdvanceTalk()
    {
        var addon = Find("Talk"u8);
        if (!IsReady(addon))
        {
            return false;
        }
        new AddonMaster.Talk(addon).Click();
        return true;
    }

    public unsafe static bool AnswerYes(out string prompt)
    {
        prompt = string.Empty;
        var addon = Find("SelectYesno"u8);
        if (!IsReady(addon))
        {
            return false;
        }
        var yesno = (AddonSelectYesno*)addon;
        if (yesno->PromptText != null)
        {
            prompt = yesno->PromptText->NodeText.GetText();
        }
        return Press(addon, yesno->YesButton);
    }

    public unsafe static bool ChooseFirstEntry(out string entry)
    {
        entry = string.Empty;
        var addon = Find("SelectString"u8);
        if (!IsReady(addon))
        {
            return false;
        }
        var selectString = new AddonMaster.SelectString(addon);
        if (selectString.EntryCount == 0)
        {
            return false;
        }
        var entries = selectString.Entries;
        entry = entries[FirstEntry].Text;
        entries[FirstEntry].Select();
        return true;
    }

    private unsafe static AtkUnitBase* Find(ReadOnlySpan<byte> name)
    {
        var manager = RaptureAtkUnitManager.Instance();
        if (manager != null)
        {
            return manager->GetAddonByName(name);
        }
        return null;
    }

    private unsafe static bool IsOpen(AtkUnitBase* addon)
    {
        return addon != null && addon->IsVisible;
    }

    private unsafe static bool IsReady(AtkUnitBase* addon)
    {
        return addon != null && GenericHelpers.IsAddonReady(addon);
    }

    private unsafe static bool Press(AtkUnitBase* addon, AtkComponentButton* button)
    {
        if (button == null || !button->IsEnabled || button->OwnerNode == null || !button->OwnerNode->AtkResNode.IsVisible())
        {
            return false;
        }
        button->ClickAddonButton(addon);
        return true;
    }
}
