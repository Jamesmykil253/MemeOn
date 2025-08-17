using UnityEngine;
using UnityEngine.UI;
using MemeArena.Combat;

namespace MemeArena.UI
{
    /// <summary>
    /// Displays an icon when the player's next basic attack is boosted.
    /// Reads the boosted state from a BoostedAttackTracker component.
    /// </summary>
    public class BoostedAttackUI : MonoBehaviour
    {
        [Tooltip("Image that will be enabled when the attack is boosted and disabled otherwise.")]
        [SerializeField] private Image icon;

        [Tooltip("BoostedAttackTracker component to check the boosted state.")]
        [SerializeField] private BoostedAttackTracker boostedTracker;

        private void Start()
        {
            if (boostedTracker == null)
            {
                boostedTracker = GetComponentInParent<BoostedAttackTracker>();
            }
        }

        private void Update()
        {
            if (icon == null || boostedTracker == null) return;
            icon.enabled = boostedTracker.IsBoosted;
        }
    }
}