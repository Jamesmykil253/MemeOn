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
    [Header("Boosted Attack (optional)")]
    [Tooltip("If enabled, after a number of normal shots the next shot will be boosted.")]
    public bool enableBoostedCycle = true;
    [Tooltip("Number of normal shots before granting a boosted shot (e.g., 3 → every third grants boost for the next).")]
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
            if (!projectilePrefab) return;
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
            bool wasBoostedShot = false;
            if (enableBoostedCycle)
            {
                if (_boost == null) _boost = GetComponent<MemeArena.Combat.BoostedAttackTracker>();
                if (_boost != null)
                {
                    if (_boost.IsBoosted)
                    {
                        // Consume boost on this shot
                        dmgToUse = Mathf.Max(0, damage + boostedDamageBonus);
                        _boost.SetBoosted(false);
                        _sinceLastBoost = 0;
                        _auditBoostConsumedCount++;
                        wasBoostedShot = true;
                        if (debugLogs || auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): Boosted CONSUMED count={_auditBoostConsumedCount} dmg={dmgToUse}");
                    }
                    else
                    {
                        _sinceLastBoost = Mathf.Clamp(_sinceLastBoost + 1, 0, 1000000);
                        if (shotsBeforeBoost > 0 && _sinceLastBoost >= shotsBeforeBoost)
                        {
                            // After the threshold, grant boost for the NEXT shot
                            _boost.SetBoosted(true);
                            _sinceLastBoost = 0;
                            _auditBoostGrantedCount++;
                            if (debugLogs || auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): Boost GRANTED count={_auditBoostGrantedCount}");
                            SetBoostReadyClientRpc(true);
                        }
                    }
                }
            }

            bool didMelee = false;
            // Try melee first if available
            if (meleeWeapon != null)
            {
                didMelee = meleeWeapon.PerformSwing(gameObject);
                if (auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): Melee swing attempted hit={didMelee}");
            }

            // Then ranged (projectile) if prefab is assigned
            if (projectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + transform.forward * 0.6f + Vector3.up * 0.8f;
                var go = Instantiate(projectilePrefab, spawnPos, transform.rotation);
                var proj = go.GetComponent<ProjectileServer>();
                if (proj == null)
                {
                    proj = go.AddComponent<ProjectileServer>();
                }
                // Initialise the projectile with the firing direction, owner, damage, speed and lifetime.
                proj.Launch(gameObject, dmgToUse, 22f, 3f);
                var netObj = go.GetComponent<NetworkObject>();
                if (debugLogs) Debug.Log("PlayerCombatController(Server): Spawning projectile NetworkObject");
                netObj?.Spawn();
                if (auditLogs) Debug.Log($"AUDIT PlayerCombat(Server): Projectile spawned dmg={dmgToUse} pos={spawnPos}");
            }

            // Visual: flash attack color on clients; if boosted shot, flash boosted variant and clear boost-ready persistent
            FlashAttackClientRpc(wasBoostedShot);
            if (wasBoostedShot)
            {
                SetBoostReadyClientRpc(false);
            }
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