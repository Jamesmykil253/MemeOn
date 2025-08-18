using Unity.Netcode;
using UnityEngine;
using MemeArena.Combat;

namespace MemeArena.Players
{
    /// <summary>
    /// Provides combat functionality for the player.  This component lives on the
    /// player prefab and is responsible for spawning projectiles on the server when
    /// the player fires.  The RPC naming follows the Netcode for GameObjects
    /// convention that server RPC methods end with the suffix "ServerRpc".
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(MemeArena.Combat.BoostedAttackTracker))]
    public class PlayerCombatController : NetworkBehaviour
    {
        [Tooltip("Projectile prefab to spawn when firing.")]
        public GameObject projectilePrefab;

        [Tooltip("Damage dealt by each projectile.")]
        public int damage = 10;
    [Tooltip("Optional: Melee weapon component to perform basic melee swings on server.")]
    public MemeArena.Combat.MeleeWeaponServer meleeWeapon;
    [Header("Melee (built-in fallback)")]
    [Tooltip("Used if no MeleeWeaponServer is present. Radius of the melee hit sphere at the end of the swing arc.")]
    public float meleeRadius = 1.2f;
    [Tooltip("Used if no MeleeWeaponServer is present. Forward distance from the player to the melee center.")]
    public float meleeRange = 1.8f;
    [Tooltip("Used if no MeleeWeaponServer is present. Damage per successful melee hit.")]
    public int meleeDamage = 15;
    [Tooltip("Used if no MeleeWeaponServer is present. Physics layer mask to hit.")]
    public LayerMask meleeHitMask = ~0;
    [Header("Boosted Attack (optional)")]
    [Tooltip("If enabled, after a number of normal shots the next shot will be boosted.")]
    public bool enableBoostedCycle = true;
    [Tooltip("Number of successful melee hits before granting a boosted attack (projectile next). Typical: 3.")]
    public int shotsBeforeBoost = 3;
    [Tooltip("Additional damage applied to the boosted shot.")]
    public int boostedDamageBonus = 10;
    [Header("Debugging")]
    [SerializeField] private bool debugLogs = false;
    [SerializeField] private bool auditLogs = true;

        private MemeArena.Combat.BoostedAttackTracker _boost;
        private int _sinceLastBoost;
        private int _auditFireCount;
        private int _auditBoostGrantedCount;
        private int _auditBoostConsumedCount;

        /// <summary>
        /// Requests the server to fire a projectile.  This RPC must be called on
        /// the server and will instantiate and spawn the projectile prefab.  The
        /// method name ends with "ServerRpc" to satisfy the NGO codegen rules.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void FireServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            if (rpcParams.Receive.SenderClientId != NetworkObject.OwnerClientId)
            {
                if (debugLogs)
                    Debug.LogWarning($"PlayerCombatController(Server): Ignoring fire from non-owner {rpcParams.Receive.SenderClientId} (owner={NetworkObject.OwnerClientId})");
                return;
            }
            if (debugLogs)
            {
                Debug.Log($"PlayerCombatController(Server): Fire requested by {rpcParams.Receive.SenderClientId}, owner={OwnerClientId}");
            }
            if (auditLogs) { _auditFireCount++; Debug.Log($"AUDIT PlayerCombat(Server): Fire RPC accepted count={_auditFireCount}"); }

            // Compute damage with optional boosted logic
            int dmgToUse = damage;
            if (_boost == null) _boost = GetComponent<MemeArena.Combat.BoostedAttackTracker>();

            // Boosted path: projectile only; consumes boost
            if (enableBoostedCycle && _boost != null && _boost.IsBoosted)
            {
                dmgToUse = Mathf.Max(0, damage + boostedDamageBonus);
                if (projectilePrefab != null)
                {
                    Vector3 spawnPos = transform.position + transform.forward * 0.6f + Vector3.up * 0.8f;
                    var go = Instantiate(projectilePrefab, spawnPos, transform.rotation);
                    var proj = go.GetComponent<ProjectileServer>();
                    if (proj == null) { proj = go.AddComponent<ProjectileServer>(); }
                    proj.Launch(gameObject, dmgToUse, 22f, 3f);
                    var netObj = go.GetComponent<NetworkObject>();
                    if (debugLogs) Debug.Log("PlayerCombatController(Server): Spawning projectile NetworkObject (boosted)");
                    netObj?.Spawn();
                    if (auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): BOOSTED projectile spawned dmg={dmgToUse} pos={spawnPos}");
                }
                else
                {
                    if (debugLogs) Debug.LogWarning("PlayerCombatController(Server): Boosted shot requested but projectilePrefab is not assigned. Falling back to melee.");
                    // Fall back to melee even during boosted if no projectile prefab is assigned
                    bool boostedMelee = false;
                    if (meleeWeapon != null)
                    {
                        boostedMelee = meleeWeapon.PerformSwing(gameObject, dmgToUse);
                    }
                    else
                    {
                        boostedMelee = ServerPerformMeleeSweep(dmgToUse);
                    }
                    if (auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): BOOSTED fallback melee hit={boostedMelee} dmg={dmgToUse}");
                }
                // consume boost
                _boost.SetBoosted(false);
                _sinceLastBoost = 0;
                _auditBoostConsumedCount++;
                SetBoostReadyClientRpc(false);
                FlashAttackClientRpc(true);
                return;
            }

            // Non-boosted path: melee-only; grant boost on successful melee hits
            bool didMelee = false;
            if (meleeWeapon != null)
            {
                didMelee = meleeWeapon.PerformSwing(gameObject);
                if (auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): Melee swing attempted hit={didMelee}");
                if (enableBoostedCycle && _boost != null && didMelee)
                {
                    _sinceLastBoost = Mathf.Clamp(_sinceLastBoost + 1, 0, 1000000);
                    if (shotsBeforeBoost > 0 && _sinceLastBoost >= shotsBeforeBoost)
                    {
                        _boost.SetBoosted(true);
                        _sinceLastBoost = 0;
                        _auditBoostGrantedCount++;
                        if (debugLogs || auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): Boost GRANTED after melee hits count={_auditBoostGrantedCount}");
                        SetBoostReadyClientRpc(true);
                    }
                }
            }
            else
            {
                // Built-in fallback melee sweep to avoid requiring separate weapon components
                didMelee = ServerPerformMeleeSweep(meleeDamage);
                if (auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): Fallback melee sweep hit={didMelee}");
                if (!didMelee && debugLogs)
                {
                    Debug.Log("PlayerCombatController(Server): Fallback melee found no targets.");
                }
                if (enableBoostedCycle && _boost != null && didMelee)
                {
                    _sinceLastBoost = Mathf.Clamp(_sinceLastBoost + 1, 0, 1000000);
                    if (shotsBeforeBoost > 0 && _sinceLastBoost >= shotsBeforeBoost)
                    {
                        _boost.SetBoosted(true);
                        _sinceLastBoost = 0;
                        _auditBoostGrantedCount++;
                        if (debugLogs || auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): Boost GRANTED after melee hits count={_auditBoostGrantedCount}");
                        SetBoostReadyClientRpc(true);
                    }
                }
            }

            // Visual: normal attack flash for melee
            FlashAttackClientRpc(false);
        }

        /// <summary>
        /// Server-side melee sweep used when no MeleeWeaponServer is available.
        /// Returns true if any IDamageable was hit and damaged.
        /// </summary>
        private bool ServerPerformMeleeSweep(int dmgAmount)
        {
            if (!IsServer) return false;
            var origin = transform.position + Vector3.up * 0.9f;
            var dir = transform.forward;
            var center = origin + dir * meleeRange;
            var myTeam = GetComponent<MemeArena.Network.TeamId>();
            var hits = Physics.OverlapSphere(center, meleeRadius, meleeHitMask, QueryTriggerInteraction.Ignore);
            bool hitAny = false;
            foreach (var h in hits)
            {
                // Ignore self or same root
                if (h.transform.root == transform.root) continue;

                // Team gate: skip friendlies if both sides have TeamId
                if (myTeam != null)
                {
                    var targetTeam = h.GetComponentInParent<MemeArena.Network.TeamId>();
                    if (targetTeam != null && targetTeam.team == myTeam.team) continue;
                }

                var dmg = h.GetComponentInParent<MemeArena.Combat.IDamageable>();
                if (dmg != null)
                {
                    dmg.ApplyDamage(dmgAmount, gameObject, h.ClosestPoint(center));
                    hitAny = true;
                }
            }
            return hitAny;
        }

        [ClientRpc]
        private void FlashAttackClientRpc(bool boostedShot)
        {
            var sync = GetComponent<MemeArena.Debugging.PlayerStateColorSync>();
            sync?.FlashAttack(boostedShot);
        }

        [ClientRpc]
        private void SetBoostReadyClientRpc(bool ready)
        {
            var sync = GetComponent<MemeArena.Debugging.PlayerStateColorSync>();
            sync?.SetBoostReady(ready);
        }
    }
}