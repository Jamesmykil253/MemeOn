namespace MemeArena.AI
{
    /// <summary>
    /// Legacy finite state machine used to bridge old AI scripts to the new
    /// server‑authoritative implementation.  This class exposes a
    /// ChangeState method that maps legacy <see cref="AIController.AIStateId"/>
    /// values to the appropriate state names on the modern AI controller.
    /// </summary>
    internal class LegacyFSM
    {
        private readonly AIController _controller;

        public LegacyFSM(AIController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Changes the AI controller to the state corresponding to the
        /// provided legacy identifier.  Unrecognised values default to
        /// IdleState.  This ensures older scripts that call
        /// fsm.ChangeState() continue to function without modification.
        /// </summary>
        /// <param name="id">The legacy state identifier.</param>
        public void ChangeState(AIController.AIStateId id)
        {
            string stateName;
            switch (id)
            {
                case AIController.AIStateId.Aim:
                case AIController.AIStateId.AcquireTarget:
                    stateName = nameof(AlertState);
                    break;
                case AIController.AIStateId.Attack:
                case AIController.AIStateId.Cooldown:
                    // Default to ranged attack; melee or ranged decisions are
                    // handled in the Pursue state now.
                    stateName = nameof(RangedAttackState);
                    break;
                case AIController.AIStateId.Evade:
                    stateName = nameof(EvadeState);
                    break;
                case AIController.AIStateId.Stunned:
                    stateName = nameof(StunnedState);
                    break;
                case AIController.AIStateId.Dead:
                    stateName = nameof(DeadState);
                    break;
                case AIController.AIStateId.ReturnToSpawn:
                    stateName = nameof(ReturnToSpawnState);
                    break;
                case AIController.AIStateId.Idle:
                    stateName = nameof(IdleState);
                    break;
                case AIController.AIStateId.Pursue:
                    stateName = nameof(PursueState);
                    break;
                default:
                    stateName = nameof(IdleState);
                    break;
            }
            _controller.ChangeState(stateName);
        }
    }
}