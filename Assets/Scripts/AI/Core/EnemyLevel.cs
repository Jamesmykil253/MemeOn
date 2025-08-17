using System;
using Unity.Netcode;
using UnityEngine;
using MemeArena.Stats;

namespace MemeArena.AI
{
    /// <summary>
    /// Minimal level provider for enemies. Server controls the level value; clients observe for UI.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class EnemyLevel : NetworkBehaviour, ILevelProvider
    {
        private NetworkVariable<int> _level = new NetworkVariable<int>(1);

        public int Level => _level.Value;
        public event Action<int> OnLevelChanged;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _level.Value = Mathf.Max(1, _level.Value);
            }
            _level.OnValueChanged += HandleLevelChanged;
            OnLevelChanged?.Invoke(_level.Value);
        }

    public override void OnDestroy()
        {
            base.OnDestroy();
            _level.OnValueChanged -= HandleLevelChanged;
        }

        private void HandleLevelChanged(int prev, int cur)
        {
            OnLevelChanged?.Invoke(cur);
        }

        /// <summary>Server only: set enemy level.</summary>
        public void SetLevel(int level)
        {
            if (!IsServer) return;
            _level.Value = Mathf.Max(1, level);
        }
    }
}
