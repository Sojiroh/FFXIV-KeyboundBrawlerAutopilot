namespace AutoKeyboundBrawler.Core.Localization;

internal static class L
{
    internal static class Common
    {
        public static readonly LocString Close = new LocString("common.close", "Close");

        public static readonly LocString OpenCopyHint = new LocString("common.openCopyHint", "Click to open · right-click to copy");

        public static readonly LocPlural PhrasesCount = new LocPlural("common.phrasesCount", "{0} phrase", "{0} phrases");
    }

    internal static class Shell
    {
        public static readonly LocString NavBrawl = new LocString("shell.nav.brawl", "Brawl");

        public static readonly LocString NavSettings = new LocString("shell.nav.settings", "Settings");

        public static readonly LocString NavAbout = new LocString("shell.nav.about", "About");

        public static readonly LocString NavLog = new LocString("shell.nav.log", "Console");

        public static readonly LocString NavChangelog = new LocString("shell.nav.changelog", "Changelog");

        public static readonly LocString StatusReady = new LocString("shell.status.ready", "Ready");

        public static readonly LocString StatusWaiting = new LocString("shell.status.waiting", "Waiting");

        public static readonly LocString StatusPaused = new LocString("shell.status.paused", "Paused");

        public static readonly LocString StatusTyping = new LocString("shell.status.typing", "Typing");

        public static readonly LocString StatusStalled = new LocString("shell.status.stalled", "Stalled");

        public static readonly LocString Minimize = new LocString("shell.minimize", "Minimize to the header bar");

        public static readonly LocString Restore = new LocString("shell.restore", "Restore the window");

        public static readonly LocString CompactIdle = new LocString("shell.compactIdle", "Press Start, then queue for a match");

        public static readonly LocString CompactRunning = new LocString("shell.compactRunning", "{0}  ·  {1}");

        public static readonly LocString Start = new LocString("shell.start", "Start");

        public static readonly LocString Stop = new LocString("shell.stop", "Stop");

        public static readonly LocString StopSub = new LocString("shell.stopSub", "{0} · {1}");

        public static readonly LocString StopTyping = new LocString("shell.stopTyping", "Stop typing");

        public static readonly LocString MiniStatus = new LocString("shell.miniStatus", "{0}   ·   {1}");
    }

    internal static class Brawl
    {
        public static readonly LocString TitleIdle = new LocString("brawl.title.idle", "Ready when you are.");

        public static readonly LocString DetailIdle = new LocString("brawl.detail.idle", "Press Start, then queue for Keybound Brawler at the Gold Saucer.");

        public static readonly LocString TitleMatch = new LocString("brawl.title.match", "Waiting for a match.");

        public static readonly LocString DetailMatch = new LocString("brawl.detail.match", "Typing starts as soon as the Keybound Brawler input bar appears.");

        public static readonly LocString TitleFocus = new LocString("brawl.title.focus", "Click the input bar.");

        public static readonly LocString DetailFocus = new LocString("brawl.detail.focus", "The input bar lost focus, so typing is paused to keep phrases out of the chat box.");

        public static readonly LocString TitleElsewhere = new LocString("brawl.title.elsewhere", "Paused while another text box is in use.");

        public static readonly LocString DetailElsewhere = new LocString("brawl.detail.elsewhere", "Typing picks up again once you leave chat or the other text box.");

        public static readonly LocString TitleWave = new LocString("brawl.title.wave", "Between waves.");

        public static readonly LocString DetailWave = new LocString("brawl.detail.wave", "Waiting for the next phrases to appear above the enemies.");

        public static readonly LocString TitleTyping = new LocString("brawl.title.typing", "Typing \"{0}\".");

        public static readonly LocString DetailTyping = new LocString("brawl.detail.typing", "{0} submitted so far.");

        public static readonly LocString TitleHolding = new LocString("brawl.title.holding", "Holding still for a cast.");

        public static readonly LocString TitleHoldingSpell = new LocString("brawl.title.holdingSpell", "Holding still for {0}.");

        public static readonly LocString DetailHolding = new LocString("brawl.detail.holding", "An enemy cast just completed, so no keys are pressed until it lands.");

        public static readonly LocString TitleStalled = new LocString("brawl.title.stalled", "The input bar stopped responding.");

        public static readonly LocString DetailStalled = new LocString("brawl.detail.stalled", "Inputs may be disabled for a moment. Retrying.");

        public static readonly LocString PhaseIdle = new LocString("brawl.phase.idle", "Idle");

        public static readonly LocString PhaseMatch = new LocString("brawl.phase.match", "Waiting for a match");

        public static readonly LocString PhaseFocus = new LocString("brawl.phase.focus", "Input bar not focused");

        public static readonly LocString PhaseWave = new LocString("brawl.phase.wave", "Between waves");

        public static readonly LocString PhaseHolding = new LocString("brawl.phase.holding", "Holding for a cast");

        public static readonly LocString PhaseTyping = new LocString("brawl.phase.typing", "Typing");

        public static readonly LocString PhaseClearing = new LocString("brawl.phase.clearing", "Clearing the input bar");

        public static readonly LocString PhaseStalled = new LocString("brawl.phase.stalled", "Stalled");

