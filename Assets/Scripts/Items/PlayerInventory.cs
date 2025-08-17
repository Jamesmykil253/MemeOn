using Unity.Netcode;
using UnityEngine;

namespace MemeArena.Items
{
    /// <summary>
    /// Legacy inventory (deprecated). Kept only to avoid breaking old scenes.
    /// Prefer MemeArena.Players.PlayerInventory.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [System.Obsolete("Use MemeArena.Players.PlayerInventory instead.")]
    public class LegacyPlayerInventory : NetworkBehaviour
    {
        private NetworkVariable<int> _coinCount = new NetworkVariable<int>(0);
        public int CoinCount => _coinCount.Value;
        public void AddCoin() { if (IsServer) _coinCount.Value++; }
        public int WithdrawCoins()
        {
            if (!IsServer) return 0;
            int amount = _coinCount.Value;
            _coinCount.Value = 0;
            return amount;
        }
    }
}