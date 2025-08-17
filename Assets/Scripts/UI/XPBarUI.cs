using UnityEngine;
using UnityEngine.UI;
using MemeArena.Players;

namespace MemeArena.UI
{
    /// <summary>
    /// Displays the player's progress towards the next level as a filled image.
    /// It reads the current level and XP from PlayerStats and computes the
    /// proportion of XP earned towards the next level.  The XP threshold is
    /// assumed to be baseXPForLevel * current level (matching the PlayerStats logic).
    /// </summary>
    public class XPBarUI : MonoBehaviour
    {
        [Tooltip("UI Image whose fillAmount reflects XP progress.")]
        [SerializeField] private Image fillImage;

        [Tooltip("PlayerStats component providing XP and level values.")]
        [SerializeField] private PlayerStats playerStats;
        public void SetSource(PlayerStats stats)
        {
            if (playerStats != null)
            {
                playerStats.OnXPChanged -= HandleXPChanged;
            }
            playerStats = stats;
            if (isActiveAndEnabled && playerStats != null)
            {
                playerStats.OnXPChanged += HandleXPChanged;
                HandleXPChanged(playerStats.CurrentXP, playerStats.Level, baseXPForLevel * playerStats.Level);
            }
        }

        [Tooltip("Base XP required for level 2.  Should match PlayerStats.baseXPForLevel.")]
        [SerializeField] private int baseXPForLevel = 10;

        private void OnEnable()
        {
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<PlayerStats>();
            }
            if (playerStats != null)
            {
                playerStats.OnXPChanged += HandleXPChanged;
                HandleXPChanged(playerStats.CurrentXP, playerStats.Level, baseXPForLevel * playerStats.Level);
            }
        }

        private void OnDisable()
        {
            if (playerStats != null)
            {
                playerStats.OnXPChanged -= HandleXPChanged;
            }
        }

        private void HandleXPChanged(int currentXP, int level, int threshold)
        {
            if (fillImage == null) return;
            float ratio = threshold > 0 ? (float)currentXP / threshold : 0f;
            fillImage.fillAmount = Mathf.Clamp01(ratio);
        }
    }
}