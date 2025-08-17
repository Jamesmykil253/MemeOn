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
        public int team = 0;
    }
}