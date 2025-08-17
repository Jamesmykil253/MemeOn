using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Combat
{
    /// <summary>
    /// Server-authoritative health. Initializes in OnNetworkSpawn (not Awake/Start)
    /// to avoid the NetworkVariable pre-spawn warning.
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
            if (IsServer)
            {
                // If value invalid or zero at spawn, set to max
                if (CurrentHealth.Value <= 0 || CurrentHealth.Value > maxHealth)
                    CurrentHealth.Value = maxHealth;
            }
        }

        public void Heal(int amount)
        {
            if (!IsServer || amount <= 0) return;
            CurrentHealth.Value = Mathf.Min(maxHealth, CurrentHealth.Value + amount);
        }

        public void Damage(int amount)
        {
            if (!IsServer || amount <= 0) return;
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