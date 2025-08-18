using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MemeArena.Combat;
using MemeArena.Stats;

namespace MemeArena.UI
{
    /// <summary>
    /// World-space enemy UI: displays health bar and level text over an enemy.
    /// Attach to a world-space Canvas that's a child of the enemy.
    /// </summary>
    [System.Obsolete("Replaced by EnemyUnitUI + HealthBarUI/LevelUI modular setup.")]
    [AddComponentMenu("")]
    public class EnemyWorldUI : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text levelText;

        [Header("Sources (auto-find if null)")]
        [SerializeField] private NetworkHealth health;
        [SerializeField] private MonoBehaviour levelProviderBehaviour; // must implement ILevelProvider
        private ILevelProvider levelProvider;

        private void OnEnable()
        {
            if (health == null) health = GetComponentInParent<NetworkHealth>();
            if (levelProviderBehaviour == null)
            {
                // Search all parents for a component implementing ILevelProvider
                var parents = GetComponentsInParent<MonoBehaviour>(true);
                foreach (var mb in parents)
                {
                    if (mb is ILevelProvider lp)
                    {
                        levelProviderBehaviour = mb;
                        levelProvider = lp;
                        break;
                    }
                }
            }
            else
            {
                levelProvider = levelProviderBehaviour as ILevelProvider;
            }

            if (health != null)
            {
                health.OnHealthChanged += OnHealthChanged;
                OnHealthChanged(health.GetCurrentHealth(), health.maxHealth);
            }
            if (levelProvider != null)
            {
                levelProvider.OnLevelChanged += OnLevelChanged;
                OnLevelChanged(levelProvider.Level);
            }
        }

        private void OnDisable()
        {
            if (health != null) health.OnHealthChanged -= OnHealthChanged;
            if (levelProvider != null) levelProvider.OnLevelChanged -= OnLevelChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            if (healthFill == null || max <= 0) return;
            healthFill.fillAmount = Mathf.Clamp01((float)current / max);
        }

        private void OnLevelChanged(int level)
        {
            if (levelText == null) return;
            levelText.text = $"Lv {level}";
        }
    }
}