        public static readonly LocString TilePhrases = new LocString("brawl.tile.phrases", "Phrases");

        public static readonly LocString TileSpeed = new LocString("brawl.tile.speed", "Speed");

        public static readonly LocString TileSpeedValue = new LocString("brawl.tile.speedValue", "{0} WPM");

        public static readonly LocString TileSession = new LocString("brawl.tile.session", "Session");

        public static readonly LocString TileMgc = new LocString("brawl.tile.mgc", "MGC");

        public static readonly LocString TileMgp = new LocString("brawl.tile.mgp", "MGP");

        public static readonly LocString Board = new LocString("brawl.board", "On screen");

        public static readonly LocString InputFocused = new LocString("brawl.input.focused", "Focused");

        public static readonly LocString InputUnfocused = new LocString("brawl.input.unfocused", "Not focused");

        public static readonly LocString InputOther = new LocString("brawl.input.other", "Another box has focus");

        public static readonly LocString InputClosed = new LocString("brawl.input.closed", "Not open");

        public static readonly LocString SnapshotDone = new LocString("brawl.snapshotDone", "Snapshot written to the Console.");

        public static readonly LocString BoardEmpty = new LocString("brawl.boardEmpty", "Phrases show up here during a match.");

        public static readonly LocString LegendBarrier = new LocString("brawl.legend.barrier", "Barrier");

        public static readonly LocString LegendCasting = new LocString("brawl.legend.casting", "Casting");

        public static readonly LocString LegendLetterlock = new LocString("brawl.legend.letterlock", "Letterlock");

        public static readonly LocString SnapshotHint = new LocString("brawl.snapshotHint", "Writes the brawler's on-screen layout to the Console for a bug report.");

        public static readonly LocString Quick = new LocString("brawl.quick", "Quick settings");

        public static readonly LocString AllSettings = new LocString("brawl.allSettings", "All settings");
    }

    internal static class Settings
    {
        public static readonly LocString CatGeneral = new LocString("settings.cat.general", "General");

        public static readonly LocString CatTyping = new LocString("settings.cat.typing", "Typing");

        public static readonly LocString Language = new LocString("settings.language", "Language");

        public static readonly LocString LanguageHelp = new LocString("settings.languageHelpBrawl", "Language for this plugin's interface. Phrases are always typed exactly as the game shows them.");

        public static readonly LocString GroupWindow = new LocString("settings.group.window", "Window");

        public static readonly LocString GroupTheme = new LocString("settings.group.theme", "Theme");

        public static readonly LocString Theme = new LocString("settings.theme", "Theme");

        public static readonly LocString ThemeHelp = new LocString("settings.themeHelp", "Colours for the whole window. Liquid, Eorzea, Aurora and the Clear themes are see-through glass over the game.");

        public static readonly LocString Appearance = new LocString("settings.appearance", "Appearance");

        public static readonly LocString AppearanceHelp = new LocString("settings.appearanceHelp", "The dark or light version of the theme.");

        public static readonly LocString AppearanceDark = new LocString("settings.appearanceDark", "Dark");

        public static readonly LocString AppearanceLight = new LocString("settings.appearanceLight", "Light");

        public static readonly LocString ThemesBy = new LocString("settings.themesBy", "Themes by Glasswork");

        public static readonly LocString DragAnywhere = new LocString("settings.dragAnywhere", "Drag from anywhere");

        public static readonly LocString DragAnywhereHelp = new LocString("settings.dragAnywhereHelp", "Move the window by dragging anywhere inside it. When off, only the top bar moves it.");

        public static readonly LocString GroupSpeed = new LocString("settings.group.speed", "Speed");

        public static readonly LocString Speed = new LocString("settings.speed", "Typing speed");

        public static readonly LocString SpeedHelp = new LocString("settings.speedHelp", "How quickly keys are sent. Each key waits until the input bar shows it before the next one goes out.");

        public static readonly LocString SpeedRelaxed = new LocString("settings.speed.relaxed", "Relaxed");

        public static readonly LocString SpeedRelaxedDetail = new LocString("settings.speed.relaxedDetail", "About 60 words a minute, with uneven gaps like a person typing.");

        public static readonly LocString SpeedSteady = new LocString("settings.speed.steady", "Steady");

        public static readonly LocString SpeedSteadyDetail = new LocString("settings.speed.steadyDetail", "About 100 words a minute.");

        public static readonly LocString SpeedFast = new LocString("settings.speed.fast", "Fast");

        public static readonly LocString SpeedFastDetail = new LocString("settings.speed.fastDetail", "About 160 words a minute, a very quick human pace.");

        public static readonly LocString SpeedBlazing = new LocString("settings.speed.blazing", "Blazing");

        public static readonly LocString SpeedBlazingDetail = new LocString("settings.speed.blazingDetail", "As fast as the input bar accepts keys, far beyond any person.");

        public static readonly LocString GroupInput = new LocString("settings.group.input", "Input bar");

        public static readonly LocString InputMethod = new LocString("settings.inputMethod", "Input method");

        public static readonly LocString InputMethodHelp = new LocString("settings.inputMethodHelp", "How each finished word reaches the game. Type keys presses its letters into the input bar and then Enter, like a player at the keyboard. Submit directly waits as long as typing the word would take at your speed, humanizer pauses included, then hands it straight to the game, so the input bar never needs focus. Both hold still for Letterlock casts. If the game's word data cannot be read, typing takes over for the rest of the session.");

