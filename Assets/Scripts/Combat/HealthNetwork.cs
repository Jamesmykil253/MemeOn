using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Combat
{
    /// <summary>
    /// Server-authoritative health. Fixes the NV-before-spawn warning by
    /// initializing in OnNetworkSpawn (NOT Awake/Start).
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class HealthNetwork : NetworkBehaviour
    {
        [Header("Health")]
        [Min(1)] public int maxHealth = 100;

        public NetworkVariable<int> CurrentHealth =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public bool IsDead => CurrentHealth.Value <= 0;

        public override void OnNetworkSpawn()
        {
            // Initialize only once the NetworkObject is spawned.
            if (IsServer)
            {
                // Preserve value if this is a respawn, otherwise set to max.
                if (CurrentHealth.Value <= 0 || CurrentHealth.Value > maxHealth)
                    CurrentHealth.Value = maxHealth;
            }
        }

        [Server] public void Heal(int amount)
        {
            if (amount <= 0 || !IsServer) return;
            CurrentHealth.Value = Mathf.Min(maxHealth, CurrentHealth.Value + amount);
        }

        [Server] public void Damage(int amount)
        {
            if (amount <= 0 || !IsServer) return;
            if (IsDead) return;
            CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - amount);
            if (CurrentHealth.Value == 0)
            {
                OnDeath?.Invoke(this);
            }
        }

        public event System.Action<HealthNetwork> OnDeath;
    }
}
