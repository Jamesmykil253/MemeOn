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

        public void SetSource(BoostedAttackTracker tracker)
        {
            if (boostedTracker != null)
            {
                boostedTracker.OnBoostChanged -= HandleBoostChanged;
            }
            boostedTracker = tracker;
            if (isActiveAndEnabled && boostedTracker != null)
            {
                boostedTracker.OnBoostChanged += HandleBoostChanged;
                HandleBoostChanged(boostedTracker.IsBoosted);
            }
        }

        private void OnEnable()
        {
            if (boostedTracker == null)
            {
                boostedTracker = GetComponentInParent<BoostedAttackTracker>();
            }
            if (boostedTracker != null)
            {
                boostedTracker.OnBoostChanged += HandleBoostChanged;
                HandleBoostChanged(boostedTracker.IsBoosted);
            }
        }

        private void OnDisable()
        {
            if (boostedTracker != null)
            {
                boostedTracker.OnBoostChanged -= HandleBoostChanged;
            }
        }

        private void HandleBoostChanged(bool isBoosted)
        {
            if (icon == null) return;
            icon.enabled = isBoosted;
        }
    }
}