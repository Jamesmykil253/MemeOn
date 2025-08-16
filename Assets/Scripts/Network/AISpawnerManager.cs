using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace MemeArena.Networking
{
    /// <summary>
    /// AISpawnerManager is responsible for instantiating AI prefabs at designated spawn
    /// points and managing their lifecycle. It runs exclusively on the server and
    /// communicates with AIController instances to handle death and respawn.
    /// </summary>
    [DisallowMultipleComponent]
    public class AISpawnerManager : NetworkBehaviour
    {
        [Tooltip("AI prefab to spawn. Must contain a NetworkObject and AIController.")]
        public GameObject aiPrefab;
        [Tooltip("List of spawn locations.")]
        public List<Transform> spawnPoints = new List<Transform>();
        [Tooltip("Should AI respawn after death?")]
        public bool allowRespawn = true;
        [Tooltip("Delay in seconds before AI respawns.")]
        public float respawnDelay = ProjectConstants.Game.RespawnDelaySeconds;

        // Mapping from AI network object id to spawn point index.
        private readonly Dictionary<ulong, int> _aiToSpawnIndex = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                SpawnAll();
            }
        }

        /// <summary>
        /// Spawns AI at all configured spawn points. Called on server at startup.
        /// </summary>
        private void SpawnAll()
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                SpawnAI(i);
            }
        }

        /// <summary>
        /// Spawns an AI at the specified spawn point index. The prefab must contain
        /// a NetworkObject and will be spawned on the server.
        /// </summary>
        /// <param name="index">Index into the spawnPoints list.</param>
        private void SpawnAI(int index)
        {
            if (aiPrefab == null || index < 0 || index >= spawnPoints.Count)
                return;
            Transform point = spawnPoints[index];
            GameObject instance = Instantiate(aiPrefab, point.position, point.rotation);
            NetworkObject netObj = instance.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
                _aiToSpawnIndex[netObj.NetworkObjectId] = index;
            }
        }

        /// <summary>
        /// Handles death of an AI entity by scheduling its respawn if allowed. AIController
        /// calls this method when entering DeadState.
        /// </summary>
        /// <param name="networkObjectId">The id of the AI's NetworkObject.</param>
        public void HandleAIDeath(ulong networkObjectId)
        {
            if (!IsServer || !allowRespawn) return;
            if (!_aiToSpawnIndex.TryGetValue(networkObjectId, out int index)) return;
            _aiToSpawnIndex.Remove(networkObjectId);
            StartCoroutine(RespawnCoroutine(index));
        }

        private IEnumerator RespawnCoroutine(int index)
        {
            yield return new WaitForSeconds(respawnDelay);
            SpawnAI(index);
        }

        /// <summary>
        /// Registers a new spawn point during runtime. This can be used for dynamic
        /// spawning systems.
        /// </summary>
        public void RegisterSpawnPoint(Transform t)
        {
            if (!spawnPoints.Contains(t))
            {
                spawnPoints.Add(t);
            }
        }
    }
}