        public static readonly LocString MethodTyping = new LocString("settings.inputMethod.typing", "Type keys");

        public static readonly LocString MethodTypingDetail = new LocString("settings.inputMethod.typingDetail", "Presses each letter into the input bar, then Enter.");

        public static readonly LocString MethodDirect = new LocString("settings.inputMethod.direct", "Submit directly");

        public static readonly LocString MethodDirectDetail = new LocString("settings.inputMethod.directDetail", "Waits as long as typing would take, then hands the word to the game.");

        public static readonly LocString AutoFocus = new LocString("settings.autoFocus", "Keep the input bar focused");

        public static readonly LocString AutoFocusHelp = new LocString("settings.autoFocusKeyHelp", "Brings the input bar back whenever it loses focus during a match, the way a player does: by pressing the first key of the next phrase. It never takes focus from chat or another text box while you are typing in it.");

        public static readonly LocString GroupTargets = new LocString("settings.group.targets", "Targets");

        public static readonly LocString Order = new LocString("settings.order", "Target priority");

        public static readonly LocString OrderHelp = new LocString("settings.orderHelp", "Which enemy's phrase is typed first. Barrier phrases and enemies whose spell can be interrupted always come before this order.");

        public static readonly LocString OrderSmart = new LocString("settings.order.smart", "Smart");

        public static readonly LocString OrderSmartDetail = new LocString("settings.order.smartDetail", "The enemy closest to the barrier first, switching to the shortest phrase first once the screen gets crowded.");

        public static readonly LocString OrderClosest = new LocString("settings.order.closest", "Closest first");

        public static readonly LocString OrderClosestDetail = new LocString("settings.order.closestDetail", "Always the enemy that has walked closest to the barrier first.");

        public static readonly LocString OrderShortest = new LocString("settings.order.shortest", "Shortest first");

        public static readonly LocString OrderShortestDetail = new LocString("settings.order.shortestDetail", "Always the shortest phrase on screen first, to thin out the crowd quickly.");

        public static readonly LocString CrowdedAt = new LocString("settings.crowdedAt", "Crowded at");

        public static readonly LocString CrowdedAtHelp = new LocString("settings.crowdedAtHelp", "How many phrases on screen count as crowded, the point where Smart starts typing the shortest phrases first.");

        public static readonly LocString CrowdedAtFormat = new LocString("settings.crowdedAtFormat", "%d phrases");

        public static readonly LocString GroupCasts = new LocString("settings.group.casts", "Enemy casts");

        public static readonly LocString Letterlock = new LocString("settings.letterlockNamed", "Hold still for Letterlock casts");

        public static readonly LocString LetterlockHelp = new LocString("settings.letterlockTimedHelp", "Stops pressing keys from just before a Letterlock cast such as Cursed Gaze completes until its attack lands, so it cannot lock your inputs, and for a moment after any other cast completes.");

        public static readonly LocString CastsFootnote = new LocString("settings.castsFootnote", "Words over the barrier are always typed first so the shield is charged before a Barrier Buster lands, and an enemy whose spell can be interrupted is hit until the spell stops.");

        public static readonly LocString GroupDiagnostics = new LocString("settings.group.diagnostics", "Diagnostics");

        public static readonly LocString Snapshots = new LocString("settings.snapshots", "Record UI snapshots");

        public static readonly LocString SnapshotsHelp = new LocString("settings.snapshotsHelp", "Writes the brawler's on-screen layout to the Console a second into the first cast of every spell, and the first time a barrier phrase or a full cast shows up, so a log sent with a bug report shows what the plugin saw.");

        public static readonly LocString SafetyFootnote = new LocString("settings.safetyFootnote", "Typing pauses whenever the input bar loses focus, so a phrase is never typed into the chat box.");

        public static readonly LocString GroupQueue = new LocString("settings.group.queue", "Auto queue");

        public static readonly LocString QueueFootnote = new LocString("settings.queueFootnote", "Start near the registrar at Keybound Square. After every match the results are closed and the next registration is made for you.");

        public static readonly LocString AutoQueue = new LocString("settings.autoQueue", "Auto queue");

        public static readonly LocString AutoQueueHelp = new LocString("settings.autoQueueHelp", "While auto typing is on, closes the results after each match and registers for the next one at the registrar, so matches run back to back.");

        public static readonly LocString WalkToRegistrar = new LocString("settings.walkToRegistrar", "Walk to the registrar");

        public static readonly LocString WalkToRegistrarHelp = new LocString("settings.walkToRegistrarHelp", "Walks to the registrar with vnavmesh when you stand more than 3 yalms away. When off, or without vnavmesh, auto queue waits for you to move closer.");

        public static readonly LocString StopAfterMatches = new LocString("settings.stopAfterMatches", "Stop after");

        public static readonly LocString StopAfterMatchesHelp = new LocString("settings.stopAfterMatchesHelp", "Turns auto typing off once this many matches are done in this session. At 0 it never stops.");

        public static readonly LocPlural StopAfterMatchesCount = new LocPlural("settings.stopAfterMatchesCount", "{0} match", "{0} matches");

