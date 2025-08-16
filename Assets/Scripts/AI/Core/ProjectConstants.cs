using UnityEngine;

namespace MemeArena
{
    /// <summary>
    /// Project-wide constants for event names, blackboard keys, and other static values.
    /// Centralising these values prevents drift between files and and ensure spec compliance.
    /// </summary>
    public static class ProjectConstants
    {
        // Combat event names used across AI and combat systems.
        public const string OnDamageReceived = "OnDamageReceived";
        public const string OnSuccessfulHit = "OnSuccessfulHit";
        public const string OnFailedHit = "OnFailedHit";

        /// <summary>
        /// Blackboard key strings used by the AI system. Avoid string literals sprinkled
        /// throughout code – reference these instead to catch typos at compile time.
        /// </summary>
        public static class BlackboardKeys
        {
            public const string SpawnPosition = "spawnPosition";
            public const string TargetId = "targetId";
            public const string Aggroed = "aggroed";
            public const string LastHitTimestamp = "lastHitTimestamp";
            public const string TimeSinceLastSuccessfulHit = "timeSinceLastSuccessfulHit";
            public const string LastKnownTargetPos = "lastKnownTargetPos";
        }

        /// <summary>
        /// Match level constants.
        /// </summary>
        public static class Game
        {
            // Length of a standard 3v3 match in seconds.
            public const float MatchLengthSeconds = 300f;
            // Minimum time in seconds between scoring deposits.
            public const float DepositThrottleSeconds = 0.5f;
            // Time before an AI character respawns after death.
            public const float RespawnDelaySeconds = 3f;
        }

        /// <summary>
        /// AI simulation rates. Tick rates should match those described in the master spec.
        /// </summary>
        public static class AI
        {
            // Number of low-level state machine ticks per second.
            public const float AITickRate = 10f;
            // Frequency of high-level behaviour tree evaluation, in ticks per second.
            public const float BehaviorTickRate = 5f;
        }
    }
}