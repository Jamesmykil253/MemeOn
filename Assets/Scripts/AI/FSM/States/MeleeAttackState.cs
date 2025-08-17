using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Executes a melee attack and then waits for the attack cooldown before
    /// returning to the pursue state.  On entry the attack is immediately
    /// attempted; the cooldown timer always counts down even if the attack
    /// fails.
    /// </summary>
    public class MeleeAttackState : AIState
    {
        private float _cooldownTimer;

        public MeleeAttackState(AIController controller) : base(controller, nameof(MeleeAttackState)) { }

        public override void Enter()
        {
            // Perform attack once on entry.
            controller.PerformMeleeAttack();
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