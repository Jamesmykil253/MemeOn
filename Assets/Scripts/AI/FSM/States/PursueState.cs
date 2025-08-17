using UnityEngine;
using Unity.Netcode;

namespace MemeArena.AI
{
    /// <summary>
    /// PursueState handles chasing the current target.  The AI continues to
    /// pursue until the target is lost (dead or out of range) or until the
    /// giveUpTimeout or maxPursueRadius conditions are met.  When within
    /// melee or ranged range the state transitions to MeleeAttackState or
    /// RangedAttackState accordingly.
    /// </summary>
    public class PursueState : AIState
    {
        public PursueState(AIController controller) : base(controller, nameof(PursueState)) { }

        public override void Tick(float dt)
        {
            var bb = controller.Blackboard;
            // If no target or not aggroed, return to spawn.
            if (!bb.aggroed || bb.targetId == 0)
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }
            // Lookup the target network object.
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(bb.targetId))
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }
            var targetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[bb.targetId].gameObject;
            // Check if target has health and is alive.
            var targetHealth = targetObj.GetComponent<Combat.NetworkHealth>();
            if (targetHealth == null || targetHealth.GetCurrentHealth() <= 0)
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }
            // Compute distance.
            Vector3 toTarget = targetObj.transform.position - controller.transform.position;
            float distance = toTarget.magnitude;
            // Update last known position for fallback.
            bb.lastKnownTargetPos = targetObj.transform.position;
            // Give up if too far or no hits for too long.
            if (distance > controller.Config.maxPursueRadius || bb.timeSinceLastSuccessfulHit >= controller.Config.giveUpTimeout)
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }
            // Transition to melee or ranged states.
            if (distance <= controller.Config.meleeRange)
            {
                controller.ChangeState(nameof(MeleeAttackState));
                return;
            }
            else if (distance <= controller.Config.rangedRange)
            {
                controller.ChangeState(nameof(RangedAttackState));
                return;
            }
            // Otherwise move toward the target and face it.
            controller.FaceTowards(targetObj.transform.position, dt);
            controller.Move(toTarget, dt);
        }
    }
}