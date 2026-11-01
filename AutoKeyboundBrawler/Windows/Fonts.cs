using System;
using System.IO;
using AutoKeyboundBrawler.Core;
using AutoKeyboundBrawler.Core.Game;
using AutoKeyboundBrawler.Core.Localization;
using Dalamud;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.DalamudServices;
using Glasswork;
using Glasswork.Dalamud;
using Lumina.Excel;

namespace AutoKeyboundBrawler.Windows;

internal static class Fonts
{
    private sealed class NoOpScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private const float CaptionScale = 1f;

    private const float BodyScale = 1.125f;

    private const float HeadlineScale = 1.25f;

    private const float TitleScale = 1.625f;

    private const float IconScale = 1.125f;

    private const float IconLargeScale = 1.625f;

    private const float IconDisplayScale = 2.25f;

    private const string PhraseSheetName = "KTGTypeWordTextData";

    private const string LatinFontFile = "NotoSans-Medium-Latin.ttf";

    private const int LatinBlocksEnd = 879;

    private const int LatinAdditionalStart = 7680;

    private const int LatinAdditionalEnd = 7935;

    private const int NotoCjkJapaneseFace = 0;

    private const int NotoCjkSimplifiedChineseFace = 2;

    private static readonly ushort[] LatinBlocks = new ushort[8] { 32, 255, 256, 383, 384, 591, 8192, 8303 };

    private static readonly ushort[] SymbolBlocks = new ushort[4] { 8592, 8703, 8704, 8959 };

    private static readonly NoOpScope noOp = new NoOpScope();

    private static IUiBuilder? builder;

    private static IFontAtlas? atlas;

    private static byte[]? latinFont;

    private static ushort[] latinRanges = new ushort[1];

    private static ushort[] mergeRanges = new ushort[1];

    private static ushort[]? phraseGlyphRanges;

    private static int cjkFace = NotoCjkJapaneseFace;

    private static float unitPx = UiBuilder.DefaultFontSizePx;

    private static IFontHandle? body;

    private static IFontHandle? title;

    private static IFontHandle? headline;

    private static IFontHandle? caption;

    private static IFontHandle? icon;

    private static IFontHandle? iconLarge;

    private static IFontHandle? iconDisplay;

    public static float BodyPixels => unitPx * BodyScale;

    public static void Initialize(IUiBuilder uiBuilder, string pluginDirectory)
    {
        builder = uiBuilder;
        atlas = uiBuilder.FontAtlas;
        latinFont = LoadLatinFont(Path.Combine(pluginDirectory, "Fonts", LatinFontFile));
        RefreshRanges();
        body = TextHandle(BodyScale);
        title = TextHandle(TitleScale);
        headline = TextHandle(HeadlineScale);
        caption = TextHandle(CaptionScale);
        icon = IconHandle(IconScale);
        iconLarge = IconHandle(IconLargeScale);
        iconDisplay = IconHandle(IconDisplayScale);
        uiBuilder.DefaultFontChanged += Rebuild;
        if (atlas.AutoRebuildMode == FontAtlasAutoRebuildMode.Disable)
        {
            atlas.BuildFontsAsync();
        }
    }

    public static void OnLanguageChanged()
    {
        Rebuild();
    }

    public static void ShareWith(DalamudFontSource source, string pluginDirectory)
    {
        var path = Path.Combine(pluginDirectory, "Fonts", LatinFontFile);
        if (!File.Exists(path))
        {
            return;
        }
        for (var fontWeight = FontWeight.Regular; fontWeight <= FontWeight.Bold; fontWeight++)
        {
            source.AddFontFile(fontWeight, path, LatinBlocks);
        }
    }

    public static void Dispose()
    {
        if (builder != null)
        {
            builder.DefaultFontChanged -= Rebuild;
        }
        body?.Dispose();
        title?.Dispose();
        headline?.Dispose();
        caption?.Dispose();
        icon?.Dispose();
        iconLarge?.Dispose();
        iconDisplay?.Dispose();
        body = (title = (headline = (caption = (icon = (iconLarge = (iconDisplay = null))))));
        builder = null;
        atlas = null;
        latinFont = null;
        phraseGlyphRanges = null;
    }

    public static IDisposable PushBody()
    {
        return body?.Push() ?? noOp;
    }

    public static IDisposable PushTitle()
    {
        return title?.Push() ?? noOp;
    }

    public static IDisposable PushHeadline()
    {
        return headline?.Push() ?? noOp;
    }

    public static IDisposable PushCaption()
    {
        return caption?.Push() ?? noOp;
    }

    public static IDisposable PushIcon()
    {
        return icon?.Push() ?? ImRaii.PushFont(UiBuilder.IconFont);
    }

    public static IDisposable PushIconLarge()
    {
        return iconLarge?.Push() ?? ImRaii.PushFont(UiBuilder.IconFont);
    }

    public static IDisposable PushIconDisplay()
    {
        return iconDisplay?.Push() ?? ImRaii.PushFont(UiBuilder.IconFont);
    }

