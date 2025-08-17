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

        public void SetSource(PlayerStats stats)
        {
            if (playerStats != null)
            {
                playerStats.OnLevelChanged -= HandleLevelChanged;
            }
            playerStats = stats;
            if (isActiveAndEnabled && playerStats != null)
            {
                playerStats.OnLevelChanged += HandleLevelChanged;
                HandleLevelChanged(playerStats.Level);
            }
        }

        private void OnEnable()
        {
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<PlayerStats>();
            }
            if (playerStats != null)
            {
                playerStats.OnLevelChanged += HandleLevelChanged;
                HandleLevelChanged(playerStats.Level);
            }
        }

        private void OnDisable()
        {
            if (playerStats != null)
            {
                playerStats.OnLevelChanged -= HandleLevelChanged;
            }
        }

        private void HandleLevelChanged(int newLevel)
        {
            if (levelText == null) return;
            levelText.text = newLevel.ToString();
        }
    }
}