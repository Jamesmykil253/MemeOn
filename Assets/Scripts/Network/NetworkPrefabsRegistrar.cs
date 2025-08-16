using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Networking
{
    /// <summary>
    /// Ensures critical prefabs are present in the NetworkManager configuration.
    /// </summary>
    public class NetworkPrefabsRegistrar : MonoBehaviour
    {
        public List<GameObject> networkPrefabs = new();

        public void RegisterAll()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) { Debug.LogWarning("No NetworkManager.Singleton found for registrar."); return; }
            foreach (var prefab in networkPrefabs)
            {
                if (!prefab) continue;
                var found = false;
                foreach (var entry in nm.NetworkConfig.Prefabs.Prefabs)
                {
                    if (entry.Prefab == prefab) { found = true; break; }
                }
                if (!found)
                {
                    nm.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = prefab });
                }
            }
        }
    }
}
