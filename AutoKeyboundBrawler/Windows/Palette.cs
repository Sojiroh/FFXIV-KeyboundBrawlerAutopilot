using System.Numerics;
using Glasswork;

namespace AutoKeyboundBrawler.Windows;

internal sealed class Palette
{
    private const float SoftLift = 0.3f;

    private const float DeepDrop = 0.3f;

    private const float Surface0Step = 0.033f;

    private const float Surface1Step = 0.061f;

    private const float Surface2Step = 0.098f;

    private const float Surface3Step = 0.139f;

    private const float SliderStep = 0.118f;

    private const float BorderStep = 0.198f;

    private const float SolidWindowAlpha = 0.985f;

    private const float GlassWindowAlpha = 0.35f;

    private const float SolidCardAlpha = 0.9f;

    private const float GlassCardAlpha = 0.55f;

    public Vector4 AccentTeal;

    public Vector4 AccentTealSoft;

    public Vector4 AccentTealDeep;

    public Vector4 AccentMint;

    public Vector4 AccentMintSoft;

    public Vector4 AccentAmber;

    public Vector4 AccentAmberSoft;

    public Vector4 AccentRose;

    public Vector4 AccentRoseSoft;

    public Vector4 AccentBlue;

    public Vector4 AccentBlueSoft;

    public Vector4 WindowBg;

    public Vector4 Surface0;

    public Vector4 Surface1;

    public Vector4 Surface2;

    public Vector4 Surface3;

    public Vector4 CardBg;

    public Vector4 CardBgSoft;

    public Vector4 CardBgHover;

    public Vector4 SliderBg;

    public Vector4 BorderDim;

    public Vector4 TextStrong;

    public Vector4 TextSecondary;

    public Vector4 TextDim;

    public Vector4 TextMuted;

    public Vector4 Ink;

    public static Palette Grindless()
    {
        return new Palette
        {
            AccentTeal = new Vector4(0.32f, 0.78f, 0.78f, 1f),
            AccentTealSoft = new Vector4(0.52f, 0.91f, 0.91f, 1f),
            AccentTealDeep = new Vector4(0.16f, 0.56f, 0.6f, 1f),
            AccentMint = new Vector4(0.46f, 0.86f, 0.66f, 1f),
            AccentMintSoft = new Vector4(0.66f, 0.96f, 0.8f, 1f),
            AccentAmber = new Vector4(0.92f, 0.74f, 0.34f, 1f),
            AccentAmberSoft = new Vector4(1f, 0.86f, 0.52f, 1f),
            AccentRose = new Vector4(0.93f, 0.42f, 0.5f, 1f),
            AccentRoseSoft = new Vector4(1f, 0.62f, 0.68f, 1f),
            AccentBlue = new Vector4(0.4f, 0.68f, 0.98f, 1f),
            AccentBlueSoft = new Vector4(0.62f, 0.82f, 1f, 1f),
            WindowBg = new Vector4(0.046f, 0.056f, 0.068f, 0.985f),
            Surface0 = new Vector4(0.076f, 0.092f, 0.11f, 1f),
            Surface1 = new Vector4(0.102f, 0.122f, 0.145f, 1f),
            Surface2 = new Vector4(0.136f, 0.16f, 0.188f, 1f),
            Surface3 = new Vector4(0.174f, 0.202f, 0.236f, 1f),
            CardBg = new Vector4(0.076f, 0.092f, 0.11f, 0.9f),
            CardBgSoft = new Vector4(0.102f, 0.122f, 0.145f, 0.62f),
            CardBgHover = new Vector4(0.136f, 0.16f, 0.188f, 0.95f),
            SliderBg = new Vector4(0.154f, 0.18f, 0.212f, 1f),
            BorderDim = new Vector4(0.228f, 0.268f, 0.31f, 1f),
            TextStrong = new Vector4(0.965f, 0.968f, 0.975f, 1f),
            TextSecondary = new Vector4(0.78f, 0.805f, 0.84f, 1f),
            TextDim = new Vector4(0.56f, 0.595f, 0.64f, 1f),
            TextMuted = new Vector4(0.4f, 0.425f, 0.47f, 1f),
            Ink = Vector4.One
        };
    }

    public static Palette From(Look look)
    {
        var background = look.Background.ToVector4() with
        {
            W = 1f
        };
        var label = look.Label.ToVector4();
        var isGlass = look.PanelMaterial.IsGlass;
        var cardAlpha = (isGlass ? GlassCardAlpha : SolidCardAlpha);
        var accent = look.Accent.ToVector4();
        var success = look.Success.ToVector4();
        var warning = look.Warning.ToVector4();
        var destructive = look.Destructive.ToVector4();
        var info = look.Info.ToVector4();
        var surface0 = Mix(background, label, Surface0Step);
        var surface1 = Mix(background, label, Surface1Step);
        var surface2 = Mix(background, label, Surface2Step);
        var secondary = look.SecondaryLabel.ToVector4();
        return new Palette
        {
            AccentTeal = accent,
            AccentTealSoft = Styling.Lighten(accent, SoftLift),
            AccentTealDeep = Styling.Darken(accent, DeepDrop),
            AccentMint = success,
            AccentMintSoft = Styling.Lighten(success, SoftLift),
            AccentAmber = warning,
            AccentAmberSoft = Styling.Lighten(warning, SoftLift),
            AccentRose = destructive,
            AccentRoseSoft = Styling.Lighten(destructive, SoftLift),
            AccentBlue = info,
            AccentBlueSoft = Styling.Lighten(info, SoftLift),
            WindowBg = background with
            {
                W = (isGlass ? GlassWindowAlpha : SolidWindowAlpha)
            },
            Surface0 = surface0,
            Surface1 = surface1,
            Surface2 = surface2,
            Surface3 = Mix(background, label, Surface3Step),
            CardBg = surface0 with
            {
                W = cardAlpha
            },
            CardBgSoft = surface1 with
            {
                W = 0.62f
            },
            CardBgHover = surface2 with
            {
                W = 0.95f
            },
            SliderBg = Mix(background, label, SliderStep),
            BorderDim = Mix(background, label, BorderStep),
            TextStrong = label,
            TextSecondary = Vector4.Lerp(label, secondary, 0.5f),
            TextDim = secondary,
            TextMuted = look.TertiaryLabel.ToVector4(),
            Ink = (look.IsLight ? (label with
            {
                W = 1f
            }) : Vector4.One)
        };
    }

    private static Vector4 Mix(Vector4 from, Vector4 to, float amount)
    {
        var mixed = Vector4.Lerp(from, to, amount);
        mixed.W = 1f;
        return mixed;
    }
}
