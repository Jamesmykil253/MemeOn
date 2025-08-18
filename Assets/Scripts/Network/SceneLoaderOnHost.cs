using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Networking
{
    /// <summary>
    /// On start, if running as Host or Server, loads the specified gameplay scene
    /// using NGO scene management so clients synchronize automatically.
    /// Place this on the Bootstrap object.
    /// </summary>
    public class SceneLoaderOnHost : MonoBehaviour
    {
        [Tooltip("The gameplay scene name to load when Host/Server starts.")]
        public string gameplaySceneName = "Gameplay_01";
    // Guard to ensure we only attempt to load the gameplay scene once.
    private bool _hasLoadedScene = false;

        void Start()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) return;
            nm.OnServerStarted += HandleServerStarted;
            // If server already started (e.g., autoStart in another Start), load immediately
            if (nm.IsServer)
            {
                HandleServerStarted();
            }
        }

        void OnDestroy()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) return;
            nm.OnServerStarted -= HandleServerStarted;
        }

        private void HandleServerStarted()
        {
            var nm = NetworkManager.Singleton;
            if (!nm || !nm.IsServer) return;
            if (string.IsNullOrEmpty(gameplaySceneName)) return;
            // Don't attempt to load the same scene that is already active (prevents reload loops)
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (string.Equals(active.name, gameplaySceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"SceneLoaderOnHost: Active scene already '{gameplaySceneName}', skipping load.");
                _hasLoadedScene = true;
                return;
            }
            if (_hasLoadedScene)
            {
                Debug.Log("SceneLoaderOnHost: Gameplay scene already requested; skipping duplicate load.");
                return;
            }
            _hasLoadedScene = true;
            nm.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
