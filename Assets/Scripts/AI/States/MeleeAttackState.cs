using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Melee attack state for AI agents. The AI performs a close-range attack.
    /// Transitions back to Pursue after attack or to other states based on conditions.
    /// </summary>
    public class MeleeAttackState : AIState
    {
        private float attackTimer;
        private bool hasAttacked;
        private const float ATTACK_DURATION = 1f;
        private const float COOLDOWN_TIME = 0.5f;

        public MeleeAttackState(AIController controller) : base(controller, nameof(MeleeAttackState))
        {
        }

        public override void Enter()
        {
            attackTimer = 0f;
            hasAttacked = false;
        }

        public override void Tick(float dt)
        {
            attackTimer += dt;

            // Perform attack at the right moment
            if (!hasAttacked && attackTimer >= 0.2f)
            {
                controller.PerformMeleeAttack();
                hasAttacked = true;
            }

            // Return to pursue state after attack completes
            if (attackTimer >= ATTACK_DURATION)
            {
                // Check if target is still valid and in range
                if (controller.Blackboard.targetId != 0)
                {
                    controller.ChangeState(nameof(PursueState));
                }
                else
                {
                    controller.ChangeState(nameof(IdleState));
                }
            }
        }

        public override void Exit()
        {
            // No cleanup needed
        }
    }
}