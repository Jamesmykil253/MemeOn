namespace MemeArena.AI
{
    /// <summary>
    /// Very simple behaviour tree placeholder.  A complete implementation
    /// would include nodes for conditions, actions and composites (selector,
    /// sequence, etc.)  For this prototype we run AI purely through the
    /// finite state machine, so the behaviour tree does nothing.  However,
    /// leaving this class in place makes it easy to extend high‑level
    /// decision making in the future.
    /// </summary>
    public class BehaviorTree
    {
        /// <summary>
        /// Ticks the behaviour tree.  In a complete implementation this
        /// method would update the root node and propagate return statuses.
        /// </summary>
        public void Tick(float dt) { }
    }
}