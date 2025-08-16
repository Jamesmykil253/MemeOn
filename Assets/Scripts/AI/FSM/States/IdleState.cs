using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// The Idle state represents the default behaviour when the AI is not engaged.
    /// It does not perform any movement or attacks. Damage events will trigger a
    /// transition into the Alert state via AIController.
    /// </summary>
    public class IdleState : AIState
    {
        public IdleState(AIController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            // Reset counters and ensure the AI returns to its spawn position when idling.
            blackboard.ResetBlackboard();
            // Align with spawn rotation if necessary.
            controller.StopMovement();
        }

        public override void Tick(float deltaTime)
        {
            // Idle behaviour does nothing unless external events occur.
        }
    }
}