using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MemeArena.Networking
{
    // Call this on the server/host to switch scenes reliably across all clients
    public class ServerSceneLoader : NetworkBehaviour
    {
        [Tooltip("Name of the next scene to load via NGO scene management.")]
        public string nextSceneName;

        [ContextMenu("Load Next Scene (Server)")]
        public void LoadNext()
        {
            if (!IsServer)
            {
                Debug.LogWarning("ServerSceneLoader: LoadNext called on non-server.");
                return;
            }
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("ServerSceneLoader: nextSceneName is empty.");
                return;
            }
            NetworkManager.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
}
