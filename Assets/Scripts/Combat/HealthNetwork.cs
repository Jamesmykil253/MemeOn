using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Combat
{
    [DisallowMultipleComponent]
    public class HealthNetwork : NetworkBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHealth = 100;
        public NetworkVariable<int> Health = new(writePerm: NetworkVariableWritePermission.Server);

        private void Awake()
        {
            Health.Value = maxHealth;
        }

        public void ApplyDamage(int amount, GameObject source, Vector3 hitPoint)
        {
            if (!IsServer) return;
            var before = Health.Value;
            Health.Value = Mathf.Max(0, Health.Value - Mathf.Abs(amount));

            var e = new HitEvents.DamageEvent
            {
                Source = source,
                Victim = gameObject,
                HitPoint = hitPoint,
                Amount = Mathf.Abs(amount)
            };

            HitEvents.RaiseDamageReceived(e);
            if (before != Health.Value && Health.Value <= 0)
            {
                // optional: notify death
            }
            else
            {
                // acknowledge successful hit
                HitEvents.RaiseSuccessfulHit(e);
            }
        }
    }
}
