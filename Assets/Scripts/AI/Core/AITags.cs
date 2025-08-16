using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Defines common tag strings used by the AI system.  Storing them in a
    /// centralised class avoids typos in string literals throughout the codebase.
    /// </summary>
    public static class AITags
    {
        /// <summary>
        /// Tag used to identify player-controlled characters.  Assign this tag to
        /// all player prefabs in the Unity Inspector.
        /// </summary>
        public const string PlayerTag = "Player";

        /// <summary>
        /// Tag used to identify AI enemies.  Assign this tag to AI prefabs.
        /// </summary>
        public const string EnemyTag = "Enemy";
    }
}