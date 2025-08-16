using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// AlertState provides a brief reaction delay after the AI is provoked before it
    /// begins pursuing the attacker. This creates a more natural response rather than
    /// immediately switching to the pursue state.
    /// </summary>
    public class AlertState : AIState
    {
        private float _reactionTimer;
        private float _reactionDelay;

        public AlertState(AIController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            // Sample a reaction delay using a simple uniform distribution around the mean.
            float mean = controller.Config.reactionDelayMean;
            float std = controller.Config.reactionDelayStdDev;
            // Approximate a Gaussian by summing two uniform randoms.
            float randomSample = (Random.value + Random.value - 1f) * std;
            _reactionDelay = Mathf.Max(0f, mean + randomSample);
            _reactionTimer = _reactionDelay;
        }

        public override void Tick(float deltaTime)
        {
            _reactionTimer -= deltaTime;
            if (_reactionTimer <= 0f)
            {
                controller.ChangeState(nameof(PursueState));
            }
        }
    }
}