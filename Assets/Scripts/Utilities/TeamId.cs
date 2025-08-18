using UnityEngine;
using Unity.Netcode;

namespace MemeArena.Network
{
    /// <summary>
    /// Identifies the team of a GameObject.  This component can be attached
    /// to both players and AI and is used by combat logic to prevent
    /// friendly fire.  The team value is assigned in the inspector and
    /// replicated to clients via a NetworkVariable for remote lookup.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public class TeamId : NetworkBehaviour
    {
        [Tooltip("Team identifier.  Assign different values to different teams (e.g., 0 and 1).")]
        public int team = 0; // Inspector default; kept in sync with network var for compatibility

        private NetworkVariable<int> _team = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _team.OnValueChanged += OnTeamChanged;
            if (IsServer)
            {
                // Initialize from inspector value on server; replicates to clients
                _team.Value = team;
            }
            else
            {
                // For clients, reflect current replicated value if already available
                team = _team.Value;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _team.OnValueChanged -= OnTeamChanged;
        }

        private void OnTeamChanged(int previous, int current)
        {
            // Keep legacy field in sync so existing reads of `.team` stay correct
            team = current;
        }

        /// <summary>Server-only setter for dynamic team assignment.</summary>
        public void SetTeamServer(int newTeam)
        {
            if (!IsServer) return;
            _team.Value = newTeam;
        }
    }
}