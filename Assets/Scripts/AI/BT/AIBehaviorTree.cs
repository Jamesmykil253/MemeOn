namespace MemeArena.AI
{
    /// <summary>
    /// Placeholder behaviour tree used by AIController. In this prototype the BT
    /// evaluation is deferred until later passes. The Tick method currently performs no
    /// operations but is left in place to satisfy compiler references.
    /// </summary>
    public class AIBehaviorTree
    {
        private readonly AIController _controller;

        public AIBehaviorTree(AIController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Evaluates the behaviour tree. Called on the server at the rate defined in
        /// ProjectConstants.AI.BehaviorTickRate. In a future implementation this
        /// method will inspect blackboard values and enqueue high-level intents.
        /// </summary>
        public void Tick(float deltaTime)
        {
            // Intentionally left blank for prototype. Decision logic will be added later.
        }
    }
}