    public static IDisposable PushIconFor(float targetHeight)
    {
        var unitScaled = unitPx * ImGuiHelpers.GlobalScale;
        if (targetHeight > unitScaled * IconLargeScale)
        {
            return PushIconDisplay();
        }
        if (targetHeight > unitScaled * IconScale)
        {
            return PushIconLarge();
        }
        return PushIcon();
    }

    private static byte[]? LoadLatinFont(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            RunLog.Warning("Latin font missing at '" + path + "'; falling back to the Dalamud default font, Latin Extended letters will not render");
        }
        catch (Exception exception)
        {
            RunLog.Error(exception, "Failed to read the Latin font");
        }
        return null;
    }

    private static IFontHandle TextHandle(float scale)
    {
        return atlas!.NewDelegateFontHandle((IFontAtlasBuildToolkit e) =>
        {
            e.OnPreBuild((IFontAtlasBuildToolkitPreBuild tk) =>
            {
                var sizePx = unitPx * scale;
                var font = latinFont != null
                    ? tk.AddFontFromMemory(latinFont, new SafeFontConfig { SizePx = sizePx, GlyphRanges = latinRanges }, LatinFontFile)
                    : tk.AddDalamudDefaultFont(sizePx, latinRanges);
                tk.Font = font;
                tk.AddGameSymbol(new SafeFontConfig
                {
                    SizePx = sizePx,
                    MergeFont = font
                });
                if (mergeRanges.Length > 1)
                {
                    tk.AddDalamudAssetFont(DalamudAsset.NotoSansCjkRegular, new SafeFontConfig
                    {
                        SizePx = sizePx,
                        GlyphRanges = mergeRanges,
                        MergeFont = font,
                        FontNo = cjkFace
                    });
                }
            });
        });
    }

    private static IFontHandle IconHandle(float scale)
    {
        return atlas!.NewDelegateFontHandle((IFontAtlasBuildToolkit e) =>
        {
            e.OnPreBuild((IFontAtlasBuildToolkitPreBuild tk) =>
            {
                tk.AddFontAwesomeIconFont(new SafeFontConfig
                {
                    SizePx = unitPx * scale
                });
            });
        });
    }

    private static void Rebuild()
    {
        RefreshRanges();
        if (atlas != null)
        {
            atlas.BuildFontsAsync();
        }
    }

    private static void RefreshRanges()
    {
        unitPx = builder?.DefaultFontSpec.SizePx ?? UiBuilder.DefaultFontSizePx;
        var latin = new bool[65536];
        var symbols = new bool[65536];
        var needed = new bool[65536];
        GlyphRanges.MarkRanges(latin, LatinBlocks);
        GlyphRanges.MarkRanges(symbols, SymbolBlocks);
        GlyphRanges.MarkRanges(needed, Loc.Current.ExtraGlyphRanges);
        GlyphRanges.MarkRanges(needed, Loc.CatalogGlyphRanges);
        GlyphRanges.MarkRanges(needed, phraseGlyphRanges ?? (phraseGlyphRanges = ScanPhraseGlyphRanges()));
        MarkNativeNames(needed);
        for (var codepoint = 128; codepoint < 65536; codepoint++)
        {
            if (!latin[codepoint] && needed[codepoint])
            {
                if (IsLatinCodepoint(codepoint))
                {
                    latin[codepoint] = true;
                }
                else
                {
                    symbols[codepoint] = true;
                }
            }
        }
        latinRanges = GlyphRanges.ToRanges(latin);
        mergeRanges = GlyphRanges.ToRanges(symbols);
        cjkFace = Svc.Data.Language switch
        {
            ClientLanguage.Japanese => NotoCjkJapaneseFace,
            (ClientLanguage)4 => NotoCjkSimplifiedChineseFace,
            _ => (Loc.Current == Languages.Chinese) ? NotoCjkSimplifiedChineseFace : NotoCjkJapaneseFace,
        };
    }

    private static ushort[]? ScanPhraseGlyphRanges()
    {
        var present = new bool[65536];
        ExcelSheet<RawRow> sheet;
        try
        {
            sheet = Svc.Data.Excel.GetSheet<RawRow>(null, PhraseSheetName);
        }
        catch (Exception exception)
        {
            RunLog.Warning(exception, "could not read the KTGTypeWordTextData sheet for font glyphs");
            return GlyphRanges.ToRanges(present);
        }
        for (var i = 0; i < sheet.Count; i++)
        {
            var row = sheet.GetRowAt(i);
            GlyphRanges.MarkText(present, row.ReadStringColumn(0).ExtractText());
            if (PhraseReadings.ClientIsJapanese)
            {
                GlyphRanges.MarkText(present, row.ReadStringColumn(1).ExtractText());
            }
        }
        return GlyphRanges.ToRanges(present);
    }

    private static bool IsLatinCodepoint(int codepoint)
    {
        return codepoint <= LatinBlocksEnd || (codepoint >= LatinAdditionalStart && codepoint <= LatinAdditionalEnd);
    }

    private static void MarkNativeNames(bool[] extra)
    {
        var languages = Languages.All;
        for (var i = 0; i < languages.Length; i++)
        {
            GlyphRanges.MarkText(extra, languages[i].NativeName);
        }
    }
}
