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

        public event System.Action<int> OnCoinsChanged;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Coins.OnValueChanged += HandleCoinsChanged;
            OnCoinsChanged?.Invoke(Coins.Value);
        }

    public override void OnDestroy()
        {
            base.OnDestroy();
            Coins.OnValueChanged -= HandleCoinsChanged;
        }

        private void HandleCoinsChanged(int previous, int current)
        {
            OnCoinsChanged?.Invoke(current);
        }

        public void AddCoins(int amount)
        {
            if (!IsServer || amount <= 0) return;
            Coins.Value += amount;
        }

        /// <summary> Removes all coins and returns how many were removed. </summary>
        public int DepositAll()
        {
            if (!IsServer) return 0;
            int c = Coins.Value;
            Coins.Value = 0;
            return c;
        }
    }
}
