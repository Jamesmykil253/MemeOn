using System;

namespace MemeArena.Stats
{
    /// <summary>
    /// Minimal interface for components that expose a level value and change notifications.
    /// Implemented by PlayerStats and EnemyLevel.
    /// </summary>
    public interface ILevelProvider
    {
        int Level { get; }
        event Action<int> OnLevelChanged;
    }
}
