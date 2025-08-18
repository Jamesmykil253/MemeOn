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
    private NetworkVariable<int> _currentHealth = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

        /// <summary>
        /// Maximum health.  Set from a CharacterStats or via inspector.
        /// </summary>
        [SerializeField]
        public int maxHealth = 100;

    public event Action<int, ulong> OnDamageReceived;
    public event Action OnDeath;
    /// <summary>
    /// Fired on clients and server whenever current health changes. Provides (current, max).
    /// </summary>
    public event Action<int, int> OnHealthChanged;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _currentHealth.OnValueChanged += OnHealthValueChanged;
            if (IsServer)
            {
                _currentHealth.Value = maxHealth; // sets and replicates to clients
                OnHealthChanged?.Invoke(_currentHealth.Value, maxHealth);
            }
            // Clients will receive an OnValueChanged when the server replication arrives;
            // avoid firing OnHealthChanged here with an uninitialized (0) value.
        }

    public override void OnDestroy()
        {
            base.OnDestroy();
            _currentHealth.OnValueChanged -= OnHealthValueChanged;
        }

        private void OnHealthValueChanged(int previous, int current)
        {
            OnHealthChanged?.Invoke(current, maxHealth);
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