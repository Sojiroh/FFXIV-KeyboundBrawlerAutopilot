using System.Numerics;
using Glasswork;
using Glasswork.Backend;

namespace AutoKeyboundBrawler.Windows.Components;

internal static class PageHeader
{
    private const float BottomGap = 14f;

    public static void Draw(string title, string subtitle, Vector4? subtitleColor = null)
    {
        Gw.Heading(title);
        Gw.TextWrapped(subtitle, TextRole.Body, subtitleColor is { } color ? Styling.ToGlasswork(color) : Frame.Look.SecondaryLabel);
        Styling.VSpace(BottomGap);
    }
}
