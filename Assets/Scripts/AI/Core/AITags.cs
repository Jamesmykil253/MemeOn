using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Centralized constants to keep terminology aligned across docs and code.
    /// </summary>
    public static class AITags
    {
        // Blackboard keys (names chosen to match design docs)
        public const string SpawnPosition = "spawnPosition";
        public const string TargetId = "targetId";
        public const string TimeSinceLastSuccessfulHit = "timeSinceLastSuccessfulHit";
        public const string LastKnownTargetPosition = "lastKnownTargetPosition";

        // Event keys
        public const string OnDamageReceived = "OnDamageReceived";
        public const string OnSuccessfulHit = "OnSuccessfulHit";
        public const string OnFailedHit = "OnFailedHit";

        // Layers / Tags
        public const string PlayerTag = "Player";
        public const string EnemyTag  = "Enemy";
        public const string ProjectileLayerName = "Projectile";
    }
}
