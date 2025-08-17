using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// AlertState is entered when the AI has taken damage but has not yet
    /// reacted.  The AI waits for a random reaction delay sampled from a
    /// normal distribution defined by AIConfig.  Once the timer expires
    /// it transitions into the PursueState.
    /// </summary>
    public class AlertState : AIState
    {
        private float _reactionTimer;

        public AlertState(AIController controller) : base(controller, nameof(AlertState)) { }

        public override void Enter()
        {
            // Sample reaction delay using a simple normal distribution.
            float mean = controller.Config.reactionDelayMean;
            float std = controller.Config.reactionDelayStdDev;
            // Box‑Muller transform: generate two uniform random numbers and
            // convert to a normally distributed value.
            float u1 = Random.value;
            float u2 = Random.value;
            float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
            _reactionTimer = Mathf.Max(0f, mean + std * randStdNormal);
        }

        public override void Tick(float dt)
        {
            _reactionTimer -= dt;
            if (_reactionTimer <= 0f)
            {
                controller.ChangeState(nameof(PursueState));
            }
        }
    }
}