using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DDARoguelike
{
    public class PlayerMove : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10.0f;

        private Rigidbody2D rigidbody2D;

        public float MoveSpeed => moveSpeed;

        public event Action StatsChanged;

        public void AddMoveSpeed(float amount)
        {
            moveSpeed += amount;
            Debug.Log($"MoveSpeed: {moveSpeed}");
            NotifyStatsChanged();
        }

        private void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();

            if (rigidbody2D == null)
            {
                Debug.LogError($"[{nameof(PlayerMove)}] Rigidbody2D is required on {gameObject.name}.", this);
            }
        }

        private void NotifyStatsChanged()
        {
            if (StatsChanged != null)
            {
                StatsChanged.Invoke();
            }
        }

        private void FixedUpdate()
        {
            if (rigidbody2D == null)
            {
                return;
            }

            Vector2 moveDirection = ReadMoveInput();

            if (moveDirection.sqrMagnitude > 1.0f)
            {
                moveDirection.Normalize();
            }

            rigidbody2D.linearVelocity = moveDirection * moveSpeed;
        }

        private Vector2 ReadMoveInput()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float horizontal = 0.0f;
            float vertical = 0.0f;

            if (keyboard.aKey.isPressed)
            {
                horizontal -= 1.0f;
            }

            if (keyboard.dKey.isPressed)
            {
                horizontal += 1.0f;
            }

            if (keyboard.sKey.isPressed)
            {
                vertical -= 1.0f;
            }

            if (keyboard.wKey.isPressed)
            {
                vertical += 1.0f;
            }

            return new Vector2(horizontal, vertical);
        }
    }
}
