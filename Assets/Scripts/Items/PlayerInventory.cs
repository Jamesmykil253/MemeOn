using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Items
{
    /// <summary>
    /// Networked player inventory to track collected coins.
    /// Coins are stored as a NetworkVariable so that HUD elements can display
    /// the value on clients.  The server has authority over the value.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerInventory : NetworkBehaviour
    {
        /// <summary>
        /// Number of coins currently held by the player.  Synced to clients.
        /// </summary>
        private NetworkVariable<int> _coinCount = new NetworkVariable<int>(0);

        /// <summary>
        /// Public getter for the coin count on clients.  Do not modify this
        /// value directly on clients; only the server can change it.
        /// </summary>
        public int CoinCount => _coinCount.Value;

        /// <summary>
        /// Add one coin to the player's inventory.  Only callable on the server.
        /// </summary>
        public void AddCoin()
        {
            if (!IsServer) return;
            _coinCount.Value++;
        }

        /// <summary>
        /// Remove all coins from the player's inventory and return the number removed.
        /// Only callable on the server.  Used by deposit zones to bank coins.
        /// </summary>
        public int WithdrawCoins()
        {
            if (!IsServer) return 0;
            int amount = _coinCount.Value;
            _coinCount.Value = 0;
            return amount;
        }
    }
}