using System;
using System.Globalization;
using System.IO;
using AutoKeyboundBrawler.Core;
using AutoKeyboundBrawler.Core.Localization;
using AutoKeyboundBrawler.Core.Typing;
using AutoKeyboundBrawler.Windows;
using AutoKeyboundBrawler.Windows.Shell;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.DalamudServices;
using Glasswork.Dalamud;

namespace AutoKeyboundBrawler;

public sealed class Plugin : IDalamudPlugin, IDisposable
{
    private readonly AppWindow appWindow;

    private readonly CommandInfo primaryCommand;

    private readonly CommandInfo aliasCommand;

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    internal Configuration Configuration { get; }

    internal static Configuration Cfg { get; private set; } = null!;

    internal static Plugin Instance { get; private set; } = null!;

    internal WindowSystem WindowSystem { get; } = new WindowSystem("KeyboundBrawlerAutopilot");

    internal AutoTyper Typer { get; }

    internal GlassworkHost GlassworkHost { get; }

    private static string PluginDirectory => PluginInterface.AssemblyLocation.DirectoryName ?? string.Empty;

    public Plugin()
    {
        ECommonsMain.Init(PluginInterface, this);
        Instance = this;
        Configuration = (PluginInterface.GetPluginConfig() as Configuration) ?? new Configuration();
        Configuration.Migrate();
        Cfg = Configuration;
        Typer = new AutoTyper(Configuration);
        InitializeLocalization();
        Fonts.Initialize(PluginInterface.UiBuilder, PluginDirectory);
        GlassworkHost = new GlassworkHost(PluginInterface, Svc.Log);
        GlassworkHost.Glass.PointerLight = false;
        Fonts.ShareWith(GlassworkHost.Fonts, PluginDirectory);
        ApplyTheme();
        appWindow = new AppWindow(this);
        WindowSystem.AddWindow(appWindow);
        primaryCommand = new CommandInfo(OnCommand)
        {
            HelpMessage = Loc.T(L.Plugin.CommandHelp)
        };
        aliasCommand = new CommandInfo(OnCommand)
        {
            HelpMessage = Loc.T(L.Plugin.CommandHelpAlias)
        };
        CommandManager.AddHandler(AkbConstants.PrimaryCommand, primaryCommand);
        CommandManager.AddHandler(AkbConstants.AliasCommand, aliasCommand);
        PluginInterface.UiBuilder.Draw += OnDraw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
    }

    public void Dispose()
    {
        Typer.Stop();
        Typer.Dispose();
        PluginInterface.UiBuilder.Draw -= OnDraw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        WindowSystem.RemoveAllWindows();
        appWindow.Dispose();
        GlassworkHost.Dispose();
        Fonts.Dispose();
        CommandManager.RemoveHandler(AkbConstants.PrimaryCommand);
        CommandManager.RemoveHandler(AkbConstants.AliasCommand);
        ECommonsMain.Dispose();
    }

    public void ApplyTheme()
    {
        Themes.Apply(GlassworkHost, Configuration.Theme, Configuration.LightTheme);
    }

    private void OnDraw()
    {
        Themes.SyncTextScale(GlassworkHost);
        GlassworkHost.BeginFrame();
        WindowSystem.Draw();
        GlassworkHost.EndFrame();
    }

    public void OnLanguageChanged()
    {
        primaryCommand.HelpMessage = Loc.T(L.Plugin.CommandHelp);
        aliasCommand.HelpMessage = Loc.T(L.Plugin.CommandHelpAlias);
    }

    private static void InitializeLocalization()
    {
        string localizationDirectory = Path.Combine(PluginDirectory, "Localization");
        if (string.IsNullOrEmpty(Cfg.Language))
        {
            Cfg.Language = DetectLanguage();
            Cfg.Save();
        }
        Loc.Initialize(Cfg.Language, localizationDirectory);
    }

    private static string DetectLanguage()
    {
        string uiLanguage = PluginInterface.UiLanguage;
        if (Languages.IsKnown(uiLanguage))
        {
            return Languages.Resolve(uiLanguage).Code;
        }
        string isoCode;
        switch (Svc.ClientState.ClientLanguage)
        {
            case ClientLanguage.German:
                return Languages.German.Code;
            case ClientLanguage.French:
                return Languages.French.Code;
            case ClientLanguage.Japanese:
                return Languages.Japanese.Code;
            default:
                isoCode = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
                if (!Languages.IsKnown(isoCode))
                {
                    return Languages.English.Code;
                }
                return Languages.Resolve(isoCode).Code;
        }
    }

    private void OnCommand(string command, string args)
    {
        string argument = args.Trim();
        if (argument.Equals("start", StringComparison.OrdinalIgnoreCase) || argument.Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            SetTyping(running: true);
        }
        else if (argument.Equals("stop", StringComparison.OrdinalIgnoreCase) || argument.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            SetTyping(running: false);
        }
        else if (argument.Equals("toggle", StringComparison.OrdinalIgnoreCase))
        {
            SetTyping(!Typer.Running);
        }
        else if (argument.Equals("config", StringComparison.OrdinalIgnoreCase))
        {
            ToggleConfigUi();
        }
        else if (argument.Equals("about", StringComparison.OrdinalIgnoreCase))
        {
            ToggleAboutUi();
        }
        else if (argument.Equals("log", StringComparison.OrdinalIgnoreCase))
        {
            ToggleLogUi();
        }
        else if (argument.Equals("changelog", StringComparison.OrdinalIgnoreCase))
        {
            ToggleChangelogUi();
        }
        else
        {
            ToggleMainUi();
        }
    }

    private void SetTyping(bool running)
    {
        if (running)
        {
            Typer.Start();
        }
        else
        {
            Typer.Stop();
        }
        Svc.Chat.Print(Loc.T(running ? L.Plugin.ChatStarted : L.Plugin.ChatStopped), AkbConstants.ChatTag);
    }

    public void ToggleMainUi()
    {
        appWindow.Toggle();
    }

    public void ToggleConfigUi()
    {
        appWindow.TogglePage(AppWindow.Page.Settings);
    }

    public void ToggleAboutUi()
    {
        appWindow.TogglePage(AppWindow.Page.About);
    }

    public void ToggleLogUi()
    {
        appWindow.TogglePage(AppWindow.Page.Log);
    }

    public void ToggleChangelogUi()
    {
        appWindow.TogglePage(AppWindow.Page.Changelog);
    }
}
