using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Combat
{
    /// <summary>
    /// Minimal server-side melee hitbox sweeper.
    /// </summary>
    public class MeleeWeaponServer : NetworkBehaviour
    {
        [Min(0f)] public float radius = 1.2f;
        [Min(0f)] public float range = 1.8f;
        public int damage = 15;
        public LayerMask hitMask = ~0;

        public bool PerformSwing(GameObject owner, int? overrideDamage = null)
        {
            if (!IsServer) return false;

            var origin = owner.transform.position + Vector3.up * 0.9f;
            var dir = owner.transform.forward;
            var center = origin + dir * range;

            var hits = Physics.OverlapSphere(center, radius, hitMask, QueryTriggerInteraction.Ignore);
            bool hitAny = false;
            int dmgAmount = overrideDamage.HasValue ? overrideDamage.Value : damage;
            foreach (var h in hits)
            {
                // Ignore self or same root
                if (owner != null && h.transform.root == owner.transform.root) continue;

                var dmg = h.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    dmg.ApplyDamage(dmgAmount, owner, h.ClosestPoint(center));
                    hitAny = true;
                }
            }

            if (!hitAny)
            {
                HitEvents.RaiseFailedHit(new HitEvents.DamageEvent { Source = owner, Victim = null, HitPoint = center, Amount = 0 });
            }
            return hitAny;
        }
    }
}
