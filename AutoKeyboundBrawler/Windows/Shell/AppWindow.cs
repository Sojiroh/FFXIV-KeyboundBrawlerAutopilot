using System;
using System.Numerics;
using AutoKeyboundBrawler.Windows.Pages;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace AutoKeyboundBrawler.Windows.Shell;

public sealed class AppWindow : Window, IDisposable
{
    public enum Page
    {
        Brawl,
        Settings,
        Log,
        Changelog,
        About
    }

    private const float PageRevealMs = 260f;

    private const float PageSlide = 12f;

    private const float CollapseMs = 280f;

    private const float GripSize = 12f;

    private const float GripInset = 4f;

    private const float MinimumBodyHeight = 260f;

    private const ImGuiWindowFlags BaseFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse;

    private static readonly Vector2 DefaultSize = new Vector2(1040f, 780f);

    private static readonly Vector2 Unbounded = new Vector2(float.MaxValue, float.MaxValue);

    private static readonly WindowSizeConstraints CompactConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(HeaderBar.MinimumWidth, 52f),
        MaximumSize = Unbounded
    };

    private static readonly WindowSizeConstraints ExpandedConstraints = new WindowSizeConstraints
    {
        MinimumSize = new Vector2(HeaderBar.MinimumWidth, 390f),
        MaximumSize = Unbounded
    };

    private readonly Plugin plugin;

    private readonly BrawlPage brawlPage = new BrawlPage();

    private readonly SettingsPage settingsPage = new SettingsPage();

    private readonly LogPage logPage = new LogPage();

    private readonly ChangelogPage changelogPage = new ChangelogPage();

    private readonly AboutPage aboutPage = new AboutPage();

    private Page page;

    private long pageShownTick = Environment.TickCount64;

    private bool resetScroll;

    private bool compact;

    private float collapse;

    private float collapseFrom;

    private long collapseTick;

    private bool sizeDriven;

    private Vector2 expandedSize = DefaultSize;

    private IDisposable? chrome;

    private IDisposable? bodyFont;

    public Page Current => page;

    public bool Compact => compact;

    public AppWindow(Plugin plugin)
        : base("Keybound Brawler Autopilot###KeyboundBrawlerAutopilotMain", BaseFlags)
    {
        this.plugin = plugin;
        Size = DefaultSize;
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = ExpandedConstraints;
        AllowPinning = false;
        AllowClickthrough = false;
    }

    public void Dispose()
    {
    }

    public void Show(Page target)
    {
        if (page != target)
        {
            page = target;
            pageShownTick = Environment.TickCount64;
            resetScroll = true;
        }
        if (compact)
        {
            ToggleCompact();
        }
        IsOpen = true;
    }

    public void TogglePage(Page target)
    {
        if (IsOpen && page == target && !compact)
        {
            IsOpen = false;
        }
        else
        {
            Show(target);
        }
    }

    public void ToggleCompact()
    {
        compact = !compact;
        collapseFrom = collapse;
        collapseTick = Environment.TickCount64;
    }

    public override void OnOpen()
    {
        pageShownTick = Environment.TickCount64;
    }

    public override void PreDraw()
    {
        bodyFont = Fonts.PushBody();
        chrome = Styling.PushChrome(Vector2.Zero);
        AdvanceCollapse();
        bool wasSizeDriven = sizeDriven;
        sizeDriven = compact || collapse > 0f;
        Flags = (sizeDriven ? (ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollWithMouse) : BaseFlags);
        SizeConstraints = (sizeDriven ? CompactConstraints : ExpandedConstraints);
        if (!plugin.Configuration.DragWindowFromAnywhere)
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
        if (sizeDriven)
        {
            float y = expandedSize.Y + (52f - expandedSize.Y) * collapse;
            Size = new Vector2(expandedSize.X, y);
            SizeCondition = ImGuiCond.Always;
        }
        else if (wasSizeDriven)
        {
            Size = expandedSize;
            SizeCondition = ImGuiCond.Always;
        }
        else
        {
            SizeCondition = ImGuiCond.FirstUseEver;
        }
    }

    private void AdvanceCollapse()
    {
        float target = (compact ? 1f : 0f);
        if (Motion.Reduced)
        {
            collapse = target;
            return;
        }
        float distance = MathF.Abs(target - collapseFrom);
        if (distance <= 0.0001f)
        {
            collapse = target;
            return;
        }
        float t = Math.Clamp((float)(Environment.TickCount64 - collapseTick) / (CollapseMs * distance), 0f, 1f);
        collapse = collapseFrom + (target - collapseFrom) * Motion.EaseInOutCubic(t);
    }

    public override void PostDraw()
    {
        chrome?.Dispose();
        chrome = null;
        bodyFont?.Dispose();
        bodyFont = null;
    }

    public override void Draw()
    {
        if (Themes.UsesGlass)
        {
            plugin.GlassworkHost.DrawWindowBackground(14f, Themes.GlassAmount, Themes.IsLight);
        }
        float globalScale = ImGuiHelpers.GlobalScale;
        float headerHeight = 52f * globalScale;
        Vector2 windowSize = ImGui.GetWindowSize();
        if (HeaderBar.HandleDrag(ImGui.GetWindowPos(), windowSize.X, headerHeight))
        {
            ToggleCompact();
        }
        Vector2 windowPos = ImGui.GetWindowPos();
        float windowRounding = 14f * globalScale;
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        if (collapse >= 0.999f)
        {
            HeaderBar.Draw(this, plugin, windowPos, windowSize.X, headerHeight, windowRounding, compact: true);
            return;
        }
        if (!sizeDriven)
        {
            expandedSize = windowSize / globalScale;
        }
        Ambient.Draw(windowDrawList, windowPos, windowPos + windowSize);
        HeaderBar.Draw(this, plugin, windowPos, windowSize.X, headerHeight, windowRounding, compact: false);
        using (ImRaii.PushStyle(ImGuiStyleVar.Alpha, MathF.Max(0.001f, 1f - collapse)))
        {
            DrawBody(windowPos, windowSize, headerHeight, windowRounding);
            if (!sizeDriven)
            {
                DrawResizeGrip(windowDrawList, windowPos + windowSize);
            }
        }
    }

    private static void DrawResizeGrip(ImDrawListPtr dl, Vector2 corner)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        float gripSize = GripSize * globalScale;
        float inset = GripInset * globalScale;
        var origin = corner - new Vector2(inset, inset);
        bool hovered = ImGui.IsMouseHoveringRect(corner - new Vector2(gripSize + inset, gripSize + inset), corner);
        uint color = Paint.Col(Styling.WithAlpha(Styling.TextStrong, hovered ? 0.38f : 0.14f));
        for (int i = 1; i <= 3; i++)
        {
            float offset = (float)i * gripSize / 3f;
            dl.AddLine(origin - new Vector2(offset, 0f), origin - new Vector2(0f, offset), color, 1.4f * globalScale);
        }
    }

    private void DrawBody(Vector2 windowPos, Vector2 windowSize, float headerHeight, float windowRounding)
    {
        float globalScale = ImGuiHelpers.GlobalScale;
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        bool running = plugin.Typer.Running;
        float dockHeight = ((this.page == Page.Brawl) ? (78f * globalScale) : (running ? (62f * globalScale) : 0f));
        float railWidth = 64f * globalScale;
        float bodyTop = windowPos.Y + headerHeight;
        float bodyHeight = windowSize.Y - headerHeight - dockHeight;
        if (bodyHeight < 1f)
        {
            return;
        }
        ImGui.SetCursorScreenPos(new Vector2(windowPos.X, bodyTop));
        using (ImRaii.ChildDisposable railChild = ImRaii.Child("##akb_rail", new Vector2(railWidth, bodyHeight), border: false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            if (railChild.Success)
            {
                if (NavRail.Draw(this.page, plugin) is { } selected)
                {
                    Show(selected);
                }
            }
        }
        Paint.Hairline(windowDrawList, new Vector2(windowPos.X + railWidth, bodyTop + 10f * globalScale), new Vector2(windowPos.X + railWidth, bodyTop + bodyHeight - 10f * globalScale));
        ImGui.SetCursorScreenPos(new Vector2(windowPos.X + railWidth, bodyTop));
        float pagePad = 26f * globalScale;
        float gutter = 6f * globalScale;
        using (ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(pagePad, pagePad * 0.8f)))
        {
            using ImRaii.ChildDisposable pageChild = ImRaii.Child("##akb_page", new Vector2(windowSize.X - railWidth - gutter, bodyHeight), border: false, ImGuiWindowFlags.AlwaysUseWindowPadding);
            if ((bool)pageChild)
            {
                DrawPage();
            }
        }
        if (dockHeight <= 0f)
        {
            return;
        }
        ImGui.SetCursorScreenPos(new Vector2(windowPos.X, windowPos.Y + windowSize.Y - dockHeight));
        Vector2 dockSize = new Vector2(windowSize.X, dockHeight);
        if (this.page == Page.Brawl)
        {
            ActionDock.Draw(plugin, dockSize, windowRounding);
        }
        else if (MiniPlayer.Draw(plugin, dockSize, windowRounding))
        {
            Show(Page.Brawl);
        }
    }

    private void DrawPage()
    {
        if (resetScroll)
        {
            ImGui.SetScrollY(0f);
            resetScroll = false;
        }
        using (Motion.PushReveal(Motion.Reveal(pageShownTick, PageRevealMs), PageSlide))
        {
            switch (page)
            {
                case Page.Brawl:
                    brawlPage.Draw(plugin);
                    break;
                case Page.Settings:
                    settingsPage.Draw(plugin);
                    break;
                case Page.Log:
                    logPage.Draw();
                    break;
                case Page.Changelog:
                    changelogPage.Draw(pageShownTick);
                    break;
                case Page.About:
                    aboutPage.Draw(pageShownTick);
                    break;
            }
        }
    }
}