        public static readonly LocString StopAfterNever = new LocString("settings.stopAfterNever", "Never");

        public static readonly LocString StopAtMgcCap = new LocString("settings.stopAtMgcCap", "Stop near the MGC cap");

        public static readonly LocString StopAtMgcCapHelp = new LocString("settings.stopAtMgcCapHelp", "Turns auto typing off when another match could push your MGC past the 10,000 cap, so no coins go to waste.");

        public static readonly LocString Lifetime = new LocString("settings.lifetime", "Lifetime");

        public static readonly LocString LifetimeHelp = new LocString("settings.lifetimeHelp", "Every match the plugin has counted across all sessions, and the coins they paid.");

        public static readonly LocPlural LifetimeValue = new LocPlural("settings.lifetimeValue", "{0} match · {1} MGC · {2} MGP", "{0} matches · {1} MGC · {2} MGP");
    }

    internal static class Queue
    {
        public static readonly LocString InMatch = new LocString("queue.status.inMatch", "In a match.");

        public static readonly LocString ClosingResults = new LocString("queue.status.closingResults", "Closing the results.");

        public static readonly LocString ChangingAreas = new LocString("queue.status.changingAreas", "Changing areas.");

        public static readonly LocString CountingRewards = new LocString("queue.status.countingRewards", "Counting the rewards.");

        public static readonly LocString Entering = new LocString("queue.status.entering", "Entering the match.");

        public static readonly LocString InQueue = new LocString("queue.status.inQueue", "In the queue for the next match.");

        public static readonly LocString FindingRegistrar = new LocString("queue.status.findingRegistrar", "Go to the registrar at Keybound Square.");

        public static readonly LocString WaitingForNavmesh = new LocString("queue.status.waitingForNavmesh", "Waiting for vnavmesh to load the area.");

        public static readonly LocString Walking = new LocString("queue.status.walking", "Walking to the registrar.");

        public static readonly LocString MoveCloser = new LocString("queue.status.moveCloser", "Move closer to the registrar.");

        public static readonly LocString Busy = new LocString("queue.status.busy", "Waiting for your character to be free.");

        public static readonly LocString Talking = new LocString("queue.status.talking", "Talking to the registrar.");

        public static readonly LocString Registering = new LocString("queue.status.registering", "Registering for the next match.");

        public static readonly LocString Retrying = new LocString("queue.status.retrying", "Trying again in a moment.");

        public static readonly LocString StoppedMatchLimit = new LocString("queue.status.stoppedMatchLimit", "Stopped: match limit reached.");

        public static readonly LocString StoppedMgcCap = new LocString("queue.status.stoppedMgcCap", "Stopped: MGC cap reached.");

        public static readonly LocString StoppedRegistration = new LocString("queue.status.stoppedRegistration", "Stopped: could not register.");

        public static readonly LocString DetailRunning = new LocString("queue.detail.running", "Auto queue joins match after match until you press Stop or a limit is reached.");

        public static readonly LocString DetailStopped = new LocString("queue.detail.stopped", "Auto typing turned off. Press Start to carry on.");
    }

    internal static class Humanizer
    {
        public static readonly LocString Tab = new LocString("humanizer.tab", "Humanizer");

        public static readonly LocString Group = new LocString("humanizer.group", "Reaction delays");

        public static readonly LocString Enabled = new LocString("humanizer.enabled", "Humanize reactions");

        public static readonly LocString EnabledHelp = new LocString("humanizer.enabledHelp", "Wait a random moment before each action below, drawn anew each time between the shortest and the longest value. A shortest value above the longest is used as a fixed wait. Off acts at once.");

        public static readonly LocString RangeTo = new LocString("humanizer.rangeTo", "to");

        public static readonly LocString SecondsFormat = new LocString("humanizer.secondsFormatFine", "%.2f s");

        public static readonly LocString React = new LocString("humanizer.react", "Reading a new phrase");

        public static readonly LocString ReactHelp = new LocString("humanizer.reactHelp", "Before the first key of a fresh phrase, the moment a player takes to read it.");

        public static readonly LocString Submit = new LocString("humanizer.submit", "Before pressing Enter");

        public static readonly LocString SubmitHelp = new LocString("humanizer.submitHelp", "After the last letter, before the phrase is submitted. With the humanizer off, the typing speed's own short pause is used.");

        public static readonly LocString GroupSlips = new LocString("humanizer.group.slips", "Slips");

        public static readonly LocString Typos = new LocString("humanizer.typos", "Make the odd typo");

        public static readonly LocString TyposHelp = new LocString("humanizer.typosHelp", "Now and then a neighbouring key is hit by mistake, then erased with Backspace and fixed. Enter is only ever pressed on a correct phrase.");
    }

    internal static class Log
    {
        public static readonly LocString Title = new LocString("log.title", "Console");

        public static readonly LocPlural Entries = new LocPlural("log.entries", "{0} line in the buffer", "{0} lines in the buffer");

        public static readonly LocString Empty = new LocString("log.empty", "Nothing logged yet. Start a run and every step the plugin takes shows up here.");

        public static readonly LocString NoMatches = new LocString("log.noMatches", "No lines match the current filters.");

