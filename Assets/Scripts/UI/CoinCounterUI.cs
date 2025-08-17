using UnityEngine;
using TMPro;
using MemeArena.Players;

namespace MemeArena.UI
{
    /// <summary>
    /// Displays the number of coins held by the local player.  This script
    /// reads the coin count from a PlayerInventory component and updates
    /// a TextMeshPro label accordingly.
    /// </summary>
    public class CoinCounterUI : MonoBehaviour
    {
        [Tooltip("Text component used to display the coin count.")]
        [SerializeField] private TMP_Text coinText;

        [Tooltip("PlayerInventory component to read the coin count from.")]
    [SerializeField] private PlayerInventory inventory;

        public void SetSource(PlayerInventory inv)
        {
            if (inventory != null)
            {
                inventory.OnCoinsChanged -= HandleCoinsChanged;
            }
            inventory = inv;
            if (isActiveAndEnabled && inventory != null)
            {
                inventory.OnCoinsChanged += HandleCoinsChanged;
                HandleCoinsChanged(inventory.Coins.Value);
            }
        }

        private void OnEnable()
        {
            if (inventory == null)
            {
                inventory = GetComponentInParent<PlayerInventory>();
            }
            if (inventory != null)
            {
                inventory.OnCoinsChanged += HandleCoinsChanged;
                HandleCoinsChanged(inventory.Coins.Value);
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnCoinsChanged -= HandleCoinsChanged;
            }
        }

        private void HandleCoinsChanged(int newCoins)
        {
            if (coinText == null) return;
            coinText.text = newCoins.ToString();
        }
    }
}