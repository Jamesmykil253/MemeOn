using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace MemeArena.Utilities
{
    /// <summary>
    /// Simple runtime checks to reduce manual setup: warns when key components
    /// are missing and auto-wires obvious references.
    /// </summary>
    public class RuntimeBootstrapValidator : MonoBehaviour
    {
        [Header("Checks")] public bool checkNetworkManager = true;
        public bool checkPlayerPrefab = true;
        public bool checkCameraTarget = true;

        void Start()
        {
            var nm = NetworkManager.Singleton;
            if (checkNetworkManager && !nm)
            {
                Debug.LogError("RuntimeBootstrapValidator: NetworkManager.Singleton not found. Add NetworkManager + UnityTransport in Bootstrap scene.");
            }
            if (nm)
            {
                if (checkPlayerPrefab && nm.NetworkConfig.PlayerPrefab == null)
                {
                    Debug.LogWarning("RuntimeBootstrapValidator: Player Prefab is not assigned in NetworkManager. Auto-assignment may occur via NetworkPrefabsRegistrar if configured.");
                }
            }

            if (checkCameraTarget)
            {
                var cam = Camera.main;
                if (cam)
                {
                    var bootstrap = cam.GetComponent<MemeArena.CameraSystem.CameraBootstrap>();
                    if (!bootstrap)
                    {
                        bootstrap = cam.gameObject.AddComponent<MemeArena.CameraSystem.CameraBootstrap>();
                        Debug.Log("RuntimeBootstrapValidator: Added CameraBootstrap to Main Camera.");
                    }

                    // Disable TMP Example CameraController which uses legacy Input
                    var tmpCamCtrl = cam.GetComponent("TMPro.Examples.CameraController");
                    if (tmpCamCtrl != null)
                    {
                        var comp = tmpCamCtrl as Behaviour;
                        if (comp != null && comp.enabled)
                        {
                            comp.enabled = false;
                            Debug.LogWarning("RuntimeBootstrapValidator: Disabled TextMeshPro Examples CameraController (legacy Input) to prevent Input System exceptions.");
                        }
                    }
                }
            }

            // Warn if multiple scenes are loaded (can cause duplicate objects if both Bootstrap and Gameplay are open additively)
            if (SceneManager.sceneCount > 1)
            {
                Debug.LogWarning($"RuntimeBootstrapValidator: {SceneManager.sceneCount} scenes loaded. For NGO flow, keep only Bootstrap loaded; it will load gameplay over the network.");
            }

            // Enforce single AudioListener: keep main camera's, disable others
            var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (listeners != null && listeners.Length > 1)
            {
                var main = Camera.main != null ? Camera.main.GetComponent<AudioListener>() : null;
                foreach (var l in listeners)
                {
                    if (main != null && l == main) continue;
                    if (l.isActiveAndEnabled)
                    {
                        l.enabled = false;
                    }
                }
                Debug.LogWarning("RuntimeBootstrapValidator: Disabled extra AudioListeners to ensure only one is active.");
            }

            // Check for PlayerMovement objects that are not marked as PlayerObject (usually scene-placed players)
            var movers = FindObjectsByType<MemeArena.Players.PlayerMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int nonPlayerObjects = 0;
            foreach (var mv in movers)
            {
                var no = mv.GetComponent<NetworkObject>();
                if (no != null && !no.IsPlayerObject) nonPlayerObjects++;
            }
            if (nonPlayerObjects > 0)
            {
                Debug.LogWarning($"RuntimeBootstrapValidator: Found {nonPlayerObjects} PlayerMovement instance(s) that are not PlayerObject. Remove scene-placed players and rely on NetworkManager Player Prefab to avoid duplicates.");
            }
        }
    }
}
