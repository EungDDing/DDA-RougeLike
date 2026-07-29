using UnityEngine;

namespace DDARoguelike
{
    public sealed class BossContext
    {
        public BossContext(BossController boss, Transform playerTransform)
        {
            Boss = boss;
            PlayerTransform = playerTransform;
        }

        public BossController Boss { get; }
        public Transform PlayerTransform { get; }
        public bool IsValid => Boss != null && PlayerTransform != null;

        public float DistanceToPlayer
        {
            get
            {
                if (!IsValid)
                {
                    return float.PositiveInfinity;
                }

                return Vector2.Distance(Boss.Position, PlayerTransform.position);
            }
        }

        public Vector2 DirectionToPlayer
        {
            get
            {
                if (!IsValid)
                {
                    return Vector2.zero;
                }

                Vector2 direction = (Vector2)PlayerTransform.position - Boss.Position;
                return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
            }
        }
    }
}
