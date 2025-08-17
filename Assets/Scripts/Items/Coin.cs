using Unity.Netcode;
using UnityEngine;
using MemeArena.Items;
using MemeArena.Spawning;

namespace MemeArena.Items
{
    /// <summary>
    /// Represents a collectible coin in the world.  Coins rotate for a visual
    /// effect on clients and award players when picked up on the server.  A
    /// collider with IsTrigger enabled is required on the GameObject.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    public class Coin : NetworkBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Degrees per second to rotate the coin around the Y axis on clients.")]
        [SerializeField] private float rotationSpeed = 180f;

        /// <summary>
        /// Reference to the spawner that created this coin.  Assigned by the
        /// CoinSpawner when spawning.  Used to schedule respawns after pickup.
        /// </summary>
        [HideInInspector] public CoinSpawner spawner;

        /// <summary>
        /// The spawn point from which this coin originated.  Provided by
        /// CoinSpawner so that coins respawn at the same location.
        /// </summary>
        [HideInInspector] public Transform spawnPoint;

        private void Update()
        {
            // Rotate only on clients.  The server does not need to update
            // rotation; rotation does not affect gameplay logic.
            if (!IsServer)
            {
                transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Only handle pickup on the server so that the authoritative
            // inventory and spawner logic runs once.  Clients will see the
            // coin disappear via network replication.
            if (!IsServer) return;

            // Only players can collect coins.  Use the tag defined in
            // ProjectConstants to avoid magic strings.
            if (!other.CompareTag(MemeArena.Network.ProjectConstants.Tags.Player)) return;

            // Get the player's inventory component and add a coin.
            var inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            inventory.AddCoin();

            // Inform the spawner that this coin was collected so it can
            // schedule a respawn.  The spawner will check for null.
            if (spawner != null && spawnPoint != null)
            {
                spawner.OnCoinCollected(spawnPoint);
            }

            // Despawn this coin across the network.  The NetworkObject
            // component must exist on the same GameObject.
            NetworkObject.Despawn();
        }
    }
}