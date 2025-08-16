using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// StunnedState incapacitates the AI for a brief period. After the stun ends, the
    /// AI resumes pursuing its target. The stun duration can be configured on the
    /// blackboard or via config if needed.
    /// </summary>
    public class StunnedState : AIState
    {
        private float _stunTimer = 1f;

        public StunnedState(AIController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            _stunTimer = 1f; // default stun duration; could be configured per attack.
            controller.StopMovement();
        }

        public override void Tick(float deltaTime)
        {
            _stunTimer -= deltaTime;
            if (_stunTimer <= 0f)
            {
                // After stun, pursue if a target exists; otherwise return home.
                if (controller.IsTargetAlive())
                {
                    controller.ChangeState(nameof(PursueState));
                }
                else
                {
                    controller.ChangeState(nameof(ReturnToSpawnState));
                }
            }
        }
    }
}