using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Evade state for AI agents. The AI attempts to move away from threats.
    /// Transitions to other states based on threat assessment.
    /// </summary>
    public class EvadeState : AIState
    {
        private float evadeTimer;
        private Vector3 evadeDirection;
        private const float EVADE_DURATION = 2f;

        public EvadeState(AIController controller) : base(controller, nameof(EvadeState))
        {
        }

        public override void Enter()
        {
            evadeTimer = 0f;
            
            // Calculate evade direction (away from last known target position)
            if (controller.Blackboard.targetId != 0)
            {
                Vector3 toTarget = controller.Blackboard.lastKnownTargetPos - controller.transform.position;
                evadeDirection = -toTarget.normalized;
            }
            else
            {
                // Random evade direction if no specific threat
                evadeDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            }
        }

        public override void Tick(float dt)
        {
            evadeTimer += dt;

            // Move in evade direction
            controller.Move(evadeDirection, dt);

            // After evading for the duration, decide next action
            if (evadeTimer >= EVADE_DURATION)
            {
                // If still have a target, return to pursuit
                if (controller.Blackboard.targetId != 0 && controller.Blackboard.aggroed)
                {
                    controller.ChangeState(nameof(PursueState));
                }
                else
                {
                    controller.ChangeState(nameof(ReturnToSpawnState));
                }
            }
        }

        public override void Exit()
        {
            // No cleanup needed
        }
    }
}