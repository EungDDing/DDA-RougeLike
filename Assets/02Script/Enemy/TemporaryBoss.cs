using UnityEngine;

namespace DDARoguelike
{
    public sealed class TemporaryBoss : BossController
    {
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private const float MovementThreshold = 0.0001f;

        [SerializeField] private Animator animator;

        protected override void Awake()
        {
            base.Awake();

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator == null)
            {
                Debug.LogError(
                    $"[{nameof(TemporaryBoss)}] Animator is required on {gameObject.name}.",
                    this);
            }
        }

        private void LateUpdate()
        {
            if (animator == null)
            {
                return;
            }

            bool isMoving = rigidbody2D != null
                && rigidbody2D.linearVelocity.sqrMagnitude > MovementThreshold
                && currentState != AI_State.Die;
            animator.SetBool(IsMovingHash, isMoving);
        }
    }
}
