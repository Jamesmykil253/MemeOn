using UnityEngine;
using TMPro;
using MemeArena.Players;

namespace MemeArena.UI
{
    /// <summary>
    /// Displays the player's current level as a numeric label.  Reads the
    /// value from a PlayerStats component.
    /// </summary>
    public class LevelUI : MonoBehaviour
    {
        [Tooltip("Text component used to display the player level.")]
        [SerializeField] private TMP_Text levelText;

        [Tooltip("PlayerStats component providing the level value.")]
        [SerializeField] private PlayerStats playerStats;

        private void Start()
        {
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<PlayerStats>();
            }
        }

        private void Update()
        {
            if (levelText == null || playerStats == null) return;
            levelText.text = playerStats.Level.ToString();
        }
    }
}