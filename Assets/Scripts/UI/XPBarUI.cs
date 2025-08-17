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

        [Tooltip("Base XP required for level 2.  Should match PlayerStats.baseXPForLevel.")]
        [SerializeField] private int baseXPForLevel = 10;

        private void Start()
        {
            if (playerStats == null)
            {
                playerStats = GetComponentInParent<PlayerStats>();
            }
        }

        private void Update()
        {
            if (fillImage == null || playerStats == null) return;
            int level = playerStats.Level;
            int xp = playerStats.CurrentXP;
            int threshold = baseXPForLevel * level;
            float ratio = threshold > 0 ? (float)xp / threshold : 0f;
            fillImage.fillAmount = Mathf.Clamp01(ratio);
        }
    }
}