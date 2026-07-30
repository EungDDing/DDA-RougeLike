using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DDARoguelike
{
    public abstract class PlayerSkill : MonoBehaviour
    {
        [SerializeField] private float cooldownSeconds = 3.0f;

        private float nextReadyTime;
        private Vector2 lastAimDirection = Vector2.right;
        private bool wasOnCooldown;

        public float CooldownDuration => Mathf.Max(0.0f, cooldownSeconds);

        public float RemainingCooldown
        {
            get
            {
                float remaining = nextReadyTime - Time.time;
                return remaining > 0.0f ? remaining : 0.0f;
            }
        }

        public bool IsOnCooldown => RemainingCooldown > 0.0f;

        public event Action CooldownStarted;
        public event Action CooldownReady;

        protected Vector2 LastAimDirection => lastAimDirection;

        protected virtual void Update()
        {
            UpdateLastAimDirection();
            UpdateCooldownReadyEvent();

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (!keyboard.spaceKey.wasPressedThisFrame)
            {
                return;
            }

            TryActivate();
        }

        protected bool TryActivate()
        {
            if (IsOnCooldown)
            {
                return false;
            }

            if (!ActivateSkill())
            {
                return false;
            }

            float cooldown = CooldownDuration;
            nextReadyTime = Time.time + cooldown;
            wasOnCooldown = cooldown > 0.0f;

            if (CooldownStarted != null)
            {
                CooldownStarted.Invoke();
            }

            if (cooldown <= 0.0f)
            {
                wasOnCooldown = false;

                if (CooldownReady != null)
                {
                    CooldownReady.Invoke();
                }
            }

            return true;
        }

        protected abstract bool ActivateSkill();

        private void UpdateCooldownReadyEvent()
        {
            bool onCooldown = IsOnCooldown;

            if (wasOnCooldown && !onCooldown)
            {
                wasOnCooldown = false;

                if (CooldownReady != null)
                {
                    CooldownReady.Invoke();
                }
            }
            else if (onCooldown)
            {
                wasOnCooldown = true;
            }
        }

        private void UpdateLastAimDirection()
        {
            Vector2 aimDirection = ReadAimDirection();

            if (aimDirection.sqrMagnitude <= 0.0f)
            {
                return;
            }

            lastAimDirection = aimDirection.normalized;
        }

        private static Vector2 ReadAimDirection()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float horizontal = 0.0f;
            float vertical = 0.0f;

            if (keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1.0f;
            }

            if (keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1.0f;
            }

            if (keyboard.downArrowKey.isPressed)
            {
                vertical -= 1.0f;
            }

            if (keyboard.upArrowKey.isPressed)
            {
                vertical += 1.0f;
            }

            return new Vector2(horizontal, vertical);
        }
    }
}
