using System;
using Glasswork;
using Glasswork.Dalamud;

namespace AutoKeyboundBrawler.Windows;

internal static class Themes
{
    public const string DefaultId = "grindless";

    public const string GlassworkUrl = "https://www.nuget.org/packages/Glasswork.Dalamud";

    private const float RadiusSmall = 7f;

    private const float RadiusMedium = 10f;

    private const float RadiusLarge = 14f;

    private const float TextScaleTolerance = 0.001f;

    private static readonly ThemeChoice[] choices = new ThemeChoice[15]
    {
        new ThemeChoice(DefaultId, "Grindless", GrindlessDark, GrindlessLight),
        new ThemeChoice("neutral", "Neutral", Look.NeutralDark, Look.NeutralLight),
        new ThemeChoice("liquid", "Liquid", Look.Liquid, Look.LiquidLight),
        new ThemeChoice("eorzea", "Eorzea", Look.Eorzea, Look.EorzeaLight),
        new ThemeChoice("classic", "Classic", Look.Classic, Look.ClassicLight),
        new ThemeChoice("clearBlue", "Clear Blue", Look.ClearBlue, Look.ClearBlueLight),
        new ThemeChoice("clearGrey", "Clear Grey", Look.ClearGrey, Look.ClearGreyLight),
        new ThemeChoice("clearGreen", "Clear Green", Look.ClearGreen, Look.ClearGreenLight),
        new ThemeChoice("clearPink", "Clear Pink", Look.ClearPink, Look.ClearPinkLight),
        new ThemeChoice("aurora", "Aurora", Look.Aurora, Look.AuroraLight),
        new ThemeChoice("catppuccin", "Catppuccin", Look.Catppuccin, Look.CatppuccinLight),
        new ThemeChoice("nord", "Nord", Look.Nord, Look.NordLight),
        new ThemeChoice("minimal", "Minimal", Look.Minimal, Look.MinimalLight),
        new ThemeChoice("retro", "Retro", Look.Retro, Look.RetroLight),
        new ThemeChoice("highContrast", "High contrast", Look.HighContrast, Look.HighContrastLight)
    };

    private static readonly string[] names = BuildNames();

    public static ReadOnlySpan<string> Names => names;

    public static bool UsesGlass { get; private set; }

    public static float GlassAmount { get; private set; }

    public static bool IsLight { get; private set; }

    public static int IndexOf(string id)
    {
        for (var i = 0; i < choices.Length; i++)
        {
            if (string.Equals(choices[i].Id, id, StringComparison.Ordinal))
            {
                return i;
            }
        }
        return 0;
    }

    public static string IdAt(int index)
    {
        return choices[Math.Clamp(index, 0, choices.Length - 1)].Id;
    }

    public static void Apply(GlassworkHost host, string id, bool light)
    {
        var themeChoice = choices[IndexOf(id)];
        var look = (light ? themeChoice.Light() : themeChoice.Dark());
        MatchControlText(look.Type);
        host.Look = look;
        UsesGlass = look.PanelMaterial.IsGlass;
        GlassAmount = look.GlassAmount;
        IsLight = look.IsLight;
        Styling.Use((themeChoice.Id == DefaultId && !light) ? Palette.Grindless() : Palette.From(look));
    }

    public static void SyncTextScale(GlassworkHost host)
    {
        var type = host.Look.Type;
        var bodyScale = Fonts.BodyPixels / type.Size(TextRole.Body);
        if (MathF.Abs(type.TextScale - bodyScale) > TextScaleTolerance)
        {
            type.TextScale = bodyScale;
        }
    }

    private static void MatchControlText(TypeScale type)
    {
        var size = type.Size(TextRole.Body);
        type.Set(TextRole.Subheadline, size, type.Weight(TextRole.Subheadline));
        type.Set(TextRole.Footnote, size, type.Weight(TextRole.Footnote));
    }

    private static Look GrindlessDark()
    {
        var palette = Palette.Grindless();
        var look = WithGrindlessAccents(Look.NeutralDark(), palette);
        look.Background = Styling.ToGlasswork(palette.WindowBg with { W = 1f });
        look.SecondaryBackground = Styling.ToGlasswork(palette.Surface0);
        look.TertiaryBackground = Styling.ToGlasswork(palette.Surface1);
        look.Label = Styling.ToGlasswork(palette.TextStrong);
        look.SecondaryLabel = Styling.ToGlasswork(palette.TextDim);
        look.TertiaryLabel = Styling.ToGlasswork(palette.TextMuted);
        return look;
    }

    private static Look GrindlessLight()
    {
        return WithGrindlessAccents(Look.NeutralLight(), Palette.Grindless());
    }

    private static Look WithGrindlessAccents(Look baseLook, Palette palette)
    {
        var look = baseLook.WithAccent(Styling.ToGlasswork(palette.AccentTealDeep)).WithStatusColors(Styling.ToGlasswork(palette.AccentMint), Styling.ToGlasswork(palette.AccentAmber), Styling.ToGlasswork(palette.AccentRose), Styling.ToGlasswork(palette.AccentBlue));
        look.Name = "Grindless";
        look.ToggleOn = Styling.ToGlasswork(palette.AccentTeal);
        look.RadiusSmall = RadiusSmall;
        look.RadiusMedium = RadiusMedium;
        look.RadiusLarge = RadiusLarge;
        return look;
    }

    private static string[] BuildNames()
    {
        var result = new string[choices.Length];
        for (var i = 0; i < choices.Length; i++)
        {
            result[i] = choices[i].Name;
        }
        return result;
    }
}
