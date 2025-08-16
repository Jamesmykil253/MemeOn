using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// EvadeState briefly moves the AI away from its target to create spacing. After
    /// completing the evade manoeuvre the AI returns to pursuing the target. Evade
    /// direction is computed as the opposite of the target's direction with a slight
    /// sideways offset for unpredictability.
    /// </summary>
    public class EvadeState : AIState
    {
        private float _evadeTimer;
        private Vector3 _evadeTargetPosition;

        public EvadeState(AIController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            _evadeTimer = controller.Config.evadeDuration;
            // Determine a point to move to. Move opposite the direction to the target plus a lateral random.
            var targetObj = controller.FindTargetNetworkObject();
            Vector3 direction = Vector3.zero;
            if (targetObj != null)
            {
                direction = (controller.transform.position - targetObj.transform.position).normalized;
            }
            // Add a lateral component.
            Vector3 right = Vector3.Cross(direction, Vector3.up);
            float lateralSign = Random.value > 0.5f ? 1f : -1f;
            direction = (direction + right * 0.5f * lateralSign).normalized;
            _evadeTargetPosition = controller.transform.position + direction * controller.Config.evadeDistance;
        }

        public override void Tick(float deltaTime)
        {
            _evadeTimer -= deltaTime;
            // Move away until timer expires.
            float speed = blackboard.stats != null ? blackboard.stats.moveSpeed : 2f;
            controller.MoveTowards(_evadeTargetPosition, speed * 1.5f);
            if (_evadeTimer <= 0f)
            {
                controller.ChangeState(nameof(PursueState));
            }
        }
    }
}