        public static readonly LocString Footer = new LocString("log.footer", "Every line also goes to the Dalamud log (/xllog) with the {0} prefix. When reporting a bug, press Copy log and paste the result into the issue.");

        public static readonly LocString SearchHint = new LocString("log.searchHint", "Search messages and sources");

        public static readonly LocString CopyAll = new LocString("log.copyAll", "Copy log");

        public static readonly LocPlural CopyFiltered = new LocPlural("log.copyFiltered", "Copy {0} line", "Copy {0} lines");

        public static readonly LocString CopyTooltip = new LocString("log.copyTooltip", "Copies the lines shown below with a header naming the plugin version, Dalamud version and zone, ready to paste into a bug report.");

        public static readonly LocString Copied = new LocString("log.copied", "Copied");

        public static readonly LocPlural CopiedLines = new LocPlural("log.copiedLines", "Copied {0} line to the clipboard", "Copied {0} lines to the clipboard");

        public static readonly LocString Clear = new LocString("log.clear", "Clear");

        public static readonly LocString ConfirmClear = new LocString("log.confirmClear", "Click again to clear");

        public static readonly LocString Close = new LocString("log.close", "Close");

        public static readonly LocString LevelVerbose = new LocString("log.level.verbose", "Verbose");

        public static readonly LocString LevelDebug = new LocString("log.level.debug", "Debug");

        public static readonly LocString LevelInfo = new LocString("log.level.info", "Info");

        public static readonly LocString LevelWarning = new LocString("log.level.warning", "Warnings");

        public static readonly LocString LevelError = new LocString("log.level.error", "Errors");

        public static readonly LocString LevelTooltip = new LocString("log.levelTooltip", "Click to show or hide these lines. Shift-click to show only this level.");

        public static readonly LocString SourceChip = new LocString("log.sourceChip", "Source: {0}");

        public static readonly LocString SourceChipTooltip = new LocString("log.sourceChipTooltip", "Click to stop filtering by source.");

        public static readonly LocString JumpLatest = new LocString("log.jumpLatest", "Jump to latest");

        public static readonly LocPlural NewLines = new LocPlural("log.newLines", "{0} new line", "{0} new lines");

        public static readonly LocString Showing = new LocString("log.showing", "Showing {0} of {1}");

        public static readonly LocString ResetFilters = new LocString("log.resetFilters", "Reset filters");

        public static readonly LocString Shortcuts = new LocString("log.shortcuts", "Ctrl+F search · Ctrl+C copy · Shift-click selects a range · Double-click copies a line");

        public static readonly LocPlural Selected = new LocPlural("log.selected", "{0} line selected", "{0} lines selected");

        public static readonly LocString CopySelection = new LocString("log.copySelection", "Copy selection");

        public static readonly LocString ClearSelection = new LocString("log.clearSelection", "Clear selection");

        public static readonly LocString CopyLine = new LocString("log.copyLine", "Copy line");

        public static readonly LocString CopyToEnd = new LocString("log.copyToEnd", "Copy from here to the end");

        public static readonly LocString OnlySource = new LocString("log.onlySource", "Show only {0}");

        public static readonly LocString Repeated = new LocString("log.repeated", "Repeated {0} times in a row");

        public static readonly LocString HasDetails = new LocString("log.hasDetails", "Has a stack trace. Select the line to read it.");
    }

    internal static class Changelog
    {
        public static readonly LocString Title = new LocString("changelog.title", "What's new");

        public static readonly LocString Subtitle = new LocString("changelog.subtitle", "Every update, newest first.");

        public static readonly LocString Version = new LocString("changelog.version", "Version {0}");

        public static readonly LocString Latest = new LocString("changelog.latest", "Latest");

        public static readonly LocString New = new LocString("changelog.new", "New");

        public static readonly LocPlural Changes = new LocPlural("changelog.changes", "{0} change", "{0} changes");

        public static readonly LocString[] Release1520 = new LocString[7]
        {
            new LocString("changelog.r1520.1", "Submit directly is now the default input method, and everyone is switched to it once: it needs no input bar focus, so there is no more clicking the bar at the start of a match or a wave. Type keys is still under Settings, Typing"),
            new LocString("changelog.r1520.2", "Type keys pauses while a text box in a plugin window is in use, so phrases are no longer typed into it"),
            new LocString("changelog.r1520.3", "Letterlock: keys now stay still for three and a half seconds after Cursed Gaze or Dynamic Sensory Jammer completes instead of two, since typing started again just before the lockout landed"),
            new LocString("changelog.r1520.4", "Auto queue measures its reach to the registrar between hitboxes, so it registers from the front of the counter instead of running into the desk or waiting a few yalms away"),
            new LocString("changelog.r1520.5", "The Console notes your statuses whenever they change during a match, to find the status the Letterlock lockout puts on you"),
            new LocString("changelog.r1520.6", "Keys also stay still for as long as Cursed Gaze's status is on you, so a gaze that lands late, on a slow connection for example, no longer locks your inputs"),
            new LocString("changelog.r1520.7", "The Language and Theme menus and the Console's search box no longer show a stray internal name such as ##akb_language")
        };

