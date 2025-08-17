using System;
using UnityEngine;
using Unity.Netcode;
using MemeArena.Network;

namespace MemeArena.Combat
{
    /// <summary>
    /// Authoritative health component for networked entities.  Keeps track of
    /// current health on the server and replicates the value to clients.  When
    /// health reaches zero, the OnDeath event is raised and the network
    /// object should transition into a dead state.  Clients should not call
    /// TakeDamageServerRpc directly; only the server should modify health.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkHealth : NetworkBehaviour
    {
        /// <summary>
        /// Current health of the object.  Synchronised to clients for UI
        /// display but authority resides on the server.
        /// </summary>
    private NetworkVariable<int> _currentHealth = new NetworkVariable<int>();

        /// <summary>
        /// Maximum health.  Set from a CharacterStats or via inspector.
        /// </summary>
        [SerializeField]
        public int maxHealth = 100;

        public event Action<int, ulong> OnDamageReceived;
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
        /// Apply damage to this entity.  Only callable on the server.
        /// </summary>
        /// <param name="amount">The amount of damage to apply.</param>
        /// <param name="attackerId">The network ID of the attacker.
        /// Provided to downstream AI so that they know who provoked them.</param>
        [ServerRpc]
        public void TakeDamageServerRpc(int amount, ulong attackerId)
        {
            if (!IsServer) return;
            if (_currentHealth.Value <= 0) return;
            _currentHealth.Value = Math.Max(0, _currentHealth.Value - amount);
            OnDamageReceived?.Invoke(amount, attackerId);
            CombatEvents.RaiseDamageReceived(GetComponent<NetworkObject>().NetworkObjectId, attackerId, amount);
            if (_currentHealth.Value == 0)
            {
                OnDeath?.Invoke();
            }
        }

        /// <summary>
        /// Heals this entity by the specified amount. Server only. Clamps to maxHealth.
        /// </summary>
        public void Heal(int amount)
        {
            if (!IsServer || amount <= 0) return;
            if (_currentHealth.Value <= 0) return; // dead entities don't heal
            _currentHealth.Value = Math.Min(maxHealth, _currentHealth.Value + amount);
        }

        /// <summary>
        /// Returns the current health on the client.  For server side logic
        /// please access the network variable directly.
        /// </summary>
    public int GetCurrentHealth() => _currentHealth.Value;
    public int MaxHealth => maxHealth;
    }
}