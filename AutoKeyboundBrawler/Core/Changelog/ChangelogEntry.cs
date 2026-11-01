using AutoKeyboundBrawler.Core.Localization;

namespace AutoKeyboundBrawler.Core.Changelog;

internal readonly record struct ChangelogEntry(string Version, string Date, LocString[] Highlights);
