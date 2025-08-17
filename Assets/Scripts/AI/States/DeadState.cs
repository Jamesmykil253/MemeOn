using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Dead state for AI agents. The AI is inactive and waiting for respawn.
    /// Handles respawn logic and transitions back to Idle after respawn delay.
    /// </summary>
    public class DeadState : AIState
    {
        private float respawnTimer;

        public DeadState(AIController controller) : base(controller, nameof(DeadState))
        {
        }

        public override void Enter()
        {
            respawnTimer = 0f;
            
            // Clear all combat state
            controller.Blackboard.aggroed = false;
            controller.Blackboard.targetId = 0;
            controller.Blackboard.timeSinceLastSuccessfulHit = 0f;
        }

        public override void Tick(float dt)
        {
            respawnTimer += dt;

            // Check if respawn delay has elapsed
            float respawnDelay = ProjectConstants.Match.RespawnDelay;
            if (respawnTimer >= respawnDelay)
            {
                // Respawn the AI
                RespawnAI();
            }
        }

        public override void Exit()
        {
            // No cleanup needed - respawn handles restoration
        }

        private void RespawnAI()
        {
            // Reset position to spawn
            controller.transform.position = controller.Blackboard.spawnPosition;
            
            // Reset health (if the health component supports it)
            var health = controller.GetComponent<MemeArena.Combat.NetworkHealth>();
            if (health != null)
            {
                // Note: This would typically require a server RPC to properly restore health
                // For now, we'll just transition states and let the health system handle respawn
            }
            
            // Transition back to idle state
            controller.ChangeState(nameof(IdleState));
        }
    }
}