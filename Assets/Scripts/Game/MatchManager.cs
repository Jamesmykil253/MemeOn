using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Game
{
    /// <summary>
    /// Tracks match timer and team scores. Single instance in scene.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class MatchManager : NetworkBehaviour
    {
        public static MatchManager Instance { get; private set; }

        [Header("Match")]
        public float matchLengthSeconds = 300f; // 5 minutes
        float _remaining;

        public NetworkVariable<int> Team0Score =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Team1Score =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        void Awake() { Instance = this; }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                _remaining = matchLengthSeconds;
        }

        void Update()
        {
            if (!IsServer) return;
            if (_remaining > 0f)
            {
                _remaining -= Time.deltaTime;
                if (_remaining <= 0f)
                {
                    // TODO: end match, show results
                }
            }
        }

        [Server] public void AddScore(int teamId, int amount)
        {
            if (amount <= 0) return;
            if (teamId == 0) Team0Score.Value += amount;
            else Team1Score.Value += amount;
        }
    }
}
