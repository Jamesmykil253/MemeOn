using UnityEngine;
using TMPro;
using MemeArena.Stats;

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

        [Tooltip("Optional explicit component that implements ILevelProvider (e.g., PlayerStats, EnemyLevel).")]
        [SerializeField] private MonoBehaviour levelProviderComponent;

        private ILevelProvider _provider;

        public void SetSource(ILevelProvider provider)
        {
            Unsubscribe();
            _provider = provider;
            TrySubscribeAndPush();
        }

        private void OnEnable()
        {
            if (_provider == null)
            {
                if (levelProviderComponent != null && levelProviderComponent is ILevelProvider explicitProv)
                {
                    _provider = explicitProv;
                }
                else
                {
                    _provider = GetComponentInParent<ILevelProvider>();
                }
            }
            TrySubscribeAndPush();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void HandleLevelChanged(int newLevel)
        {
            if (levelText == null) return;
            levelText.text = newLevel.ToString();
        }

        private void TrySubscribeAndPush()
        {
            if (_provider == null) return;
            _provider.OnLevelChanged += HandleLevelChanged;
            HandleLevelChanged(_provider.Level);
        }

        private void Unsubscribe()
        {
            if (_provider != null)
            {
                _provider.OnLevelChanged -= HandleLevelChanged;
            }
        }
    }
}