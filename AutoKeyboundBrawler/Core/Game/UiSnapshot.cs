using System;
using System.Globalization;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Text.ReadOnly;

namespace AutoKeyboundBrawler.Core.Game;

internal static class UiSnapshot
{
    private const string AddonMarker = "KTG";

    private const ushort FirstComponentNodeType = 1000;

    private const string NodeSeparator = " | ";

    private const char DepthMarker = '>';

    public unsafe static void Write(string reason)
    {
        var manager = RaptureAtkUnitManager.Instance();
        if (manager == null)
        {
            return;
        }
        var builder = new StringBuilder();
        var unitList = &manager->AtkUnitManager.AllLoadedUnitsList;
        var entries = unitList->Entries;
        var windowCount = 0;
        for (int i = 0; i < unitList->Count && i < entries.Length; i++)
        {
            var addon = entries[i].Value;
            if (addon != null && addon->IsVisible)
            {
                var nameString = addon->NameString;
                if (nameString.Contains(AddonMarker, StringComparison.Ordinal))
                {
                    windowCount++;
                    builder.Clear();
                    builder.Append(nameString).Append(" at ").Append(addon->X)
                        .Append(',')
                        .Append(addon->Y)
                        .Append(" scale ")
                        .Append(Format(addon->Scale))
                        .Append(':');
                    AppendNodes(builder, &addon->UldManager, 1);
                    RunLog.Debug(builder.ToString());
                }
            }
        }
        RunLog.Info($"UI snapshot ({reason}): {windowCount} brawler windows, one line each above.");
    }

    private unsafe static void AppendNodes(StringBuilder builder, AtkUldManager* manager, int depth)
    {
        for (int i = 0; i < manager->NodeListCount; i++)
        {
            var node = manager->NodeList[i];
            if (node == null || !node->IsVisible())
            {
                continue;
            }
            builder.Append(NodeSeparator).Append(DepthMarker, depth).Append('#')
                .Append(node->NodeId)
                .Append(' ')
                .Append(node->Type)
                .Append(" at ")
                .Append(Format(node->ScreenX))
                .Append(',')
                .Append(Format(node->ScreenY))
                .Append(" size ")
                .Append(node->Width)
                .Append('x')
                .Append(node->Height)
                .Append(" scale ")
                .Append(Format(node->ScaleX))
                .Append(',')
                .Append(Format(node->ScaleY))
                .Append(" alpha ")
                .Append(node->Alpha_2);
            AppendKindDetail(builder, node);
            if ((int)node->Type >= FirstComponentNodeType)
            {
                var component = ((AtkComponentNode*)node)->Component;
                if (component != null)
                {
                    AppendNodes(builder, &component->UldManager, depth + 1);
                }
            }
        }
    }

    private unsafe static void AppendKindDetail(StringBuilder builder, AtkResNode* node)
    {
        switch (node->Type)
        {
            case NodeType.Text:
                builder.Append(" text \"").Append(new ReadOnlySeStringSpan(((AtkTextNode*)node)->NodeText.AsSpan()).ExtractText()).Append('"');
                break;
            case NodeType.Image:
                builder.Append(" part ").Append(((AtkImageNode*)node)->PartId);
                break;
            case NodeType.NineGrid:
                builder.Append(" part ").Append(((AtkNineGridNode*)node)->PartId);
                break;
        }
    }

    private static string Format(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
