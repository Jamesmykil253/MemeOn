using UnityEngine;
using Unity.Netcode;

namespace MemeArena.AI
{
    /// <summary>
    /// Temporarily incapacitates the AI.  Used for crowd control effects.
    /// During this state the AI neither moves nor rotates.  After the
    /// stunned duration the AI either resumes pursuing its target or returns
    /// home if no target exists.
    /// </summary>
    public class StunnedState : AIState
    {
        private float _stunTimer;

        public StunnedState(AIController controller) : base(controller, nameof(StunnedState)) { }

        public override void Enter()
        {
            _stunTimer = controller.Config.stunnedDuration;
        }

        public override void Tick(float dt)
        {
            _stunTimer -= dt;
            if (_stunTimer <= 0f)
            {
                var bb = controller.Blackboard;
                if (bb.aggroed && bb.targetId != 0)
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