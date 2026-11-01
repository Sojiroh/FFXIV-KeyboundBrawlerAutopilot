using System;

namespace AutoKeyboundBrawler.Core.Typing;

internal sealed class TypingSession
{
    private const double MillisecondsPerMinute = 60000.0;

    private const double CharactersPerWord = 5.0;

    private long startedAt;

    private long stoppedAt;

    private long typingMs;

    private long typingSince;

    public int PhrasesSubmitted { get; private set; }

    public int KeysSent { get; private set; }

    public int CharactersTyped { get; private set; }

    public int MatchesCompleted { get; private set; }

    public int MgcEarned { get; private set; }

    public int MgpEarned { get; private set; }

    public bool HasData => PhrasesSubmitted > 0 || KeysSent > 0;

    public void Begin(long now)
    {
        startedAt = now;
        stoppedAt = 0L;
        typingMs = 0L;
        typingSince = 0L;
        PhrasesSubmitted = 0;
        KeysSent = 0;
        CharactersTyped = 0;
        MatchesCompleted = 0;
        MgcEarned = 0;
        MgpEarned = 0;
    }

    public void End(long now)
    {
        PauseClock(now);
        stoppedAt = now;
    }

    public long ElapsedMs(long now)
    {
        if (startedAt != 0L)
        {
            return ((stoppedAt == 0L) ? now : stoppedAt) - startedAt;
        }
        return 0L;
    }

    public void ResumeClock(long now)
    {
        if (typingSince == 0L)
        {
            typingSince = now;
        }
    }

    public void PauseClock(long now)
    {
        if (typingSince == 0L)
        {
            return;
        }
        typingMs += now - typingSince;
        typingSince = 0L;
    }

    public void CountCharacter()
    {
        CharactersTyped++;
        KeysSent++;
    }

    public void CountKey()
    {
        KeysSent++;
    }

    public void CountTypedCharacters(int count)
    {
        CharactersTyped += count;
    }

    public void CountPhrase()
    {
        PhrasesSubmitted++;
    }

    public void CountMatch(int mgc, int mgp)
    {
        MatchesCompleted++;
        MgcEarned += mgc;
        MgpEarned += mgp;
    }

    public int WordsPerMinute(long now)
    {
        long elapsedMs = typingMs + ((typingSince == 0L) ? 0 : (now - typingSince));
        if (elapsedMs <= 0 || CharactersTyped == 0)
        {
            return 0;
        }
        return (int)Math.Round((double)CharactersTyped / CharactersPerWord / ((double)elapsedMs / MillisecondsPerMinute));
    }
}
