namespace AutoKeyboundBrawler.Core.Game;

internal sealed class BattleSnapshot
{
    public const int MaxPhrases = 16;

    public const int MaxCasts = 8;

    public const int MaxGauges = 16;

    public const int InputSlot = 16;

    public const int FirstCastSlot = 17;

    public const int TextSlotCount = 25;

    public const int NoCast = -1;

    public const int NoGauge = -1;

    public const float NoDistance = -1f;

    public const uint NoEntity = 0u;

    public readonly string[] Phrases = new string[MaxPhrases];

    public readonly string[] PhraseTyped = new string[MaxPhrases];

    public readonly float[] PhraseScreenX = new float[MaxPhrases];

    public readonly float[] PhraseScreenY = new float[MaxPhrases];

    public readonly float[] PhraseScale = new float[MaxPhrases];

    public readonly bool[] PhraseOnBarrier = new bool[MaxPhrases];

    public readonly int[] PhraseCast = new int[MaxPhrases];

    public readonly int[] PhraseGauge = new int[MaxPhrases];

    public readonly uint[] PhraseEntityId = new uint[MaxPhrases];

    public readonly bool[] PhraseEntityCasting = new bool[MaxPhrases];

    public readonly float[] PhraseEntityCastProgress = new float[MaxPhrases];

    public readonly bool[] PhraseDefeated = new bool[MaxPhrases];

    public readonly float[] PhraseDistance = new float[MaxPhrases];

    public readonly nint[] CastIds = new nint[MaxCasts];

    public readonly string[] CastNames = new string[MaxCasts];

    public readonly float[] CastScreenX = new float[MaxCasts];

    public readonly float[] CastScreenY = new float[MaxCasts];

    public readonly float[] CastProgress = new float[MaxCasts];

    public readonly int[] CastHits = new int[MaxCasts];

    public readonly bool[] CastInterrupted = new bool[MaxCasts];

    public readonly float[] GaugeScreenX = new float[MaxGauges];

    public readonly float[] GaugeScreenY = new float[MaxGauges];

    public readonly float[] GaugeSignature = new float[MaxGauges];

    public int PhraseCount;

    public int CastCount;

    public int GaugeCount;

    public bool InputVisible;

    public float InputScreenY;

    public bool InputFocused;

    public bool OtherInputFocused;

    public string InputText = string.Empty;

    public float ProgressSignature(int phrase)
    {
        var gauge = PhraseGauge[phrase];
        var cast = PhraseCast[phrase];
        var gaugeSignature = gauge == NoGauge ? 0f : GaugeSignature[gauge];
        if (cast != NoCast)
        {
            return gaugeSignature + CastHits[cast] * 10000f;
        }
        return gaugeSignature;
    }

    public bool IsLinked(int phrase) => PhraseEntityId[phrase] != NoEntity;

    public bool HasDistance(int phrase) => PhraseDistance[phrase] >= 0f;

    public bool IsCasting(int phrase)
    {
        var cast = PhraseCast[phrase];
        if (cast == NoCast)
        {
            return PhraseEntityCasting[phrase];
        }
        return !CastInterrupted[cast];
    }

    public bool IsInterruptible(int phrase)
    {
        var cast = PhraseCast[phrase];
        return cast != NoCast && CastHits[cast] > 0 && !CastInterrupted[cast];
    }

    public float CastProgressOf(int phrase)
    {
        var cast = PhraseCast[phrase];
        if (cast == NoCast)
        {
            return PhraseEntityCastProgress[phrase];
        }
        return CastProgress[cast];
    }

    public void Clear()
    {
        PhraseCount = 0;
        CastCount = 0;
        GaugeCount = 0;
        InputVisible = false;
        InputFocused = false;
        OtherInputFocused = false;
        InputText = string.Empty;
    }
}
