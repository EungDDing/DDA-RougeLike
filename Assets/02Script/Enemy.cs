using UnityEngine;

namespace DDARoguelike
{
    public enum AI_State
    {
        Idle,
        Roaming,
        Return,
        Chase,
        Attack,
        Die,
    }

    public abstract class Enemy : MonoBehaviour
    {
        [SerializeField] protected float maxHp;
        [SerializeField] protected float attackPower;

        private float currentHp;
        protected AI_State currentState;

        public float MaxHp => maxHp;
        public float CurrentHp => currentHp;
        public float AttackPower => attackPower;
        public AI_State CurrentState => currentState;

        protected virtual void Awake()
        {
            currentHp = maxHp;
        }

        private void Update()
        {
            TickAI();
        }

        private void TickAI()
        {
            switch (currentState)
            {
                case AI_State.Idle:
                    OnIdle();
                    break;
                case AI_State.Roaming:
                    OnRoaming();
                    break;
                case AI_State.Return:
                    OnReturn();
                    break;
                case AI_State.Chase:
                    OnChase();
                    break;
                case AI_State.Attack:
                    OnAttack();
                    break;
                case AI_State.Die:
                    OnDie();
                    break;
            }
        }

        protected void SetState(AI_State newState)
        {
            if (currentState == newState)
            {
                return;
            }

            currentState = newState;
        }

        protected virtual void OnIdle()
        {
        }

        protected virtual void OnRoaming()
        {
        }

        protected virtual void OnReturn()
        {
        }

        protected virtual void OnChase()
        {
        }

        protected virtual void OnAttack()
        {
        }

        protected virtual void OnDie()
        {
        }
    }
}
