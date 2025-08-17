using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// IdleState represents the baseline behaviour of the AI.  While idle the
    /// AI does nothing until provoked.  On entry, it resets aggro and clears
    /// target information on the blackboard.  It does not transition on its
    /// own; transitions occur via events such as OnDamageReceived.
    /// </summary>
    public class IdleState : AIState
    {
        public IdleState(AIController controller) : base(controller, nameof(IdleState)) { }

        public override void Enter()
        {
            var bb = controller.Blackboard;
            bb.aggroed = false;
            bb.targetId = 0;
            bb.timeSinceLastSuccessfulHit = 0f;
        }
    }
}