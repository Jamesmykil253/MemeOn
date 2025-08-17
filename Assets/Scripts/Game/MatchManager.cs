using Unity.Netcode;
using UnityEngine;
using MemeArena.Network;

namespace MemeArena.Game
{
    /// <summary>
    /// Centralised match management.  Tracks team scores and match timer and
    /// exposes read‑only access to clients.  Should be added to a single
    /// GameObject in the scene and registered as a network object.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class MatchManager : NetworkBehaviour
    {
        /// <summary>
        /// Singleton instance for easy access.  Do not rely on this on the
        /// server before OnNetworkSpawn; non‑server code should use it only for
        /// reading scores.
        /// </summary>
        public static MatchManager Instance { get; private set; }

        // Networked scores for the two teams.  Only the server may modify these.
        private NetworkVariable<int> _team0Score = new NetworkVariable<int>(0);
        private NetworkVariable<int> _team1Score = new NetworkVariable<int>(0);

        // Match timer (in seconds).  Only updated on the server.
        private float _matchTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                // Initialise the match timer using the project constant.  If the
                // constant is not defined, default to 300 seconds (5 minutes).
                _matchTimer = ProjectConstants.Match.MatchLength;
            }
        }

        private void Update()
        {
            // Only the server updates the match timer.  Clients simply read
            // the value when they need to display it.
            if (!IsServer) return;
            if (_matchTimer > 0f)
            {
                _matchTimer -= Time.deltaTime;
                if (_matchTimer <= 0f)
                {
                    _matchTimer = 0f;
                    EndMatch();
                }
            }
        }

        private void EndMatch()
        {
            // TODO: Implement match end behaviour (e.g., show results, stop
            // player actions, transition scenes).  This placeholder does
            // nothing for now.
        }

        /// <summary>
        /// Adds points to the specified team's score.  This method should be
        /// called only on the server.
        /// </summary>
        /// <param name="teamId">Team identifier (0 for team A, 1 for team B).</param>
        /// <param name="amount">Number of points to add.</param>
        public void AddScore(int teamId, int amount)
        {
            if (!IsServer) return;
            if (amount <= 0) return;
            if (teamId == 0)
            {
                _team0Score.Value += amount;
            }
            else if (teamId == 1)
            {
                _team1Score.Value += amount;
            }
        }

        /// <summary>
        /// Returns the current score for the specified team.  Can be called by
        /// clients to update their UI.
        /// </summary>
        public int GetScore(int teamId)
        {
            return teamId == 0 ? _team0Score.Value : _team1Score.Value;
        }

        /// <summary>
        /// Returns the amount of time remaining in the match, in seconds.
        /// Clients should poll this value to update the match timer UI.
        /// </summary>
        public float GetTimeRemaining()
        {
            return _matchTimer;
        }
    }
}