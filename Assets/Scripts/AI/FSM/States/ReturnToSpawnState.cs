using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Guides the AI back to its original spawn position.  Once the AI
    /// reaches within a small threshold it transitions back to IdleState.
    /// While returning home the AI does not engage with targets.
    /// </summary>
    public class ReturnToSpawnState : AIState
    {
        public ReturnToSpawnState(AIController controller) : base(controller, nameof(ReturnToSpawnState)) { }

        public override void Enter()
        {
            // Clear aggro so that stray damage doesn't cause immediate aggro.
            controller.Blackboard.aggroed = false;
            controller.Blackboard.targetId = 0;
            controller.Blackboard.timeSinceLastSuccessfulHit = 0f;
        }

        public override void Tick(float dt)
        {
            Vector3 spawn = controller.Blackboard.spawnPosition;
            Vector3 toSpawn = spawn - controller.transform.position;
            float distance = toSpawn.magnitude;
            if (distance < 0.1f)
            {
                // Snap to exact spawn to avoid drift.
                controller.transform.position = spawn;
                controller.ChangeState(nameof(IdleState));
                return;
            }
            // Move toward spawn and rotate to face it.
            controller.FaceTowards(spawn, dt);
            controller.Move(toSpawn, dt);
        }
    }
}