        public static readonly LocString[] Release1510 = new LocString[2]
        {
            new LocString("changelog.r1510.1", "A phrase only a few letters from done is finished before switching to a more urgent one, instead of being deleted and typed again later"),
            new LocString("changelog.r1510.2", "UI snapshots are written one line per window, so they survive being pasted into a bug report")
        };

        public static readonly LocString[] Release1500 = new LocString[6]
        {
            new LocString("changelog.r1500.1", "Auto queue: closes the results, walks to the Keybound registrar (with vnavmesh when installed), registers and accepts the duty, match after match. It can stop after a number of matches or before the MGC cap"),
            new LocString("changelog.r1500.2", "Reads the brawler's own word list: barrier words, casting enemies and Japanese readings now come straight from the game instead of the screen layout, and words of defeated enemies are skipped"),
            new LocString("changelog.r1500.3", "Closest first now uses each enemy's real distance to you"),
            new LocString("changelog.r1500.4", "Letterlock is timed from the enemy's own cast: keys stop just before Cursed Gaze or Dynamic Sensory Jammer completes and resume two seconds after"),
            new LocString("changelog.r1500.5", "New input method under Settings, Typing: Submit directly sends each word to the game after a typing-length wait, with no input bar focus needed"),
            new LocString("changelog.r1500.6", "MGC and MGP earned show in the status card, with lifetime totals under Settings, Typing")
        };

        public static readonly LocString[] Release1440 = new LocString[2]
        {
            new LocString("changelog.r1440.1", "Fixed being expelled for inactivity while the plugin was typing: the duty keeps an idle timer of its own, and it is now reset during the match too"),
            new LocString("changelog.r1440.2", "Dynamic Sensory Jammer is handled like Cursed Gaze: no keys are pressed while it lands")
        };

        public static readonly LocString[] Release1430 = new LocString[5]
        {
            new LocString("changelog.r1430.3", "Bigger text on the Brawl page: the quick settings, their labels and the All settings button now match the size of the rest of the window, and follow Dalamud's font size"),
            new LocString("changelog.r1430.4", "Tooltips and option labels are as large as the rest of the window's text, and the top bar and the Start bar now use the theme's bar style: real glass on the glass themes"),
            new LocString("changelog.r1430.5", "The whole window is now built from Glasswork: Start and Stop, the top bar, the side rail, every Settings control, the Console's buttons and every card, so each theme reaches all of it"),
            new LocString("changelog.r1430.1", "Controller layout: the controller and mouse inactivity timers are reset during a match too, since players on the controller layout were expelled while their words were landing"),
            new LocString("changelog.r1430.2", "The Console notes the game's inactivity timers every 15 seconds during a match, and Copy log no longer passes on characters that broke logs pasted into Discord")
        };

        public static readonly LocString[] Release1420 = new LocString[1]
        {
            new LocString("changelog.r1420.1", "UI snapshots are now on for everyone, so a log sent with a bug report shows what the plugin saw. They can be turned off under Settings, General, Diagnostics")
        };

        public static readonly LocString[] Release1410 = new LocString[3]
        {
            new LocString("changelog.r1410.2", "Fixed only the first barrier phrase being typed: every later one appears over the barrier's gauge and was taken for an enemy's. A phrase that appears where a barrier phrase just was is now typed as one"),
            new LocString("changelog.r1410.1", "With the humanizer on, the reading pause before a new phrase now applies to every word, including the next word of the enemy just hit"),
            new LocString("changelog.r1410.3", "The glass themes no longer light up under the cursor")
        };

        public static readonly LocString[] Release1400 = new LocString[7]
        {
            new LocString("changelog.r1400.1", "A cleaner Brawl page: one status card shows the phrase being typed with a progress ring, the Letterlock countdown and the session's numbers, next to the live board and quick settings, with far less text"),
            new LocString("changelog.r1400.2", "Themes: every Glasswork theme (Neutral, Catppuccin, Nord, Eorzea, the Clear themes and more), dark or light, under Settings, General"),
            new LocString("changelog.r1400.3", "Fixed Cursed Gaze still locking inputs: the hold no longer ends when the cast bar vanishes and now lasts 3.5 seconds after the bar fills"),
            new LocString("changelog.r1400.4", "Barrier words keep their top priority while a charge gauge fades in under them, and an enemy word can no longer turn into a barrier word mid-fight"),
            new LocString("changelog.r1400.5", "The input bar is woken with a real key press while the game window is in front, and focusing it no longer parks the controller hand cursor on the bar"),
            new LocString("changelog.r1400.6", "With Record UI snapshots on, the first cast of every spell and the first barrier words are written to the Console; it also names the last cast when focus is lost right after it"),
            new LocString("changelog.r1400.7", "Japanese client: the input bar is now woken even when the next phrase starts with kana or kanji; it used to stay unfocused and players were expelled for inactivity. A plugin window popped out of the game window hands the keyboard back to the game while phrases are up")
        };

        public static readonly LocString[] Release1320 = new LocString[4]
        {
            new LocString("changelog.r1320.1", "Fixed players still being expelled for inactivity: the match no longer counts as idle while auto typing is on, even while the typer waits for focus, a cast or the next wave"),
            new LocString("changelog.r1320.2", "The input bar is woken the way a player does it, by pressing the first key of the next phrase, so it no longer needs a click at the start of a wave or after a debuff"),
            new LocString("changelog.r1320.3", "Fixed words being typed again after they were already cleared: hidden words are only read while the obscuring noise is on the box"),
            new LocString("changelog.r1320.4", "Holds still for a moment after every enemy cast that completes, not only Cursed Gaze, so Letterlock casts are respected; the console names a cast when keys stop landing right after it")
        };

