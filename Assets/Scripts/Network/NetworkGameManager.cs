using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Networking
{
    public class NetworkGameManager : MonoBehaviour
    {
        [Tooltip("Optional: Prefabs to auto-register with NetworkManager at runtime.")]
        public NetworkPrefabsRegistrar registrar;

        private void Start()
        {
            if (registrar) registrar.RegisterAll();
        }
    }
}
