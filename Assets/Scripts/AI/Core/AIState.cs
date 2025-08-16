using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Base class for all AI finite state machine states. Each state implements its own
    /// logic for entering, ticking, and exiting. States can request transitions via
    /// the AIController. Subclasses should be simple and deterministic.
    /// </summary>
    public abstract class AIState
    {
        protected readonly AIController controller;
        protected readonly AIBlackboard blackboard;

        protected AIState(AIController controller)
        {
            this.controller = controller;
            this.blackboard = controller.Blackboard;
        }

        /// <summary>
        /// Called when the state becomes active. Override to setup internal variables.
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Called every AI tick. deltaTime is the time in seconds since the last tick.
        /// </summary>
        public virtual void Tick(float deltaTime) { }

        /// <summary>
        /// Called when leaving the state. Override to clean up.
        /// </summary>
        public virtual void Exit() { }

        /// <summary>
        /// Gets the friendly name of the state for debugging.
        /// </summary>
        public virtual string Name => GetType().Name;
    }
}