        public static readonly LocString[] Release1310 = new LocString[3]
        {
            new LocString("changelog.r1310.1", "Stopped typing a word a second time for nothing: enemies get a new word after every hit and the old one lingers for a moment, so it is now skipped briefly"),
            new LocString("changelog.r1310.2", "The next word of the enemy just hit is typed without a reading pause, which makes interrupts faster"),
            new LocString("changelog.r1310.3", "The focus warning only shows when the input bar really stays unfocused while words are up, not after every Enter or between waves")
        };

        public static readonly LocString[] Release1300 = new LocString[7]
        {
            new LocString("changelog.r1300.1", "The input bar is focused again on its own at the start of each wave and after a debuff, going through the same steps as a mouse click"),
            new LocString("changelog.r1300.2", "Every enemy with a cast bar now goes before plain enemies, interruptible ones and the cast closest to finishing first; cast bars and enemy HP bars are read from every part of their window"),
            new LocString("changelog.r1300.3", "While a debuff hides the phrases, the word being typed is finished from memory instead of being deleted and typed again"),
            new LocString("changelog.r1300.4", "Clearing keeps the letters that still match the next phrase and removes the rest in one burst, like holding Backspace"),
            new LocString("changelog.r1300.5", "Optional punctuation (? ! - ') is skipped, as the game's input rules allow"),
            new LocString("changelog.r1300.6", "The Brawl page lists the enemy casts it sees and has quick settings for speed, target priority, humanizer, focus and Letterlock"),
            new LocString("changelog.r1220.1", "Japanese clients: phrases are typed as their hiragana reading, the form the game checks, instead of the kanji or katakana shown on screen")
        };

        public static readonly LocString[] Release1210 = new LocString[4]
        {
            new LocString("changelog.r1210.1", "Fixed being expelled from the match for extended inactivity while the plugin was typing: its keys now count as activity, like a real keyboard"),
            new LocString("changelog.r1210.2", "Fixed letters sometimes going in twice after an enemy vanished or a phrase was rejected"),
            new LocString("changelog.r1210.3", "Phrases a boss hides (Cryptic Communications) are still typed, from what the box last showed"),
            new LocString("changelog.r1210.4", "Clearing the input bar is faster than typing, like holding Backspace, and the typer no longer turns itself off when inputs are disabled for a while")
        };

        public static readonly LocString[] Release1200 = new LocString[5]
        {
            new LocString("changelog.r1200.1", "Fixed the boss's phrase being taken for a barrier phrase, which kept it on top and left the shield uncharged during Barrier Busters"),
            new LocString("changelog.r1200.2", "A phrase typed 3 times in a row with no visible effect now steps aside for a few seconds so the other phrases get typed"),
            new LocString("changelog.r1200.3", "Recognises Letterlock casts such as Cursed Gaze by name and holds still only for those, instead of after every cast"),
            new LocString("changelog.r1200.4", "New Target priority setting: Smart, Closest first or Shortest first, with the crowd size where Smart switches to shortest (Settings, Typing)"),
            new LocString("changelog.r1200.5", "The console names every enemy cast as it starts, marking Letterlock and interruptible ones")
        };

        public static readonly LocString[] Release1100 = new LocString[5]
        {
            new LocString("changelog.r1100.1", "Types the words over the barrier first during a Barrier Buster, charging the shield before the attack lands"),
            new LocString("changelog.r1100.2", "Keeps hitting an enemy whose spell can be interrupted until the spell stops, the cast closest to going off first"),
            new LocString("changelog.r1100.3", "Holds still when a cast bar fills, so a Letterlock attack cannot lock your inputs (Settings, Typing)"),
            new LocString("changelog.r1100.4", "Hits the same enemy again right away instead of moving to another one after every word"),
            new LocString("changelog.r1100.5", "Optional UI snapshots in the Console (Settings, General) to help pin down new mechanics")
        };

        public static readonly LocString[] Release1000 = new LocString[3]
        {
            new LocString("changelog.r1000.1", "First release: types the phrases above the enemies in Keybound Brawler and presses Enter for you"),
            new LocString("changelog.r1000.2", "Clicks back into the input bar on its own whenever it loses focus, and never types into chat or another text box"),
            new LocString("changelog.r1000.3", "Four typing speeds and a humanizer with reaction pauses and the odd typo")
        };
    }

    internal static class About
    {
        public static readonly LocString Version = new LocString("about.version", "v {0}");

        public static readonly LocString MadeBy = new LocString("about.madeBy", "Made by {0}");

        public static readonly LocString Community = new LocString("about.community", "Community");

        public static readonly LocString GitHubTitle = new LocString("about.githubTitle", "View on GitHub");

        public static readonly LocString GitHubBody = new LocString("about.githubBody", "Browse the source code and every release.");

        public static readonly LocString ReminderTitle = new LocString("about.reminderTitle", "A little reminder");

