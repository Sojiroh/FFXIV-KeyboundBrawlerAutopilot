using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace AutoKeyboundBrawler.Windows.Shell;

internal static class Ambient
{
    private const int Layers = 5;

    public static void Draw(ImDrawListPtr dl, Vector2 min, Vector2 max)
    {
        float width = max.X - min.X;
        float height = max.Y - min.Y;
        dl.PushClipRect(min, max, intersectWithCurrentClipRect: true);
        Blob(dl, min + new Vector2(width * (0.2f + 0.08f * Motion.Wave(16000.0)), height * (0.14f + 0.06f * Motion.Wave(21000.0))), width * 0.45f, Styling.AccentTeal, 0.055f);
        Blob(dl, min + new Vector2(width * (0.86f + 0.06f * Motion.Wave(19000.0)), height * (0.32f + 0.08f * Motion.Wave(14000.0))), width * 0.4f, Styling.AccentMint, 0.04f);
        Blob(dl, min + new Vector2(width * (0.55f + 0.1f * Motion.Wave(23000.0)), height * (0.96f + 0.05f * Motion.Wave(17000.0))), width * 0.42f, Styling.AccentViolet, 0.035f);
        dl.PopClipRect();
    }

    private static void Blob(ImDrawListPtr dl, Vector2 center, float radius, Vector4 color, float peak)
    {
        for (int layer = Layers; layer >= 1; layer--)
        {
            float layerRadius = radius * (float)layer / Layers;
            float alpha = peak * (1f - ((float)layer - 1f) / Layers);
            dl.AddCircleFilled(center, layerRadius, Paint.Col(Styling.WithAlpha(color, alpha)), 48);
        }
    }
}
