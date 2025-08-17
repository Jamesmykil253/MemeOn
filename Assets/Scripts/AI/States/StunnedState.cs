using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Stunned state for AI agents. The AI is temporarily incapacitated.
    /// Automatically transitions to previous state after stun duration expires.
    /// </summary>
    public class StunnedState : AIState
    {
        private float stunTimer;
        private const float STUN_DURATION = 2f;

        public StunnedState(AIController controller) : base(controller, nameof(StunnedState))
        {
        }

        public override void Enter()
        {
            stunTimer = 0f;
        }

        public override void Tick(float dt)
        {
            stunTimer += dt;

            // AI cannot move or act while stunned
            // Just wait for the stun to expire

            if (stunTimer >= STUN_DURATION)
            {
                // Return to appropriate state after stun
                if (controller.Blackboard.aggroed && controller.Blackboard.targetId != 0)
                {
                    controller.ChangeState(nameof(AlertState));
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