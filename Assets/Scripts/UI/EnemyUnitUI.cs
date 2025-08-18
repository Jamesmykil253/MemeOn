using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MemeArena.Combat;
using MemeArena.Stats;

namespace MemeArena.UI
{
    /// <summary>
    /// Helper that auto-wires an enemy world-space UI: health bar and level text.
    /// Place this on a world-space canvas under the enemy, assign references, or let it auto-find.
    /// </summary>
    public class EnemyUnitUI : MonoBehaviour
    {
        [Header("Bindings")]
        [SerializeField] private HealthBarUI healthBar;
        [SerializeField] private LevelUI levelUI;
        [SerializeField] private BillboardUI billboard; // optional

        [Header("Auto-find (optional)")]
        [SerializeField] private NetworkHealth healthSource;
        [SerializeField] private MonoBehaviour levelProviderComponent; // ILevelProvider

        private void Awake()
        {
            if (healthBar == null) healthBar = GetComponentInChildren<HealthBarUI>(true);
            if (levelUI == null) levelUI = GetComponentInChildren<LevelUI>(true);
            if (billboard == null) billboard = GetComponentInChildren<BillboardUI>(true);
        }

        private void OnEnable()
        {
            if (healthSource == null) healthSource = GetComponentInParent<NetworkHealth>();
            if (healthBar != null && healthSource != null)
            {
                healthBar.SetSource(healthSource);
            }

            ILevelProvider provider = null;
            if (levelProviderComponent is ILevelProvider explicitProv)
            {
                provider = explicitProv;
            }
            else
            {
                provider = GetComponentInParent<ILevelProvider>();
            }
            if (levelUI != null && provider != null)
            {
                levelUI.SetSource(provider);
            }
        }
    }
}
