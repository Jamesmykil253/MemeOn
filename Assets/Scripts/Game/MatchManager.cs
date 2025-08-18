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
    bool _overtime;
    bool _ended;

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
            if (_ended) return;
            if (_remaining > 0f)
            {
                _remaining -= Time.deltaTime;
                if (_remaining <= 0f)
                {
                    // Time expired. If tie, enter overtime sudden death.
                    if (Team0Score.Value == Team1Score.Value)
                    {
                        _overtime = true;
                        // Optionally notify clients that overtime started
                        OvertimeStartedClientRpc();
                    }
                    else
                    {
                        EndMatch(Team0Score.Value > Team1Score.Value ? 0 : 1);
                    }
                }
            }
        }

        public void AddScore(int teamId, int amount)
        {
            if (!IsServer || amount <= 0) return;
            if (teamId == 0) Team0Score.Value += amount;
            else Team1Score.Value += amount;

            // In overtime, first score wins
            if (_overtime && !_ended)
            {
                int winner = Team0Score.Value > Team1Score.Value ? 0 : 1;
                EndMatch(winner);
            }
        }

        private void EndMatch(int winningTeam)
        {
            if (_ended) return;
            _ended = true;
            MatchEndedClientRpc(winningTeam, Team0Score.Value, Team1Score.Value);
            // Optionally perform server-side cleanup or transition.
        }

        [ClientRpc]
        private void OvertimeStartedClientRpc()
        {
            // Hook up UI feedback for overtime start (e.g., flash banner)
        }

        [ClientRpc]
        private void MatchEndedClientRpc(int winningTeam, int team0Score, int team1Score)
        {
            // Hook up UI to show results panel.
        }
    }
}
