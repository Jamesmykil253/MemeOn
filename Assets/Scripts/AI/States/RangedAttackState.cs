using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Ranged attack state for AI agents. The AI performs a ranged attack with projectiles.
    /// Transitions back to Pursue after attack or to other states based on conditions.
    /// </summary>
    public class RangedAttackState : AIState
    {
        private float attackTimer;
        private bool hasAttacked;
        private const float ATTACK_DURATION = 1.5f;

        public RangedAttackState(AIController controller) : base(controller, nameof(RangedAttackState))
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

            // Face target during attack preparation
            if (controller.Blackboard.targetId != 0)
            {
                controller.FaceTowards(controller.Blackboard.lastKnownTargetPos, dt);
            }

            // Perform attack at the right moment
            if (!hasAttacked && attackTimer >= 0.3f)
            {
                controller.PerformRangedAttack();
                hasAttacked = true;
            }

            // Return to pursue state after attack completes
            if (attackTimer >= ATTACK_DURATION)
            {
                // Check if target is still valid
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