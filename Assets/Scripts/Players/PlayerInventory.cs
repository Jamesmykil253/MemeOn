using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Players
{
    /// <summary>
    /// Holds the player's coin count (server-authoritative).
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerInventory : NetworkBehaviour
    {
        public NetworkVariable<int> Coins =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [Server] public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Coins.Value += amount;
        }

        /// <summary> Removes all coins and returns how many were removed. </summary>
        [Server] public int DepositAll()
        {
            int c = Coins.Value;
            Coins.Value = 0;
            return c;
        }
    }
}
