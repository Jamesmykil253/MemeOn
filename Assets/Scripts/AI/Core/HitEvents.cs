using System;
using UnityEngine;

namespace MemeArena.Combat
{
    /// <summary>
    /// Canonical event payloads for consistency across systems.
    /// </summary>
    public static class HitEvents
    {
        public struct DamageEvent
        {
            public GameObject Source;
            public GameObject Victim;
            public Vector3 HitPoint;
            public int Amount;
        }

        public static event Action<DamageEvent> OnDamageReceived;  // fires on victim
        public static event Action<DamageEvent> OnSuccessfulHit;   // fires on attacker upon confirmed damage
        public static event Action<DamageEvent> OnFailedHit;       // fires on attacker if missed / blocked

        public static void RaiseDamageReceived(DamageEvent e) => OnDamageReceived?.Invoke(e);
        public static void RaiseSuccessfulHit(DamageEvent e) => OnSuccessfulHit?.Invoke(e);
        public static void RaiseFailedHit(DamageEvent e) => OnFailedHit?.Invoke(e);
    }
}
