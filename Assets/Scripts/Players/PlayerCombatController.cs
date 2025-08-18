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
    public class PlayerCombatController : NetworkBehaviour
    {
        [Tooltip("Projectile prefab to spawn when firing.")]
        public GameObject projectilePrefab;

        [Tooltip("Damage dealt by each projectile.")]
        public int damage = 10;
    [Header("Debugging")]
    [SerializeField] private bool debugLogs = false;

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

            // Spawn the projectile in front of the player.  Adjust the spawn offset
            // as needed to match the muzzle position on your character model.
            Vector3 spawnPos = transform.position + transform.forward * 0.6f + Vector3.up * 0.8f;
            var go = Instantiate(projectilePrefab, spawnPos, transform.rotation);
            var proj = go.GetComponent<ProjectileServer>();
            if (proj == null)
            {
                proj = go.AddComponent<ProjectileServer>();
            }
            // Initialise the projectile with the firing direction, owner, damage, speed and lifetime.
            proj.Launch(gameObject, damage, 22f, 3f);
            var netObj = go.GetComponent<NetworkObject>();
            if (debugLogs) Debug.Log("PlayerCombatController(Server): Spawning projectile NetworkObject");
            netObj?.Spawn();
        }
    }
}