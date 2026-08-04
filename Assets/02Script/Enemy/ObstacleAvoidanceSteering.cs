using UnityEngine;

namespace DDARoguelike
{
    public static class ObstacleAvoidanceSteering
    {
        private const string ObstacleLayerName = "Obstacle";
        private const int HitBufferSize = 8;

        private static readonly RaycastHit2D[] HitBuffer = new RaycastHit2D[HitBufferSize];
        private static readonly float[] CandidateAnglesDegrees =
        {
            0.0f,
            45.0f,
            -45.0f,
            90.0f,
            -90.0f,
            135.0f,
            -135.0f,
        };

        private static bool isLayerMaskInitialized;
        private static int obstacleLayerMask;

        public static Vector2 Resolve(
            Vector2 origin,
            Vector2 desiredDirection,
            Vector2 targetPosition,
            float castRadius,
            float castDistance,
            Vector2 previousDirection,
            float stickBias)
        {
            if (desiredDirection.sqrMagnitude <= 0.0001f)
            {
                return Vector2.zero;
            }

            EnsureLayerMask();

            Vector2 desired = desiredDirection.normalized;
            float safeRadius = Mathf.Max(0.05f, castRadius);
            float safeDistance = Mathf.Max(safeRadius, castDistance);
            Vector2 stickDirection = previousDirection.sqrMagnitude > 0.0001f
                ? previousDirection.normalized
                : desired;
            float safeStickBias = Mathf.Max(0.0f, stickBias);

            if (!IsBlocked(origin, desired, safeRadius, safeDistance))
            {
                return desired;
            }

            Vector2 toTarget = targetPosition - origin;
            Vector2 toTargetDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : desired;

            Vector2 bestClearDirection = desired;
            float bestClearScore = float.NegativeInfinity;
            bool foundClear = false;

            Vector2 bestBlockedDirection = desired;
            float bestBlockedScore = float.NegativeInfinity;

            for (int i = 0; i < CandidateAnglesDegrees.Length; i++)
            {
                Vector2 candidate = Rotate(desired, CandidateAnglesDegrees[i]);
                float clearance;
                float stickScore = Vector2.Dot(candidate, stickDirection) * safeStickBias;

                if (!TryGetClearance(origin, candidate, safeRadius, safeDistance, out clearance))
                {
                    float score = Vector2.Dot(candidate, toTargetDirection) + stickScore;

                    if (!foundClear || score > bestClearScore)
                    {
                        foundClear = true;
                        bestClearScore = score;
                        bestClearDirection = candidate;
                    }

                    continue;
                }

                float blockedScore = clearance + stickScore;

                if (blockedScore > bestBlockedScore)
                {
                    bestBlockedScore = blockedScore;
                    bestBlockedDirection = candidate;
                }
            }

            if (foundClear)
            {
                return bestClearDirection;
            }

            return bestBlockedDirection;
        }

        public static Vector2 SmoothDirection(
            Vector2 currentDirection,
            Vector2 targetDirection,
            float maxRadiansPerSecond,
            float deltaTime)
        {
            if (targetDirection.sqrMagnitude <= 0.0001f)
            {
                return Vector2.zero;
            }

            Vector2 target = targetDirection.normalized;

            if (currentDirection.sqrMagnitude <= 0.0001f)
            {
                return target;
            }

            Vector2 current = currentDirection.normalized;
            float maxDelta = Mathf.Max(0.0f, maxRadiansPerSecond) * Mathf.Max(0.0f, deltaTime);

            if (maxDelta <= 0.0f)
            {
                return target;
            }

            float angle = Vector2.SignedAngle(current, target);
            float step = Mathf.Clamp(angle, -maxDelta * Mathf.Rad2Deg, maxDelta * Mathf.Rad2Deg);
            return Rotate(current, step);
        }

        private static void EnsureLayerMask()
        {
            if (isLayerMaskInitialized)
            {
                return;
            }

            isLayerMaskInitialized = true;
            int layer = LayerMask.NameToLayer(ObstacleLayerName);

            if (layer < 0)
            {
                Debug.LogError($"[{nameof(ObstacleAvoidanceSteering)}] Layer '{ObstacleLayerName}' was not found.");
                obstacleLayerMask = 0;
                return;
            }

            obstacleLayerMask = 1 << layer;
        }

        private static bool IsBlocked(Vector2 origin, Vector2 direction, float radius, float distance)
        {
            float clearance;
            return TryGetClearance(origin, direction, radius, distance, out clearance);
        }

        private static bool TryGetClearance(
            Vector2 origin,
            Vector2 direction,
            float radius,
            float distance,
            out float clearance)
        {
            clearance = distance;

            if (obstacleLayerMask == 0)
            {
                return false;
            }

            int hitCount = Physics2D.CircleCastNonAlloc(
                origin,
                radius,
                direction,
                HitBuffer,
                distance,
                obstacleLayerMask);

            if (hitCount <= 0)
            {
                return false;
            }

            float nearest = distance;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = HitBuffer[i];

                if (hit.collider == null)
                {
                    continue;
                }

                if (hit.distance < nearest)
                {
                    nearest = hit.distance;
                }
            }

            clearance = nearest;
            return nearest < distance;
        }

        private static Vector2 Rotate(Vector2 direction, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos);
        }
    }
}
