using UnityEngine;

namespace DDARoguelike
{
    public sealed class BossAttackIndicator : MonoBehaviour
    {
        private const int CircleSegmentCount = 48;
        private const float ArcDegreesPerSegment = 10.0f;

        private Transform followTarget;
        private LineRenderer lineRenderer;
        private Material runtimeMaterial;
        private Vector3[] pointOffsets;

        public static BossAttackIndicator CreateArc(
            Transform target,
            Vector2 direction,
            float radius,
            float arcAngle,
            Color color,
            float lineWidth)
        {
            float clampedArc = Mathf.Clamp(arcAngle, 0.0f, 360.0f);
            int arcSegmentCount = Mathf.Max(2, Mathf.CeilToInt(clampedArc / ArcDegreesPerSegment));
            Vector3[] offsets = new Vector3[arcSegmentCount + 3];
            float centerAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float startAngle = centerAngle - clampedArc * 0.5f;

            offsets[0] = Vector3.zero;

            for (int i = 0; i <= arcSegmentCount; i++)
            {
                float ratio = (float)i / arcSegmentCount;
                float angle = (startAngle + clampedArc * ratio) * Mathf.Deg2Rad;
                offsets[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f)
                    * Mathf.Max(0.0f, radius);
            }

            offsets[offsets.Length - 1] = Vector3.zero;
            return Create(target, offsets, color, lineWidth, "BossArcIndicator");
        }

        public static BossAttackIndicator CreateCircle(
            Transform target,
            float radius,
            Color color,
            float lineWidth)
        {
            Vector3[] offsets = new Vector3[CircleSegmentCount + 1];

            for (int i = 0; i <= CircleSegmentCount; i++)
            {
                float angle = (float)i / CircleSegmentCount * Mathf.PI * 2.0f;
                offsets[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f)
                    * Mathf.Max(0.0f, radius);
            }

            return Create(target, offsets, color, lineWidth, "BossCircleIndicator");
        }

        public static BossAttackIndicator CreateLine(
            Transform target,
            Vector2 direction,
            float length,
            Color color,
            float lineWidth)
        {
            Vector2 normalizedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector2.right;
            Vector3[] offsets =
            {
                Vector3.zero,
                normalizedDirection * Mathf.Max(0.0f, length),
            };

            return Create(target, offsets, color, lineWidth, "BossLineIndicator");
        }

        private static BossAttackIndicator Create(
            Transform target,
            Vector3[] offsets,
            Color color,
            float lineWidth,
            string objectName)
        {
            if (target == null)
            {
                return null;
            }

            GameObject indicatorObject = new GameObject(objectName);
            indicatorObject.layer = target.gameObject.layer;
            BossAttackIndicator indicator = indicatorObject.AddComponent<BossAttackIndicator>();
            indicator.Initialize(target, offsets, color, lineWidth);
            return indicator;
        }

        private void Initialize(
            Transform target,
            Vector3[] offsets,
            Color color,
            float lineWidth)
        {
            followTarget = target;
            pointOffsets = offsets;
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            lineRenderer.positionCount = pointOffsets.Length;
            lineRenderer.startWidth = Mathf.Max(0.01f, lineWidth);
            lineRenderer.endWidth = Mathf.Max(0.01f, lineWidth);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.numCapVertices = 2;
            lineRenderer.numCornerVertices = 2;

            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            if (shader != null)
            {
                runtimeMaterial = new Material(shader);
                runtimeMaterial.color = color;
                lineRenderer.material = runtimeMaterial;
            }

            SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();

            if (targetRenderer != null)
            {
                lineRenderer.sortingLayerID = targetRenderer.sortingLayerID;
                lineRenderer.sortingOrder = targetRenderer.sortingOrder + 1;
            }

            UpdatePositions();
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                Destroy(gameObject);
                return;
            }

            UpdatePositions();
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }

        private void UpdatePositions()
        {
            Vector3 targetPosition = followTarget.position;

            for (int i = 0; i < pointOffsets.Length; i++)
            {
                lineRenderer.SetPosition(i, targetPosition + pointOffsets[i]);
            }
        }
    }
}
