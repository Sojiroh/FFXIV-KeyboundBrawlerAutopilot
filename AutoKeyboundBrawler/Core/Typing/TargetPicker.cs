using System;
using AutoKeyboundBrawler.Core.Game;

namespace AutoKeyboundBrawler.Core.Typing;

internal readonly record struct TargetRules(TargetOrder Order, int CrowdedAt, string? Demoted, string? Skipped, bool LinkedOnly);

internal static class TargetPicker
{
    public const int NotShown = -1;

    public static readonly TargetOrder[] Orders = new TargetOrder[3]
    {
        TargetOrder.Smart,
        TargetOrder.ClosestFirst,
        TargetOrder.ShortestFirst
    };

    private const int RankDemoted = -1;

    private const int RankNormal = 0;

    private const int RankCasting = 1;

    private const int RankInterrupt = 2;

    private const int RankBarrier = 3;

    public static int IndexOf(BattleSnapshot snapshot, string target, bool linkedOnly = false)
    {
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            if (IsCandidate(snapshot, i, linkedOnly))
            {
                string phrase = snapshot.Phrases[i];
                if (phrase.Equals(target, StringComparison.OrdinalIgnoreCase) || target.EndsWith(phrase, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public static int Rank(BattleSnapshot snapshot, int index, in TargetRules rules)
    {
        if (rules.Demoted != null && snapshot.Phrases[index].Equals(rules.Demoted, StringComparison.OrdinalIgnoreCase))
        {
            return RankDemoted;
        }
        if (snapshot.PhraseOnBarrier[index])
        {
            return RankBarrier;
        }
        if (snapshot.IsInterruptible(index))
        {
            return RankInterrupt;
        }
        return snapshot.IsCasting(index) ? RankCasting : RankNormal;
    }

    public static bool IsPriority(BattleSnapshot snapshot, int index, in TargetRules rules) => Rank(snapshot, index, in rules) > RankNormal;

    public static int Choose(BattleSnapshot snapshot, string input, in TargetRules rules)
    {
        bool shortestFirst = rules.Order == TargetOrder.ShortestFirst || (rules.Order == TargetOrder.Smart && snapshot.PhraseCount >= rules.CrowdedAt);
        int bestIndex = -1;
        for (int i = 0; i < snapshot.PhraseCount; i++)
        {
            if (IsCandidate(snapshot, i, rules.LinkedOnly) && (rules.Skipped == null || !snapshot.Phrases[i].Equals(rules.Skipped, StringComparison.OrdinalIgnoreCase)) && (bestIndex == -1 || IsBetter(snapshot, input, in rules, shortestFirst, i, bestIndex)))
            {
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    private static bool IsCandidate(BattleSnapshot snapshot, int index, bool linkedOnly)
    {
        return !snapshot.PhraseDefeated[index] && (!linkedOnly || snapshot.IsLinked(index));
    }

    private static bool IsBetter(BattleSnapshot snapshot, string input, in TargetRules rules, bool shortestFirst, int index, int bestIndex)
    {
        int rank = Rank(snapshot, index, in rules);
        int bestRank = Rank(snapshot, bestIndex, in rules);
        if (rank != bestRank)
        {
            return rank > bestRank;
        }
        bool continues = Continues(snapshot, input, index);
        if (continues != Continues(snapshot, input, bestIndex))
        {
            return continues;
        }
        bool casting = rank is RankCasting or RankInterrupt;
        if (casting)
        {
            float progress = snapshot.CastProgressOf(index);
            float bestProgress = snapshot.CastProgressOf(bestIndex);
            if (progress != bestProgress)
            {
                return progress > bestProgress;
            }
        }
        int candidateLength = snapshot.PhraseTyped[index].Length;
        int bestLength = snapshot.PhraseTyped[bestIndex].Length;
        if (shortestFirst && candidateLength != bestLength)
        {
            return candidateLength < bestLength;
        }
        int closeness = Closeness(snapshot, index, bestIndex);
        if (closeness != 0)
        {
            return closeness > 0;
        }
        return candidateLength < bestLength;
    }

    private static int Closeness(BattleSnapshot snapshot, int index, int bestIndex)
    {
        if (snapshot.HasDistance(index) && snapshot.HasDistance(bestIndex))
        {
            return snapshot.PhraseDistance[bestIndex].CompareTo(snapshot.PhraseDistance[index]);
        }
        return snapshot.PhraseScreenY[index].CompareTo(snapshot.PhraseScreenY[bestIndex]);
    }

    private static bool Continues(BattleSnapshot snapshot, string input, int index)
    {
        return input.Length > 0 && snapshot.PhraseTyped[index].StartsWith(input, StringComparison.OrdinalIgnoreCase);
    }
}
