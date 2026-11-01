using System;
using System.Globalization;
using System.Numerics;
using AutoKeyboundBrawler.Core.Changelog;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace AutoKeyboundBrawler.Windows.Pages;

internal sealed class ChangelogPage
{
    private const float RailWidth = 26f;

    private const float RailDotRadius = 5f;

    private const float CardGap = 14f;

    private const float CardPadX = 18f;

    private const float CardPadY = 16f;

    private const float HeaderGap = 10f;

    private const float SeparatorGap = 12f;

    private const float BulletColumn = 18f;

    private const float BulletRadius = 2.5f;

    private const float BulletGap = 8f;

    private const float PillPadX = 8f;

    private const float PillPadY = 3f;

    private const float PillGap = 10f;

    private const float RevealMs = 360f;

    private const float RevealStaggerMs = 80f;

    private const float RevealSlide = 10f;

    private const string DateFormat = "d MMM yyyy";

    private const string MetaSeparator = " · ";

    private readonly string[] versionLabels = new string[ChangelogData.Entries.Length];

    private readonly string[] metaLabels = new string[ChangelogData.Entries.Length];

    private LanguageInfo? labelsLanguage;

    private long visitTick = long.MinValue;

    private int unseenAtVisit;

    public void Draw(long shownTick)
    {
        BeginVisit(shownTick);
        RefreshLabels();
        PageHeader.Draw(Loc.T(L.Changelog.Title), Loc.T(L.Changelog.Subtitle));
        var entries = ChangelogData.Entries;
        for (var i = 0; i < entries.Length; i++)
        {
            using (Motion.PushReveal(Motion.Reveal(shownTick, RevealMs, (float)i * RevealStaggerMs)))
            {
                DrawEntry(i, i == entries.Length - 1);
            }
        }
    }

    private void BeginVisit(long shownTick)
    {
        if (visitTick == shownTick)
        {
            return;
        }
        visitTick = shownTick;
        unseenAtVisit = ChangelogData.UnseenCount(Plugin.Cfg.LastSeenChangelogVersion);
        Plugin.Cfg.MarkChangelogSeen();
    }

