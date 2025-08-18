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
            var myTeam = owner ? owner.GetComponent<MemeArena.Network.TeamId>() : null;

            var hits = Physics.OverlapSphere(center, radius, hitMask, QueryTriggerInteraction.Ignore);
            bool hitAny = false;
            int dmgAmount = overrideDamage.HasValue ? overrideDamage.Value : damage;
            foreach (var h in hits)
            {
                // Ignore self or same root
                if (owner != null && h.transform.root == owner.transform.root) continue;

                // Team gate: skip friendlies if both sides have TeamId
                if (myTeam != null)
                {
                    var targetTeam = h.GetComponentInParent<MemeArena.Network.TeamId>();
                    if (targetTeam != null && targetTeam.team == myTeam.team) continue;
                }

                var dmg = h.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    var hitPoint = h.ClosestPoint(center);
                    dmg.ApplyDamage(dmgAmount, owner, hitPoint);
                    // Raise unified combat event for success
                    var attackerNO = owner ? owner.GetComponent<Unity.Netcode.NetworkObject>() : null;
                    var victimNO = h.GetComponentInParent<Unity.Netcode.NetworkObject>();
                    if (attackerNO && victimNO)
                    {
                        CombatEvents.RaiseSuccessfulHit(attackerNO.NetworkObjectId, victimNO.NetworkObjectId, dmgAmount);
                    }
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
