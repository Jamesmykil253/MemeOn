using UnityEngine;
using UnityEngine.UI;
using MemeArena.Combat;

namespace MemeArena.UI
{
    /// <summary>
    /// Displays the player's current health as a filled image.  This script
    /// expects a reference to an Image component and a NetworkHealth
    /// component on the same or parent object.  The fill amount is updated
    /// every frame based on the current health over max health.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Tooltip("UI Image whose fillAmount will be set based on current health.")]
        [SerializeField] private Image fillImage;

        [Tooltip("NetworkHealth component providing current and maximum health.")]
        [SerializeField] private NetworkHealth health;

        public void SetSource(NetworkHealth source)
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }
            health = source;
            if (isActiveAndEnabled && health != null)
            {
                health.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(health.GetCurrentHealth(), health.maxHealth);
            }
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponentInParent<NetworkHealth>();
            }
            if (health != null)
            {
                health.OnHealthChanged += HandleHealthChanged;
                // Push initial value
                HandleHealthChanged(health.GetCurrentHealth(), health.maxHealth);
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (fillImage == null || max <= 0) return;
            float ratio = (float)current / max;
            fillImage.fillAmount = Mathf.Clamp01(ratio);
        }
    }
}