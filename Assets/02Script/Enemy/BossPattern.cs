using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    public abstract class BossPattern : ScriptableObject, IBossPattern
    {
        [Header("Selection")]
        [SerializeField] private string patternName;
        [SerializeField] private float weight = 1.0f;
        [SerializeField] private float minimumActivationDistance;
        [SerializeField] private float maximumActivationDistance = 10.0f;

        [Header("Timing")]
        [SerializeField] private float telegraphDuration = 0.5f;
        [SerializeField] private float recoveryDuration = 0.6f;
        [SerializeField] private Color telegraphColor = Color.red;

        public string PatternName => string.IsNullOrWhiteSpace(patternName) ? name : patternName;
        public float Weight => Mathf.Max(0.0f, weight);
        public float RecoveryDuration => Mathf.Max(0.0f, recoveryDuration);
        protected float TelegraphDuration => Mathf.Max(0.0f, telegraphDuration);
        protected Color TelegraphColor => telegraphColor;

        public virtual bool CanExecute(BossContext context)
        {
            if (context == null || !context.IsValid)
            {
                return false;
            }

            float distance = context.DistanceToPlayer;
            float minimumDistance = Mathf.Max(0.0f, minimumActivationDistance);
            float maximumDistance = Mathf.Max(minimumDistance, maximumActivationDistance);
            return distance >= minimumDistance && distance <= maximumDistance;
        }

        public abstract IEnumerator Execute(BossContext context);

        protected IEnumerator PlayTelegraph(BossContext context)
        {
            return context.Boss.PlayTelegraph(TelegraphDuration, TelegraphColor);
        }
    }
}
