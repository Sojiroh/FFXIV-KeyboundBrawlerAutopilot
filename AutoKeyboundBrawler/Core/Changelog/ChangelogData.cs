using System;
using AutoKeyboundBrawler.Core.Localization;

namespace AutoKeyboundBrawler.Core.Changelog;

internal static class ChangelogData
{
    public static readonly ChangelogEntry[] Entries = new ChangelogEntry[15]
    {
        new ChangelogEntry("1.5.2.0", "2026-10-28", L.Changelog.Release1520),
        new ChangelogEntry("1.5.1.0", "2026-10-28", L.Changelog.Release1510),
        new ChangelogEntry("1.5.0.0", "2026-10-28", L.Changelog.Release1500),
        new ChangelogEntry("1.4.4.0", "2026-10-28", L.Changelog.Release1440),
        new ChangelogEntry("1.4.3.0", "2026-10-28", L.Changelog.Release1430),
        new ChangelogEntry("1.4.2.0", "2026-10-28", L.Changelog.Release1420),
        new ChangelogEntry("1.4.1.0", "2026-10-28", L.Changelog.Release1410),
        new ChangelogEntry("1.4.0.0", "2026-10-28", L.Changelog.Release1400),
        new ChangelogEntry("1.3.2.0", "2026-10-28", L.Changelog.Release1320),
        new ChangelogEntry("1.3.1.0", "2026-10-28", L.Changelog.Release1310),
        new ChangelogEntry("1.3.0.0", "2026-10-28", L.Changelog.Release1300),
        new ChangelogEntry("1.2.1.0", "2026-10-28", L.Changelog.Release1210),
        new ChangelogEntry("1.2.0.0", "2026-10-28", L.Changelog.Release1200),
        new ChangelogEntry("1.1.0.0", "2026-10-28", L.Changelog.Release1100),
        new ChangelogEntry("1.0.0.0", "2026-10-28", L.Changelog.Release1000)
    };

    public static string LatestVersion => Entries[0].Version;

    public static int UnseenCount(string lastSeenVersion)
    {
        for (int i = 0; i < Entries.Length; i++)
        {
            if (string.Equals(Entries[i].Version, lastSeenVersion, StringComparison.Ordinal))
            {
                return i;
            }
        }
        return Entries.Length;
    }
}
