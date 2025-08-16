using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Executes a ranged attack by spawning a projectile aimed at the current target.
    /// After firing, waits for the attack cooldown then returns to Pursue.
    /// </summary>
    public class RangedAttackState : AIState
    {
        private float _cooldownTimer;
        private bool _hasAttacked;

        public RangedAttackState(AIController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            _hasAttacked = false;
            _cooldownTimer = controller.Config.attackCooldown;
        }

        public override void Tick(float deltaTime)
        {
            // If target died or lost, return to spawn.
            if (!controller.IsTargetAlive())
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }
            if (!_hasAttacked)
            {
                controller.PerformRangedAttack();
                _hasAttacked = true;
            }
            _cooldownTimer -= deltaTime;
            if (_cooldownTimer <= 0f)
            {
                controller.ChangeState(nameof(PursueState));
            }
        }
    }
}