using UnityEngine;
using TMPro;
using MemeArena.Items;

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

        private void Start()
        {
            if (inventory == null)
            {
                inventory = GetComponentInParent<PlayerInventory>();
            }
        }

        private void Update()
        {
            if (coinText == null || inventory == null) return;
            coinText.text = inventory.CoinCount.ToString();
        }
    }
}