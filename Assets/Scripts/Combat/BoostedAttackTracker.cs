using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Combat
{
    /// <summary>
    /// Tracks whether the player's next basic attack is boosted.  The
    /// underlying game logic should toggle this value on the server
    /// whenever the conditions for a boosted attack are met or consumed.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class BoostedAttackTracker : NetworkBehaviour
    {
        private NetworkVariable<bool> _isBoosted = new NetworkVariable<bool>(false);

        /// <summary>
        /// Indicates whether the next basic attack is boosted.  Clients read
        /// this value to update the UI; only the server should write to it.
        /// </summary>
        public bool IsBoosted => _isBoosted.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _isBoosted.Value = false;
            }
        }

        /// <summary>
        /// Toggle the boosted state.  Should be called on the server when the
        /// conditions for a boosted attack change.  For example, upon landing
        /// three basic attacks or consuming the boost.
        /// </summary>
        public void SetBoosted(bool value)
        {
            if (!IsServer) return;
            _isBoosted.Value = value;
        }
    }
}