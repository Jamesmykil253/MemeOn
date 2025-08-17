using UnityEngine;
using Unity.Netcode;

namespace MemeArena.AI
{
    /// <summary>
    /// Moves the AI away from its target for a short duration.  Used when
    /// attacks repeatedly fail or to avoid cluster collisions.  After the
    /// timer expires the AI returns to the pursue state.
    /// </summary>
    public class EvadeState : AIState
    {
        private float _evadeTimer;
        private Vector3 _moveDir;

        public EvadeState(AIController controller) : base(controller, nameof(EvadeState)) { }

        public override void Enter()
        {
            _evadeTimer = controller.Config.evadeDuration;
            // Compute a direction opposite to the target.  If no target use a
            // random direction.
            var bb = controller.Blackboard;
            Vector3 away;
            if (bb.targetId != 0 && NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(bb.targetId))
            {
                Vector3 toTarget = NetworkManager.Singleton.SpawnManager.SpawnedObjects[bb.targetId].transform.position - controller.transform.position;
                away = -toTarget.normalized;
            }
            else
            {
                away = Random.onUnitSphere;
                away.y = 0f;
                away.Normalize();
            }
            // Choose a perpendicular lateral offset to avoid linear retreat.
            Vector3 lateral = Vector3.Cross(away, Vector3.up);
            if (Random.value > 0.5f) lateral = -lateral;
            _moveDir = (away + 0.3f * lateral).normalized;
        }

        public override void Tick(float dt)
        {
            _evadeTimer -= dt;
            // Move each frame; do not rotate toward movement to emphasize panic.
            controller.Move(_moveDir, dt);
            if (_evadeTimer <= 0f)
            {
                controller.ChangeState(nameof(PursueState));
            }
        }
    }
}