    private void RefreshLabels()
    {
        if (labelsLanguage == Loc.Current)
        {
            return;
        }
        labelsLanguage = Loc.Current;
        var entries = ChangelogData.Entries;
        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            versionLabels[i] = Loc.T(L.Changelog.Version, entry.Version);
            metaLabels[i] = FormatDate(entry.Date) + MetaSeparator + Loc.Plural(L.Changelog.Changes, entry.Highlights.Length);
        }
    }

    private void DrawEntry(int index, bool isLast)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var entry = ChangelogData.Entries[index];
        var isUnseen = index < unseenAtVisit;
        var isLatest = index == 0;
        var origin = ImGui.GetCursorScreenPos();
        var availWidth = ImGui.GetContentRegionAvail().X;
        var railWidth = RailWidth * scale;
        var min = new Vector2(origin.X + railWidth, origin.Y);
        var cardWidth = availWidth - railWidth;
        var padX = CardPadX * scale;
        var padY = CardPadY * scale;
        var textLeft = min.X + padX + BulletColumn * scale;
        var textWidth = MathF.Max(1f, min.X + cardWidth - padX - textLeft);
        float headlineHeight;
        using (Fonts.PushHeadline())
        {
            headlineHeight = TextDraw.LineHeight();
        }
        var bodyHeight = MeasureBody(entry.Highlights, textWidth);
        var separatorY = min.Y + padY + headlineHeight + SeparatorGap * scale;
        var cardHeight = separatorY - min.Y + SeparatorGap * scale + bodyHeight + padY;
        var max = new Vector2(min.X + cardWidth, min.Y + cardHeight);
        var drawList = ImGui.GetWindowDrawList();
        var rounding = 10f * scale;
        var dotY = min.Y + padY + headlineHeight * 0.5f;
        DrawRail(drawList, origin, dotY, cardHeight, isUnseen | isLatest, isLast);
        if (isUnseen)
        {
            Paint.Glow(drawList, min, max, rounding, Styling.AccentTeal, 0.8f);
        }
        if (isLatest)
        {
            Paint.Glass(drawList, min, max, rounding, Styling.AccentTeal, 0.08f);
            Paint.Stroke(drawList, min, max, Styling.WithAlpha(Styling.AccentTeal, isUnseen ? 0.55f : 0.3f), rounding);
        }
        else
        {
            Paint.Surface(drawList, min, max, rounding, Vector4.Zero, Styling.WithAlpha(Styling.BorderDim, 0.5f));
        }
        var cursorX = min.X + padX;
        using (Fonts.PushHeadline())
        {
            TextDraw.At(versionLabels[index], new Vector2(cursorX, min.Y + padY), isLatest ? Styling.AccentTealSoft : Styling.TextStrong);
            cursorX += TextDraw.Measure(versionLabels[index]).X + HeaderGap * scale;
        }
        if (isUnseen | isLatest)
        {
            cursorX += DrawPill(drawList, isUnseen ? L.Changelog.New : L.Changelog.Latest, cursorX, dotY, isUnseen) + PillGap * scale;
        }
        using (Fonts.PushCaption())
        {
            var meta = metaLabels[index];
            var metaSize = TextDraw.Measure(meta);
            var metaX = max.X - padX - metaSize.X;
            if (metaX > cursorX)
            {
                TextDraw.At(meta, new Vector2(metaX, dotY - metaSize.Y * 0.5f), Styling.TextMuted);
            }
        }
        Paint.Hairline(drawList, new Vector2(min.X + padX, separatorY), new Vector2(max.X - padX, separatorY));
        DrawBody(drawList, entry.Highlights, min.X + padX, textLeft, textWidth, separatorY + SeparatorGap * scale);
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(availWidth, cardHeight + (isLast ? 0f : (CardGap * scale))));
    }

    private static void DrawRail(ImDrawListPtr dl, Vector2 origin, float dotY, float cardHeight, bool highlighted, bool isLast)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var dotCenter = new Vector2(origin.X + RailWidth * scale * 0.5f - 2f * scale, dotY);
        var radius = RailDotRadius * scale;
        if (!isLast)
        {
            var lineEndY = origin.Y + cardHeight + CardGap * scale + (dotY - origin.Y) - radius;
            dl.AddLine(dotCenter + new Vector2(0f, radius + 2f * scale), new Vector2(dotCenter.X, lineEndY), Paint.Col(Styling.WithAlpha(Styling.BorderDim, 0.8f)), 1.5f * scale);
        }
        if (highlighted)
        {
            Paint.Dot(dl, dotCenter, radius, Styling.AccentTeal, 0.28f);
        }
        else
        {
            dl.AddCircle(dotCenter, radius, Paint.Col(Styling.WithAlpha(Styling.TextDim, 0.8f)), 0, 1.5f * scale);
        }
    }

    private static float DrawPill(ImDrawListPtr dl, LocString label, float x, float midY, bool filled)
    {
        var scale = ImGuiHelpers.GlobalScale;
        using (Fonts.PushCaption())
        {
            var text = TextDraw.Upper(Loc.T(label));
            var textSize = TextDraw.Measure(text);
            var min = new Vector2(x, midY - textSize.Y * 0.5f - PillPadY * scale);
            var max = new Vector2(x + textSize.X + 16f * scale, midY + textSize.Y * 0.5f + PillPadY * scale);
            if (filled)
            {
                Paint.Pill(dl, min, max, Styling.AccentTeal, Styling.WithAlpha(Styling.AccentTealSoft, 0.6f));
                TextDraw.At(text, new Vector2(min.X + PillPadX * scale, midY - textSize.Y * 0.5f), Styling.ForegroundOn(Styling.AccentTeal));
            }
            else
            {
                Paint.Pill(dl, min, max, Styling.WithAlpha(Styling.AccentTeal, 0.16f), Styling.WithAlpha(Styling.AccentTeal, 0.4f));
                TextDraw.At(text, new Vector2(min.X + PillPadX * scale, midY - textSize.Y * 0.5f), Styling.AccentTealSoft);
            }
            return max.X - min.X;
        }
    }

    private static float MeasureBody(LocString[] highlights, float textWidth)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var height = 0f;
        for (var i = 0; i < highlights.Length; i++)
        {
            if (i > 0)
            {
                height += BulletGap * scale;
            }
            height += TextDraw.MeasureWrapped(Loc.T(highlights[i]), textWidth).Y;
        }
        return height;
    }

    private static void DrawBody(ImDrawListPtr dl, LocString[] highlights, float bulletX, float textLeft, float textWidth, float top)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var lineHeight = TextDraw.LineHeight();
        var y = top;
        for (var i = 0; i < highlights.Length; i++)
        {
            if (i > 0)
            {
                y += BulletGap * scale;
            }
            var text = Loc.T(highlights[i]);
            dl.AddCircleFilled(new Vector2(bulletX + BulletRadius * scale, y + lineHeight * 0.5f), BulletRadius * scale, Paint.Col(Styling.AccentTeal));
            TextDraw.Wrapped(text, new Vector2(textLeft, y), textWidth, Styling.TextSecondary);
            y += TextDraw.MeasureWrapped(text, textWidth).Y;
        }
    }

    private static string FormatDate(string isoDate)
    {
        return DateTime.TryParse(isoDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date.ToString(DateFormat, Loc.Culture) : isoDate;
    }
}