        public static readonly LocString[] Reminders = new LocString[12]
        {
            new LocString("about.reminder.1", "Been at it a while? Roll your shoulders and take one slow breath."),
            new LocString("about.reminder.2", "Hydration check. When did you last drink some water?"),
            new LocString("about.reminder.3", "Blink a few times and let your eyes rest for a moment."),
            new LocString("about.reminder.4", "Stand up, stretch, and shake out your hands. Future you says thanks."),
            new LocString("about.reminder.5", "Sit up and settle in comfortably. Your back will thank you later."),
            new LocString("about.reminder.6", "Remember to eat something today. You matter more than any score."),
            new LocString("about.reminder.7", "Eyes feel tired? Look at something far away for twenty seconds."),
            new LocString("about.reminder.8", "Whatever you're chasing, you're allowed to take a break whenever."),
            new LocString("about.reminder.9", "You're doing great. Be a little kinder to yourself today."),
            new LocString("about.reminder.10", "A glass of water and a quick stretch can reset a long session."),
            new LocString("about.reminder.11", "Unclench your jaw and drop your shoulders. There you go."),
            new LocString("about.reminder.12", "Rest is part of the journey too. Step away whenever you need to.")
        };

        public static readonly LocString FactsTitle = new LocString("about.factsTitle", "Did you know?");

        public static readonly LocString[] Facts = new LocString[12]
        {
            new LocString("about.fact.1", "Honey never spoils. Jars over 3,000 years old have been found still edible."),
            new LocString("about.fact.2", "Octopuses have three hearts and blue blood."),
            new LocString("about.fact.3", "A day on Venus is longer than a whole year on Venus."),
            new LocString("about.fact.4", "Bananas are berries, but strawberries aren't."),
            new LocString("about.fact.5", "There are more possible chess games than atoms in the observable universe."),
            new LocString("about.fact.6", "Sharks have been around longer than trees have."),
            new LocString("about.fact.7", "A group of flamingos is called a flamboyance."),
            new LocString("about.fact.8", "Honeybees can recognize individual human faces."),
            new LocString("about.fact.9", "Wombat droppings are cube shaped."),
            new LocString("about.fact.10", "The Eiffel Tower can grow over 15 cm taller on a hot day."),
            new LocString("about.fact.11", "Hot water can sometimes freeze faster than cold water."),
            new LocString("about.fact.12", "A bolt of lightning is roughly five times hotter than the surface of the Sun.")
        };

        public static readonly LocString QuotesTitle = new LocString("about.quotesTitle", "Words to live by");

        public static readonly LocString[] Quotes = new LocString[10]
        {
            new LocString("about.quote.1", "Done is better than perfect. You can always polish later."),
            new LocString("about.quote.2", "Small steps every day add up to surprising distances."),
            new LocString("about.quote.3", "Comparison is the thief of joy. Run your own race."),
            new LocString("about.quote.4", "Progress, not perfection."),
            new LocString("about.quote.5", "You don't have to be great to start, but you have to start to be great."),
            new LocString("about.quote.6", "Be patient with yourself. Growth takes time."),
            new LocString("about.quote.7", "The best time to begin was yesterday. The second best is right now."),
            new LocString("about.quote.8", "Celebrate the small wins. They count too."),
            new LocString("about.quote.9", "Slow progress is still progress."),
            new LocString("about.quote.10", "Your only real competition is who you were yesterday.")
        };

        public static readonly LocString JokesTitle = new LocString("about.jokesTitle", "Just for fun");

        public static readonly LocString[] Jokes = new LocString[10]
        {
            new LocString("about.joke.1", "Why don't scientists trust atoms? Because they make up everything."),
            new LocString("about.joke.2", "I would tell you a chemistry joke, but I know I wouldn't get a reaction."),
            new LocString("about.joke.3", "Why did the scarecrow win an award? He was outstanding in his field."),
            new LocString("about.joke.4", "I'm reading a book about anti-gravity. It's impossible to put down."),
            new LocString("about.joke.5", "Why don't skeletons fight each other? They don't have the guts."),
            new LocString("about.joke.6", "What do you call fake spaghetti? An impasta."),
            new LocString("about.joke.7", "Why did the bicycle fall over? It was two tired."),
            new LocString("about.joke.8", "What do you call cheese that isn't yours? Nacho cheese."),
            new LocString("about.joke.9", "I'm on a seafood diet. I see food, and I eat it."),
            new LocString("about.joke.10", "I only know 25 letters of the alphabet. I don't know y.")
        };
    }

    internal static class Plugin
    {
        public static readonly LocString CommandHelp = new LocString("plugin.commandHelpBrawl", "Toggle the Keybound Brawler Autopilot window. /kba start | stop | toggle | config | log | changelog | about.");

        public static readonly LocString CommandHelpAlias = new LocString("plugin.commandHelpAlias", "Alias for /kba.");

        public static readonly LocString ChatStarted = new LocString("plugin.chatStarted", "Auto typing on. Queue for Keybound Brawler and the plugin takes it from there.");

        public static readonly LocString ChatStopped = new LocString("plugin.chatStopped", "Auto typing off.");
    }
}

internal enum PluralKind : byte
{
    EnglishLike,
    French
}
