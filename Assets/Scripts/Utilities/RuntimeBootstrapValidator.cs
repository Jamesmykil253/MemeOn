using UnityEngine;
using Unity.Netcode;

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
                }
            }
        }
    }
}
