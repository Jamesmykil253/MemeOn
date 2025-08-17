using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Idle state for AI agents. The AI remains stationary and scans for potential targets.
    /// Transitions to Alert state when a target is detected or damage is received.
    /// </summary>
    public class IdleState : AIState
    {
        public IdleState(AIController controller) : base(controller, nameof(IdleState))
        {
        }

        public override void Enter()
        {
            // Reset aggro state when entering idle
            controller.Blackboard.aggroed = false;
            controller.Blackboard.targetId = 0;
        }

        public override void Tick(float dt)
        {
            // In idle state, AI remains stationary but can be triggered by damage
            // The AIController handles damage events and will transition to Alert if needed
        }

        public override void Exit()
        {
            // No cleanup needed for idle state
        }
    }
}