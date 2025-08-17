using UnityEngine;
using Unity.Netcode;

namespace MemeArena.AI
{
    /// <summary>
    /// Pursue state for AI agents. The AI actively moves toward and engages a target.
    /// Transitions to attack states when in range or to ReturnToSpawn when giving up.
    /// </summary>
    public class PursueState : AIState
    {
        private const float GIVE_UP_TIMEOUT = 10f;
        private const float MELEE_RANGE = 2f;
        private const float RANGED_RANGE = 8f;

        public PursueState(AIController controller) : base(controller, nameof(PursueState))
        {
        }

        public override void Enter()
        {
            // Reset pursuit timer
            controller.Blackboard.timeSinceLastSuccessfulHit = 0f;
        }

        public override void Tick(float dt)
        {
            // Check if we should give up pursuit
            if (controller.Blackboard.timeSinceLastSuccessfulHit >= GIVE_UP_TIMEOUT)
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }

            // Get current target
            if (controller.Blackboard.targetId == 0)
            {
                controller.ChangeState(nameof(IdleState));
                return;
            }

            // Check if target still exists
            if (NetworkManager.Singleton?.SpawnManager?.SpawnedObjects.ContainsKey(controller.Blackboard.targetId) != true)
            {
                controller.Blackboard.targetId = 0;
                controller.ChangeState(nameof(IdleState));
                return;
            }

            var targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[controller.Blackboard.targetId];
            if (targetObject == null)
            {
                controller.Blackboard.targetId = 0;
                controller.ChangeState(nameof(IdleState));
                return;
            }

            Vector3 targetPos = targetObject.transform.position;
            float distanceToTarget = Vector3.Distance(controller.transform.position, targetPos);

            // Update last known position
            controller.Blackboard.lastKnownTargetPos = targetPos;

            // Move toward target
            Vector3 direction = (targetPos - controller.transform.position).normalized;
            controller.Move(direction, dt);
            controller.FaceTowards(targetPos, dt);

            // Check for attack opportunities
            if (distanceToTarget <= MELEE_RANGE)
            {
                controller.ChangeState(nameof(MeleeAttackState));
            }
            else if (distanceToTarget <= RANGED_RANGE && controller.projectilePrefab != null)
            {
                controller.ChangeState(nameof(RangedAttackState));
            }
        }

        public override void Exit()
        {
            // No cleanup needed
        }
    }
}