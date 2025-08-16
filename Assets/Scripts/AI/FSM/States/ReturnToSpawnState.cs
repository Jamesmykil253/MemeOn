using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Moves the AI back to its recorded spawn position. Once the AI arrives within a
    /// small threshold it transitions to Idle. This state clears aggro and resets
    /// blackboard values when entering.
    /// </summary>
    public class ReturnToSpawnState : AIState
    {
        public ReturnToSpawnState(AIController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            // Disengage when returning.
            blackboard.aggroed = false;
            blackboard.targetId = 0;
            blackboard.timeSinceLastSuccessfulHit = 0f;
            blackboard.failedHitCounter = 0;
        }

        public override void Tick(float deltaTime)
        {
            Vector3 home = blackboard.spawnPosition;
            float distance = Vector3.Distance(controller.transform.position, home);
            if (distance <= 0.1f)
            {
                controller.ChangeState(nameof(IdleState));
                return;
            }
            float speed = blackboard.stats != null ? blackboard.stats.moveSpeed : 2f;
            controller.MoveTowards(home, speed);
        }
    }
}