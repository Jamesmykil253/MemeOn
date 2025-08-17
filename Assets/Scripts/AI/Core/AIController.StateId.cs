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

            // End of modern state list
        }
    }
}