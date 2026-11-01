using System.IO;
using ECommons.DalamudServices;

namespace AutoKeyboundBrawler.Windows;

internal static class Assets
{
    public static readonly string ImagesRoot = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName ?? "", "Images");
}
