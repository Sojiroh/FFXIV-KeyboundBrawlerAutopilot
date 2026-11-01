using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace AutoKeyboundBrawler.Core.Game;

internal static partial class GameKeyboard
{
    [StructLayout(LayoutKind.Explicit, Size = InputSize)]
    private struct KeyboardInput
    {
        [FieldOffset(0)]
        public uint Type;

        [FieldOffset(8)]
        public ushort VirtualKey;

        [FieldOffset(10)]
        public ushort ScanCode;

        [FieldOffset(12)]
        public uint Flags;

        [FieldOffset(16)]
        public uint Time;

        [FieldOffset(24)]
        public nuint ExtraInfo;
    }

    private const uint KeyDownMessage = 256u;

    private const uint KeyUpMessage = 257u;

    private const uint CharMessage = 258u;

    private const int ScanCodeShift = 16;

    private const long SingleRepeat = 1L;

    private const long KeyUpTransitionBits = 3221225472L;

    private const long PreviouslyDownBit = 1073741824L;

    private const uint VirtualKeyToScanCode = 0u;

    private const byte FirstDigitKey = 48;

    private const byte FirstLetterKey = 65;

    private const byte SpaceKey = 32;

    private const uint KeyboardInputType = 1u;

    private const uint KeyUpFlag = 2u;

    private const int InputSize = 40;

    public static bool PostCharacter(char character) => Post(CharMessage, character, SingleRepeat);

    public static bool PostKeyDown(GameKey key) => Post(KeyDownMessage, key.VirtualKey, (long)(SingleRepeat | ((ulong)key.ScanCode << ScanCodeShift)));

    public static bool PostKeyRepeat(GameKey key) => Post(KeyDownMessage, key.VirtualKey, (long)(SingleRepeat | ((ulong)key.ScanCode << ScanCodeShift) | PreviouslyDownBit));

    public static bool PostKeyUp(GameKey key) => Post(KeyUpMessage, key.VirtualKey, (long)(SingleRepeat | ((ulong)key.ScanCode << ScanCodeShift) | KeyUpTransitionBits));

    public unsafe static bool SendSystemKey(GameKey key, bool keyUp)
    {
        var gameWindow = GameWindowHandle();
        if (!keyUp && (gameWindow == 0 || GetForegroundWindow() != gameWindow))
        {
            return false;
        }
        var keyboardInput = new KeyboardInput
        {
            Type = KeyboardInputType,
            VirtualKey = key.VirtualKey,
            ScanCode = key.ScanCode,
            Flags = keyUp ? KeyUpFlag : 0u
        };
        return SendInput(1u, &keyboardInput, InputSize) == 1;
    }

    public static void ReclaimForeground()
    {
        var gameWindow = GameWindowHandle();
        var foregroundWindow = GetForegroundWindow();
        if (gameWindow == 0 || foregroundWindow == 0 || foregroundWindow == gameWindow)
        {
            return;
        }
        GetWindowThreadProcessId(foregroundWindow, out var processId);
        if (processId == (uint)Environment.ProcessId)
        {
            SetForegroundWindow(gameWindow);
        }
    }

    public static bool TryKeyFor(char character, out GameKey key)
    {
        byte virtualKey;
        if (character >= 'a' && character <= 'z')
        {
            virtualKey = (byte)(FirstLetterKey + (character - 'a'));
        }
        else if (character >= 'A' && character <= 'Z')
        {
            virtualKey = (byte)(FirstLetterKey + (character - 'A'));
        }
        else if (character >= '0' && character <= '9')
        {
            virtualKey = (byte)(FirstDigitKey + (character - '0'));
        }
        else if (character == ' ')
        {
            virtualKey = SpaceKey;
        }
        else
        {
            key = default;
            return false;
        }
        key = new GameKey(virtualKey, (byte)MapVirtualKey(virtualKey, VirtualKeyToScanCode));
        return true;
    }

    private static bool Post(uint message, nuint wParam, long lParam)
    {
        var gameWindow = GameWindowHandle();
        return gameWindow != 0 && PostMessage(gameWindow, message, wParam, (nint)lParam);
    }

    private unsafe static nint GameWindowHandle()
    {
        var framework = Framework.Instance();
        if (framework != null && framework->GameWindow != null)
        {
            return framework->GameWindow->WindowHandle;
        }
        return 0;
    }

    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(nint windowHandle, uint message, nuint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    private unsafe static partial uint SendInput(uint count, KeyboardInput* inputs, int size);

    [LibraryImport("user32.dll")]
    private static partial nint GetForegroundWindow();

    [LibraryImport("user32.dll")]
    private static partial uint GetWindowThreadProcessId(nint windowHandle, out uint processId);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(nint windowHandle);

    [LibraryImport("user32.dll", EntryPoint = "MapVirtualKeyW")]
    private static partial uint MapVirtualKey(uint code, uint mapType);
}
