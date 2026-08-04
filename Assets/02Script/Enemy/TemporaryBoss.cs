using UnityEngine;

namespace DDARoguelike
{
    public sealed class TemporaryBoss : BossController
    {
        private const float MovementThreshold = 0.0001f;

        [SerializeField] private SpriteRenderer animatedSpriteRenderer;
        [SerializeField] private Sprite[] walkingFrames;
        [SerializeField, Min(1.0f)] private float walkingFramesPerSecond = 9.0f;

        private int currentWalkingFrame;
        private float walkingFrameTimer;

        protected override void Awake()
        {
            base.Awake();

            if (animatedSpriteRenderer == null)
            {
                animatedSpriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (animatedSpriteRenderer == null)
            {
                Debug.LogError(
                    $"[{nameof(TemporaryBoss)}] SpriteRenderer is required on {gameObject.name}.",
                    this);
            }

            if (walkingFrames == null || walkingFrames.Length == 0)
            {
                Debug.LogError(
                    $"[{nameof(TemporaryBoss)}] Walking frames are not assigned on {gameObject.name}.",
                    this);
                return;
            }

            ShowFirstWalkingFrame();
        }

        private void LateUpdate()
        {
            if (animatedSpriteRenderer == null || walkingFrames == null || walkingFrames.Length == 0)
            {
                return;
            }

            bool isMoving = rigidbody2D != null
                && rigidbody2D.linearVelocity.sqrMagnitude > MovementThreshold
                && currentState != AI_State.Die;

            if (!isMoving)
            {
                ShowFirstWalkingFrame();
                return;
            }

            walkingFrameTimer += Time.deltaTime;
            float frameDuration = 1.0f / walkingFramesPerSecond;

            while (walkingFrameTimer >= frameDuration)
            {
                walkingFrameTimer -= frameDuration;
                currentWalkingFrame = (currentWalkingFrame + 1) % walkingFrames.Length;
                animatedSpriteRenderer.sprite = walkingFrames[currentWalkingFrame];
            }
        }

        private void ShowFirstWalkingFrame()
        {
            currentWalkingFrame = 0;
            walkingFrameTimer = 0.0f;

            if (walkingFrames[0] != null)
            {
                animatedSpriteRenderer.sprite = walkingFrames[0];
            }
        }
    }
}
