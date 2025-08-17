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

        private void Start()
        {
            // If no health is assigned, attempt to find one on the same or parent GameObject.
            if (health == null)
            {
                health = GetComponentInParent<NetworkHealth>();
            }
        }

        private void Update()
        {
            if (health == null || fillImage == null) return;
            if (health.maxHealth <= 0) return;
            float ratio = (float)health.GetCurrentHealth() / health.maxHealth;
            fillImage.fillAmount = Mathf.Clamp01(ratio);
        }
    }
}