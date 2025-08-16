// Meme Online Battle Arena - Team
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Team : NetworkBehaviour
{
    public NetworkVariable<byte> TeamIndex = new NetworkVariable<byte>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [ServerRpc(RequireOwnership = false)]
    public void SetTeamServerRpc(byte teamIndex)
    {
        if (!IsServer) return;
        TeamIndex.Value = teamIndex;
    }
}
