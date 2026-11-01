using System;
using System.Diagnostics;
using AutoKeyboundBrawler.Core;
using Dalamud.Bindings.ImGui;

namespace AutoKeyboundBrawler.Windows;

internal static class UrlActions
{
    public static void Open(string url, bool log = true)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            if (log)
            {
                RunLog.Warning(exception, "failed to launch browser for " + url + ", copied to clipboard instead");
            }
            ImGui.SetClipboardText(url);
        }
    }
}
