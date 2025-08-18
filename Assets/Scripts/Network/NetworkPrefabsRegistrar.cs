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
    [Tooltip("Explicit list of networked prefabs to register.")]
    public List<GameObject> networkPrefabs = new();

    [Header("Auto-load (optional)")]
    [Tooltip("If true and the explicit list is empty, attempts to load prefabs from a Resources folder.")]
    public bool autoLoadFromResources = true;
    [Tooltip("Resources folder path to load from (e.g., Resources/NetworkPrefabs → 'NetworkPrefabs').")]
    public string resourcesFolder = "NetworkPrefabs";
    [Tooltip("If NetworkManager has no Player Prefab assigned, attempt to set it to the first prefab containing PlayerMovement.")]
    public bool autoAssignPlayerPrefab = true;

        public void RegisterAll()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) { Debug.LogWarning("No NetworkManager.Singleton found for registrar."); return; }
            var list = networkPrefabs;
            // Fallback: load from Resources if requested and explicit list is empty
            if (autoLoadFromResources && (list == null || list.Count == 0))
            {
                var loaded = Resources.LoadAll<GameObject>(resourcesFolder);
                if (loaded != null && loaded.Length > 0)
                {
                    list = new List<GameObject>(loaded);
                    if (Application.isEditor)
                        Debug.Log($"NetworkPrefabsRegistrar: Loaded {list.Count} prefabs from Resources/{resourcesFolder}.");
                }
            }
            foreach (var prefab in list)
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

            // If Player Prefab is unassigned, try to pick one that has PlayerMovement
            if (autoAssignPlayerPrefab && nm.NetworkConfig.PlayerPrefab == null)
            {
                try
                {
                    foreach (var entry in nm.NetworkConfig.Prefabs.Prefabs)
                    {
                        var go = entry.Prefab;
                        if (!go) continue;
                        if (go.GetComponent<MemeArena.Players.PlayerMovement>() != null)
                        {
                            nm.NetworkConfig.PlayerPrefab = go;
                            Debug.Log($"NetworkPrefabsRegistrar: Auto-assigned Player Prefab → {go.name}");
                            break;
                        }
                    }
                }
                catch { /* ignore reflection issues if any */ }
            }
        }
    }
}
