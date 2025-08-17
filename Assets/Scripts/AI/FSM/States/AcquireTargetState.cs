using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Legacy state used to acquire a target.  In the current AI design,
    /// target acquisition occurs within the Idle/Alert states and this state
    /// is no longer used.  However, it is provided as a stub to ensure
    /// compatibility with older code that references <c>AcquireTargetState</c>.
    /// Upon ticking, it immediately transitions to the <see cref="AlertState"/>
    /// to begin pursuit of the newly aggroed target.
    /// </summary>
    public class AcquireTargetState : AIState
    {
        private float _timer;

        public AcquireTargetState(AIController controller) : base(controller, nameof(AcquireTargetState))
        {
        }

        public override void Enter()
        {
            _timer = 0f;
        }

        public override void Tick(float dt)
        {
            // Wait a brief moment before transitioning to Alert state.  This
            // mimics the reaction delay found in older implementations.
            _timer += dt;
            if (_timer < 0.1f) return;
            // Immediately switch to AlertState; target acquisition logic is
            // handled by the alert/pursue states in the current design.
            controller.ChangeState(nameof(AlertState));
        }

        public override void Exit() { }
    }
}