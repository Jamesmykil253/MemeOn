using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Networking
{
    /// <summary>
    /// Simple authoritative spawner for AI sentinels.
    /// </summary>
    public class AISpawnerManager : NetworkBehaviour
    {
        public GameObject aiPrefab;
        public int count = 1;
        public Vector3 areaSize = new Vector3(10, 0, 10);

        public override void OnNetworkSpawn()
        {
            if (IsServer) SpawnAll();
        }

        private void SpawnAll()
        {
            if (!aiPrefab) { Debug.LogWarning("AISpawnerManager missing aiPrefab."); return; }

            for (int i = 0; i < count; i++)
            {
                var offset = new Vector3(
                    Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                    0f,
                    Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f)
                );
                var pos = transform.position + offset;
                var go = Instantiate(aiPrefab, pos, Quaternion.identity);
                var no = go.GetComponent<NetworkObject>();
                if (!no) no = go.AddComponent<NetworkObject>();
                no.Spawn(true);
            }
        }
    }
}
