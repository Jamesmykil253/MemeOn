using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Executes a ranged attack by spawning a projectile and then waits for
    /// the attack cooldown before returning to the pursue state.
    /// </summary>
    public class RangedAttackState : AIState
    {
        private float _cooldownTimer;

        public RangedAttackState(AIController controller) : base(controller, nameof(RangedAttackState)) { }

        public override void Enter()
        {
            controller.PerformRangedAttack();
            _cooldownTimer = controller.Config.attackCooldown;
        }

        public override void Tick(float dt)
        {
            _cooldownTimer -= dt;
            if (_cooldownTimer <= 0f)
            {
                controller.ChangeState(nameof(PursueState));
            }
        }
    }
}