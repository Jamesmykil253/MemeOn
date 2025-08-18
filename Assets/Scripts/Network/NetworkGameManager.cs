using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Networking
{
    public class NetworkGameManager : MonoBehaviour
    {
        [Tooltip("Optional: Prefabs to auto-register with NetworkManager at runtime.")]
        public NetworkPrefabsRegistrar registrar;

        [Header("Auto Start (Editor/dev)")]
        [Tooltip("Start transport when entering Play Mode. Off in builds by default.")]
        public bool autoStartInEditor = false;
        public enum StartMode { None, Host, Server, Client }
        public StartMode startMode = StartMode.None;

        private void Start()
        {
            if (registrar) registrar.RegisterAll();
            // Optional: auto start network session (dev convenience)
            if (autoStartInEditor && Application.isEditor)
            {
                var nm = NetworkManager.Singleton;
                if (!nm) return;
                switch (startMode)
                {
                    case StartMode.Host:
                        nm.StartHost();
                        break;
                    case StartMode.Server:
                        nm.StartServer();
                        break;
                    case StartMode.Client:
                        nm.StartClient();
                        break;
                }
            }
        }
    }
}
