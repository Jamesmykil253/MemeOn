using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// PursueState drives the AI towards its current target. If the target comes within
    /// melee or ranged range, transitions to the appropriate attack state. If the AI
    /// strays too far from its spawn or fails to land hits for long enough, it
    /// transitions to ReturnToSpawn.
    /// </summary>
    public class PursueState : AIState
    {
        public PursueState(AIController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Tick(float deltaTime)
        {
            // If there is no target or the target died, return home.
            if (!controller.IsTargetAlive())
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }

            // Update timers on blackboard.
            blackboard.timeSinceLastSuccessfulHit += deltaTime;

            // Grab target and distance.
            var targetObj = controller.FindTargetNetworkObject();
            if (targetObj == null)
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }
            Vector3 targetPos = targetObj.transform.position;
            blackboard.lastKnownTargetPos = targetPos;

            float distanceToTarget = Vector3.Distance(controller.transform.position, targetPos);

            // If too far from spawn or gave up chasing, return to spawn.
            float distanceFromSpawn = Vector3.Distance(controller.transform.position, blackboard.spawnPosition);
            if (distanceFromSpawn > controller.Config.maxPursueRadius ||
                blackboard.timeSinceLastSuccessfulHit >= controller.Config.giveUpTimeout)
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }

            // Decide on attack state.
            if (distanceToTarget <= controller.Config.meleeRange)
            {
                controller.ChangeState(nameof(MeleeAttackState));
                return;
            }
            else if (distanceToTarget <= controller.Config.rangedRange)
            {
                controller.ChangeState(nameof(RangedAttackState));
                return;
            }

            // Otherwise continue pursuing.
            float speed = blackboard.stats != null ? blackboard.stats.moveSpeed : 2f;
            controller.MoveTowards(targetPos, speed);
        }
    }
}