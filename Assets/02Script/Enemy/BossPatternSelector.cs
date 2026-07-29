using UnityEngine;

namespace DDARoguelike
{
    public sealed class BossPatternSelector
    {
        public BossPattern Select(BossPattern[] patterns, BossContext context, BossPattern previousPattern)
        {
            if (patterns == null || patterns.Length == 0)
            {
                return null;
            }

            bool hasAlternative = HasExecutableAlternative(patterns, context, previousPattern);
            float totalWeight = 0.0f;
            BossPattern fallbackPattern = null;

            for (int i = 0; i < patterns.Length; i++)
            {
                BossPattern pattern = patterns[i];

                if (!IsCandidate(pattern, context, previousPattern, hasAlternative))
                {
                    continue;
                }

                fallbackPattern = pattern;
                totalWeight += pattern.Weight;
            }

            if (fallbackPattern == null)
            {
                return null;
            }

            if (totalWeight <= 0.0f)
            {
                return fallbackPattern;
            }

            float randomValue = Random.value * totalWeight;

            for (int i = 0; i < patterns.Length; i++)
            {
                BossPattern pattern = patterns[i];

                if (!IsCandidate(pattern, context, previousPattern, hasAlternative))
                {
                    continue;
                }

                randomValue -= pattern.Weight;

                if (randomValue <= 0.0f)
                {
                    return pattern;
                }
            }

            return fallbackPattern;
        }

        private static bool HasExecutableAlternative(
            BossPattern[] patterns,
            BossContext context,
            BossPattern previousPattern)
        {
            for (int i = 0; i < patterns.Length; i++)
            {
                BossPattern pattern = patterns[i];

                if (pattern != null && pattern != previousPattern && pattern.CanExecute(context))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCandidate(
            BossPattern pattern,
            BossContext context,
            BossPattern previousPattern,
            bool hasAlternative)
        {
            if (pattern == null || !pattern.CanExecute(context))
            {
                return false;
            }

            return !hasAlternative || pattern != previousPattern;
        }
    }
}
