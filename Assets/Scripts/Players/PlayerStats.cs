using System;
using Unity.Netcode;
using UnityEngine;
using MemeArena.Stats;

namespace MemeArena.Players
{
    /// <summary>
    /// Tracks the player's experience and level.  Experience and level are
    /// synchronised across the network so that the HUD can display the
    /// current values on clients.  Levels and experience thresholds can
    /// be tuned via the inspector.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerStats : NetworkBehaviour, ILevelProvider
    {
        [Header("Level Settings")]
        [Tooltip("The amount of XP required to reach level 2.  Each subsequent level multiplies this amount by the current level.")]
        [SerializeField] private int baseXPForLevel = 10;

        // Current level of the player.  Starts at 1 and increments as XP is gained.
    private NetworkVariable<int> _level = new NetworkVariable<int>(1);

        // Current accumulated XP towards the next level.  Resets to zero upon levelling up.
    private NetworkVariable<int> _currentXP = new NetworkVariable<int>(0);

    public int Level => _level.Value;
    public int CurrentXP => _currentXP.Value;

    public event Action<int> OnLevelChanged;
    public event Action<int, int, int> OnXPChanged; // currentXP, level, threshold

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                // Initialise level and XP on the server to ensure deterministic values.
                _level.Value = 1;
                _currentXP.Value = 0;
            }
            // Subscribe to changes and push initial state
            _level.OnValueChanged += HandleLevelChanged;
            _currentXP.OnValueChanged += HandleXPChanged;
            OnLevelChanged?.Invoke(_level.Value);
            OnXPChanged?.Invoke(_currentXP.Value, _level.Value, XPThresholdForLevel(_level.Value));
        }

    public override void OnDestroy()
        {
            base.OnDestroy();
            _level.OnValueChanged -= HandleLevelChanged;
            _currentXP.OnValueChanged -= HandleXPChanged;
        }

        /// <summary>
        /// Award experience points to the player.  Only callable on the server.
        /// Automatically handles levelling up and carries remaining XP into the
        /// next level.
        /// </summary>
        /// <param name="amount">Amount of XP to add.</param>
        public void AddExperience(int amount)
        {
            if (!IsServer) return;
            if (amount <= 0) return;
            _currentXP.Value += amount;
            // Check for level up as long as we have enough XP.
            while (_currentXP.Value >= XPThresholdForLevel(_level.Value))
            {
                _currentXP.Value -= XPThresholdForLevel(_level.Value);
                _level.Value++;
                // Level up logic could be expanded here (e.g. increase stats).
            }
        }

        private void HandleLevelChanged(int previous, int current)
        {
            OnLevelChanged?.Invoke(current);
            // Threshold changed as well; notify XP listeners with same current xp
            OnXPChanged?.Invoke(_currentXP.Value, current, XPThresholdForLevel(current));
        }

        private void HandleXPChanged(int previous, int current)
        {
            OnXPChanged?.Invoke(current, _level.Value, XPThresholdForLevel(_level.Value));
        }

        /// <summary>
        /// Computes the XP threshold required to reach the next level.  The
        /// threshold grows linearly with the current level.
        /// </summary>
        private int XPThresholdForLevel(int level)
        {
            return baseXPForLevel * level;
        }
    }
}