using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Return to spawn state for AI agents. The AI moves back to its spawn position.
    /// Transitions to Idle when reaching the spawn point.
    /// </summary>
    public class ReturnToSpawnState : AIState
    {
        private const float ARRIVAL_THRESHOLD = 1.5f;

        public ReturnToSpawnState(AIController controller) : base(controller, nameof(ReturnToSpawnState))
        {
        }

        public override void Enter()
        {
            // Clear target and aggro when returning to spawn
            controller.Blackboard.aggroed = false;
            controller.Blackboard.targetId = 0;
        }

        public override void Tick(float dt)
        {
            Vector3 toSpawn = controller.Blackboard.spawnPosition - controller.transform.position;
            float distanceToSpawn = toSpawn.magnitude;

            // Check if we've reached the spawn position
            if (distanceToSpawn <= ARRIVAL_THRESHOLD)
            {
                controller.ChangeState(nameof(IdleState));
                return;
            }

            // Move toward spawn position
            Vector3 direction = toSpawn.normalized;
            controller.Move(direction, dt);
            controller.FaceTowards(controller.Blackboard.spawnPosition, dt);
        }

        public override void Exit()
        {
            // Reset position to exact spawn point when exiting
            controller.transform.position = controller.Blackboard.spawnPosition;
        }
    }
}