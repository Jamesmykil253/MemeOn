using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Spawning
{
    /// <summary>
    /// Manages spawning of coin prefabs at configured spawn points.  Only
    /// executes on the server; clients receive spawned coins via network
    /// replication.  When coins are collected, the spawner schedules
    /// respawns after a configurable delay.
    /// </summary>
    [DisallowMultipleComponent]
    public class CoinSpawner : NetworkBehaviour
    {
        [Header("Spawning Settings")]
        [Tooltip("Prefab of the coin to spawn.  Must have a NetworkObject and Coin component.")]
        [SerializeField] private GameObject coinPrefab;

        [Tooltip("How long to wait before respawning a coin after it has been collected.")]
        [SerializeField] private float respawnDelaySeconds = 10f;

        [Tooltip("List of transforms indicating where coins should spawn.")]
        [SerializeField] private Transform[] spawnPoints;

        // Keep track of active coins by their spawn point.  Used to avoid
        // spawning multiple coins at the same location simultaneously.
        private readonly Dictionary<Transform, GameObject> _activeCoins = new Dictionary<Transform, GameObject>();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Spawn a coin at every spawn point when the network
                // spawner starts.  This ensures coins exist at the start
                // of the game.
                foreach (var point in spawnPoints)
                {
                    SpawnCoin(point);
                }
            }
        }

        /// <summary>
        /// Spawn a coin at the given spawn point.  Should only be called on the server.
        /// </summary>
        private void SpawnCoin(Transform spawnPoint)
        {
            if (_activeCoins.ContainsKey(spawnPoint)) return;

            var coinObj = Instantiate(coinPrefab, spawnPoint.position, Quaternion.identity);
            var networkObject = coinObj.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }

            // Assign the spawner and spawn point to the coin so that it can
            // call back when collected.
            var coin = coinObj.GetComponent<MemeArena.Items.Coin>();
            if (coin != null)
            {
                coin.spawner = this;
                coin.spawnPoint = spawnPoint;
            }

            _activeCoins[spawnPoint] = coinObj;
        }

        /// <summary>
        /// Called by a coin when it is collected.  Schedules a respawn at the
        /// given spawn point after the configured delay.  Only runs on the server.
        /// </summary>
        /// <param name="spawnPoint">The spawn point from which the coin was taken.</param>
        public void OnCoinCollected(Transform spawnPoint)
        {
            if (!IsServer) return;
            // Remove reference to the old coin so that a new one can be spawned.
            _activeCoins.Remove(spawnPoint);
            // Start coroutine to respawn after the delay.
            StartCoroutine(RespawnCoroutine(spawnPoint));
        }

        private IEnumerator RespawnCoroutine(Transform spawnPoint)
        {
            yield return new WaitForSeconds(respawnDelaySeconds);
            SpawnCoin(spawnPoint);
        }
    }
}