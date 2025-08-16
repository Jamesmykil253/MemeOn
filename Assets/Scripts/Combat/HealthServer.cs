using System;
using UnityEngine;
using Unity.Netcode;

namespace MemeArena.Combat
{
    /// <summary>
    /// Authoritative health component that runs only on the server. Damage can only be
    /// applied via server RPCs. It exposes events for damage received and death so
    /// listeners can respond (e.g. AIController reacts to damage, AISpawnerManager
    /// handles respawn).
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class HealthServer : NetworkBehaviour
    {
        [Tooltip("Maximum health points for this entity.")]
        public int maxHealth = 100;

        private NetworkVariable<int> _currentHealth = new NetworkVariable<int>();

        /// <summary>
        /// Invoked on the server whenever damage is applied. The ulong parameter is the
        /// NetworkObjectId of the attacker.
        /// </summary>
        public event Action<ulong> OnDamageReceived;
        /// <summary>
        /// Invoked on the server when health reaches zero.
        /// </summary>
        public event Action OnDeath;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _currentHealth.Value = maxHealth;
            }
        }

        /// <summary>
        /// Server RPC that applies damage to this entity. The caller should not require
        /// ownership as AI and other actors may attack freely.
        /// </summary>
        /// <param name="damage">Amount of damage to apply.</param>
        /// <param name="attackerId">NetworkObjectId of the attacker.</param>
        [ServerRpc(RequireOwnership = false)]
        public void ApplyDamageServerRpc(int damage, ulong attackerId)
        {
            if (!IsServer) return;
            if (damage <= 0) return;
            _currentHealth.Value = Math.Max(_currentHealth.Value - damage, 0);
            OnDamageReceived?.Invoke(attackerId);
            if (_currentHealth.Value <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        /// <summary>
        /// Gets the current health value. Use this on the server to inspect health. On
        /// clients the value will replicate if read from the NetworkVariable.
        /// </summary>
        public int CurrentHealth => _currentHealth.Value;
    }
}