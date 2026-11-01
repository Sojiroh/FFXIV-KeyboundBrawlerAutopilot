namespace AutoKeyboundBrawler.Core.Localization;

internal readonly struct LocString(string key, string source)
{
    public readonly string Key = key;

    public readonly string Source = source;
}

internal readonly struct LocPlural(string keyBase, string oneSource, string otherSource)
{
    public readonly string KeyBase = keyBase;

    public readonly string OneSource = oneSource;

    public readonly string OtherSource = otherSource;
}
