using System;
using Lumina.Text.ReadOnly;

namespace AutoKeyboundBrawler.Core.Game;

internal sealed class TextSlotCache
{
    private const int InitialBufferLength = 64;

    private readonly byte[][] rawBytes;

    private readonly int[] rawLengths;

    private readonly string[] decodedTexts;

    public TextSlotCache(int slotCount)
    {
        rawBytes = new byte[slotCount][];
        rawLengths = new int[slotCount];
        decodedTexts = new string[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            rawBytes[i] = new byte[InitialBufferLength];
            decodedTexts[i] = string.Empty;
        }
    }

    public string Decode(int slot, ReadOnlySpan<byte> bytes, bool singleLine)
    {
        if (bytes.SequenceEqual(rawBytes[slot].AsSpan(0, rawLengths[slot])))
        {
            return decodedTexts[slot];
        }
        if (rawBytes[slot].Length < bytes.Length)
        {
            rawBytes[slot] = new byte[Math.Max(bytes.Length, rawBytes[slot].Length * 2)];
        }
        bytes.CopyTo(rawBytes[slot]);
        rawLengths[slot] = bytes.Length;
        var text = bytes.IsEmpty ? string.Empty : new ReadOnlySeStringSpan(bytes).ExtractText();
        decodedTexts[slot] = singleLine ? ToSingleLine(text) : text;
        return decodedTexts[slot];
    }

    private static string ToSingleLine(string text)
    {
        if (text.IndexOfAny('\r', '\n') < 0)
        {
            return text.Trim();
        }
        return text.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ')
            .Trim();
    }
}
