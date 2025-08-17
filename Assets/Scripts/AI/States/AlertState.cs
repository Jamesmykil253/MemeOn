using UnityEngine;
using Unity.Netcode;

namespace MemeArena.AI
{
    /// <summary>
    /// Alert state for AI agents. The AI has detected a threat and is preparing to engage.
    /// Transitions to Pursue state when a target is confirmed.
    /// </summary>
    public class AlertState : AIState
    {
        private float alertTimer;
        private const float ALERT_DURATION = 2f;

        public AlertState(AIController controller) : base(controller, nameof(AlertState))
        {
        }

        public override void Enter()
        {
            alertTimer = 0f;
            controller.Blackboard.aggroed = true;
        }

        public override void Tick(float dt)
        {
            alertTimer += dt;

            // Check if we have a valid target to pursue
            if (controller.Blackboard.targetId != 0)
            {
                var targetExists = NetworkManager.Singleton != null &&
                                 NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(controller.Blackboard.targetId);
                
                if (targetExists)
                {
                    controller.ChangeState(nameof(PursueState));
                    return;
                }
            }

            // Return to idle if alert period expires without finding a target
            if (alertTimer >= ALERT_DURATION)
            {
                controller.ChangeState(nameof(IdleState));
            }
        }

        public override void Exit()
        {
            // No cleanup needed
        }
    }
}