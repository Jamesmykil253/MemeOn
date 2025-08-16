using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Executes a melee attack on the current target. After the attack is performed
    /// the AI returns to Pursue to re-evaluate. The cooldown is controlled via
    /// AIConfig.attackCooldown.
    /// </summary>
    public class MeleeAttackState : AIState
    {
        private float _cooldownTimer;
        private bool _hasAttacked;

        public MeleeAttackState(AIController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            _hasAttacked = false;
            _cooldownTimer = controller.Config.attackCooldown;
        }

        public override void Tick(float deltaTime)
        {
            // If target is gone, return to spawn.
            if (!controller.IsTargetAlive())
            {
                controller.ChangeState(nameof(ReturnToSpawnState));
                return;
            }

            // Attack once on first tick.
            if (!_hasAttacked)
            {
                controller.PerformMeleeAttack();
                _hasAttacked = true;
            }

            // Wait for cooldown before chasing again.
            _cooldownTimer -= deltaTime;
            if (_cooldownTimer <= 0f)
            {
                controller.ChangeState(nameof(PursueState));
            }
        }
    }
}