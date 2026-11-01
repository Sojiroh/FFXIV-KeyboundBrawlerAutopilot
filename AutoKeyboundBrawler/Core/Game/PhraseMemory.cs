using System;

namespace AutoKeyboundBrawler.Core.Game;

internal sealed class PhraseMemory
{
    private const int Capacity = 32;

    private readonly nint[] boxes = new nint[Capacity];

    private readonly string[] texts = new string[Capacity];

    private int nextSlot;

    public void Remember(nint box, string text)
    {
        var slot = SlotOf(box);
        if (slot < 0)
        {
            slot = nextSlot;
            nextSlot = (nextSlot + 1) % Capacity;
            boxes[slot] = box;
        }
        texts[slot] = text;
    }

    public string? Recall(nint box)
    {
        var slot = SlotOf(box);
        if (slot >= 0)
        {
            return texts[slot];
        }
        return null;
    }

    public void Clear()
    {
        Array.Clear(boxes);
        Array.Clear(texts);
        nextSlot = 0;
    }

    private int SlotOf(nint box)
    {
        for (int i = 0; i < Capacity; i++)
        {
            if (boxes[i] == box)
            {
                return i;
            }
        }
        return -1;
    }
}
