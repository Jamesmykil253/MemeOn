namespace MemeArena.AI
{
    /// <summary>
    /// Base class for finite state machine states.  Each state owns a
    /// reference to the AIController and can override lifecycle methods.
    /// Concrete states should call controller.ChangeState to transition
    /// between states.  Tick is called at a fixed rate defined by
    /// ProjectConstants.AI.AITickRate.
    /// </summary>
    public abstract class AIState
    {
        /// <summary> Name of this state.  Used for lookup and debugging. </summary>
        public readonly string Name;

        protected readonly AIController controller;

        protected AIState(AIController controller, string name)
        {
            this.controller = controller;
            Name = name;
        }

        /// <summary> Called when entering this state. </summary>
        public virtual void Enter() { }

        /// <summary> Called once per AI tick while this state is active. </summary>
        /// <param name="dt">The time in seconds since the last tick.  Use
        /// this value to scale movement and timers.</param>
        public virtual void Tick(float dt) { }

        /// <summary> Called when exiting this state. </summary>
        public virtual void Exit() { }
    }
}