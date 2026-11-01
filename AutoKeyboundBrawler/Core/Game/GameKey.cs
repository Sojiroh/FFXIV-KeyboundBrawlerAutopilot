namespace AutoKeyboundBrawler.Core.Game;

internal readonly record struct GameKey(byte VirtualKey, byte ScanCode)
{
    public static readonly GameKey Enter = new GameKey(13, 28);

    public static readonly GameKey Backspace = new GameKey(8, 14);
}
