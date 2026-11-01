using System;
using AutoKeyboundBrawler.Core;
using AutoKeyboundBrawler.Core.Changelog;
using AutoKeyboundBrawler.Core.Typing;
using Dalamud.Configuration;
using ECommons.Throttlers;
using Newtonsoft.Json;

namespace AutoKeyboundBrawler;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    private const int SnapshotsOnVersion = 2;

    private const int DirectSubmitVersion = 3;

    private const int CurrentVersion = 3;

    public int Version { get; set; } = CurrentVersion;

    public string Language { get; set; } = "";

    public bool DragWindowFromAnywhere { get; set; }

    public string Theme { get; set; } = "grindless";

    public bool LightTheme { get; set; }

    public TypingSpeed Speed { get; set; } = TypingSpeed.Fast;

    public InputMethod InputMethod { get; set; } = InputMethod.Direct;

    public bool AutoFocusInput { get; set; } = true;

    public TargetOrder Order { get; set; }

    public int CrowdedAt { get; set; } = 4;

    public bool LetterlockGuard { get; set; } = true;

    public bool RecordSnapshots { get; set; } = true;

    public bool HumanizerEnabled { get; set; } = true;

    public float ReactDelayMinSeconds { get; set; } = 0.25f;

    public float ReactDelayMaxSeconds { get; set; } = 0.6f;

    public float SubmitDelayMinSeconds { get; set; } = 0.08f;

    public float SubmitDelayMaxSeconds { get; set; } = 0.25f;

    public bool HumanizerTypos { get; set; } = true;

    public bool AutoQueue { get; set; }

    public bool WalkToRegistrar { get; set; } = true;

    public int StopAfterMatches { get; set; }

    public bool StopAtMgcCap { get; set; } = true;

    public int LifetimeMatches { get; set; }

    public long LifetimeMgc { get; set; }

    public long LifetimeMgp { get; set; }

    public string LastSeenChangelogVersion { get; set; } = string.Empty;

    [JsonIgnore]
    public bool HasUnseenChangelog => !string.Equals(LastSeenChangelogVersion, ChangelogData.LatestVersion, StringComparison.Ordinal);

    public void Migrate()
    {
        if (Version >= CurrentVersion)
        {
            return;
        }
        if (Version < SnapshotsOnVersion)
        {
            RecordSnapshots = true;
        }
        if (Version < DirectSubmitVersion)
        {
            InputMethod = InputMethod.Direct;
        }
        Version = CurrentVersion;
        Save();
    }

    public DelayRange DelayFor(HumanAction action)
    {
        return action switch
        {
            HumanAction.React => new DelayRange(ReactDelayMinSeconds, ReactDelayMaxSeconds),
            _ => new DelayRange(SubmitDelayMinSeconds, SubmitDelayMaxSeconds),
        };
    }

    public void SetDelay(HumanAction action, DelayRange range)
    {
        switch (action)
        {
            case HumanAction.React:
                (ReactDelayMinSeconds, ReactDelayMaxSeconds) = range;
                break;
            default:
                (SubmitDelayMinSeconds, SubmitDelayMaxSeconds) = range;
                break;
        }
    }

    public void MarkChangelogSeen()
    {
        if (!HasUnseenChangelog)
        {
            return;
        }
        LastSeenChangelogVersion = ChangelogData.LatestVersion;
        Save();
    }

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public void SaveDebounced()
    {
        if (EzThrottler.Throttle(AkbConstants.ThrottleKeys.Save))
        {
            Save();
        }
    }
}
