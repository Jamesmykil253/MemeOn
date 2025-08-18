using Unity.Netcode;
using UnityEngine;
using MemeArena.Players;

namespace MemeArena.Items
{
    /// <summary>
    /// Rotates visually; on server grants 1 coin if a player walks over it.
    /// Requires: NetworkObject + Trigger Collider on the coin.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class Coin : NetworkBehaviour
    {
        public float rotateDegPerSec = 90f;
    // Optional: set by CoinSpawner so it can respawn coins after collection
    [HideInInspector] public MemeArena.Spawning.CoinSpawner spawner;
    [HideInInspector] public Transform spawnPoint;

        void Update()
        {
            // purely visual; can run on all clients
            transform.Rotate(0f, rotateDegPerSec * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            // Explicitly use Players.PlayerInventory to avoid resolving to Items.PlayerInventory
            var inv = other.GetComponentInParent<MemeArena.Players.PlayerInventory>();
            if (inv != null)
            {
                inv.AddCoins(1);
                // Notify spawner before despawn so it can schedule a respawn
                if (spawner != null && spawnPoint != null)
                {
                    spawner.OnCoinCollected(spawnPoint);
                }
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }
}
