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

        void Start()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) return;
            if (nm.IsServer)
            {
                if (!string.IsNullOrEmpty(gameplaySceneName))
                {
                    nm.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
            }
        }
    }
}
