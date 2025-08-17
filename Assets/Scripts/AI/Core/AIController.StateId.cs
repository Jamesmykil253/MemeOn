using Unity.Netcode;
namespace MemeArena.AI
{
    /// <summary>
    /// Partial definition of <see cref="AIController"/> that declares the
    /// <see cref="AIStateId"/> enum.  This enum mirrors the server‑side
    /// state names used by the AI state machine.  It is provided to
    /// maintain backwards compatibility with older state implementations
    /// (e.g. AcquireTargetState) that expect a nested enum on
    /// <c>AIController</c>.
    /// </summary>
    public partial class AIController
    {
        /// <summary>
        /// Identifiers for the AI finite state machine.  These values
        /// correspond to the available states in the current AI
        /// implementation.  If additional states are added or renamed,
        /// update this enum accordingly.  Keeping the enum distinct
        /// enables legacy code to refer to states via <c>AIController.AIStateId</c>
        /// without directly depending on string constants.
        /// </summary>
        public enum AIStateId
        {
            Idle,
            Alert,
            Pursue,
            MeleeAttack,
            RangedAttack,
            Evade,
            Stunned,
            ReturnToSpawn,
            Dead

            , // legacy states below maintain compatibility with older BT/FSM scripts

        /// <summary>
        /// Legacy state used for aiming at a target.  In the new
        /// architecture this maps to the Alert state for turning towards
        /// a target before attacking.
        /// </summary>
        Aim,

        /// <summary>
        /// Legacy state representing an attack.  New code uses
        /// MeleeAttack and RangedAttack; this constant is retained for
        /// backwards compatibility.
        /// </summary>
        Attack,

        /// <summary>
        /// Legacy cooldown state.  Attack cooldowns are now handled via
        /// timers in AIController and do not require a separate state.
        /// </summary>
        Cooldown,

        /// <summary>
        /// Legacy state used for acquiring a target.  Target acquisition
        /// now occurs in the Alert and Pursue states.
        /// </summary>
        AcquireTarget
        }
    }
}