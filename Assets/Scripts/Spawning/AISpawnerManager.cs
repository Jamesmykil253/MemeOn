using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MemeArena.Network;

namespace MemeArena.Spawning
{
    /// <summary>
    /// Manages spawning and respawning of AI prefabs.  Spawn points are
    /// defined in the inspector.  Only runs on the server; clients receive
    /// spawned objects through Netcode replication.  When an AI dies, this
    /// manager schedules a respawn after a configurable delay.
    /// </summary>
    public class AISpawnerManager : NetworkBehaviour
    {
        [Tooltip("Prefab to spawn for AI.  Must contain a NetworkObject and AIController.")]
        public GameObject aiPrefab;

        [Tooltip("Spawn locations for AI.  Drag empty GameObjects here.")]
        public List<Transform> spawnPoints = new List<Transform>();

        [Tooltip("Maximum number of AI instances to spawn at once.")]
        public int maxCount = 3;

        [Tooltip("Whether dead AIs will respawn after a delay.")]
        public bool allowRespawn = true;

        [Tooltip("Delay before respawning a dead AI, in seconds.")]
        public float respawnDelay = ProjectConstants.Match.RespawnDelay;

        private readonly HashSet<ulong> _aliveAI = new HashSet<ulong>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;
            SpawnInitialAIs();
        }

        /// <summary>
        /// Spawns AI up to maxCount at the configured spawn points.
        /// </summary>
        private void SpawnInitialAIs()
        {
            if (aiPrefab == null || spawnPoints.Count == 0) return;
            for (int i = 0; i < Mathf.Min(maxCount, spawnPoints.Count); i++)
            {
                SpawnAIAt(spawnPoints[i]);
            }
        }

        private void SpawnAIAt(Transform point)
        {
            GameObject obj = Instantiate(aiPrefab, point.position, point.rotation);
            var netObj = obj.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogWarning("AI prefab missing NetworkObject.  Cannot spawn.");
                Destroy(obj);
                return;
            }
            netObj.Spawn(true);
            _aliveAI.Add(netObj.NetworkObjectId);
        }

        /// <summary>
        /// Called by DeadState when an AI dies.  The AI will be removed from
        /// the alive set and optionally respawned after respawnDelay.
        /// </summary>
        /// <param name="networkObjectId">The NetworkObjectId of the AI that died.</param>
        public void HandleAIDeath(ulong networkObjectId)
        {
            if (!IsServer) return;
            _aliveAI.Remove(networkObjectId);
            if (allowRespawn && aiPrefab != null)
            {
                // Choose a spawn point.  Round‑robin through the list.
                int idx = _aliveAI.Count % spawnPoints.Count;
                Transform spawnPoint = spawnPoints[idx];
                StartCoroutine(RespawnCoroutine(spawnPoint, respawnDelay));
            }
        }

        private IEnumerator RespawnCoroutine(Transform spawnPoint, float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnAIAt(spawnPoint);
        }
    }
}