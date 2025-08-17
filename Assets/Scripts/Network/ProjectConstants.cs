using UnityEngine;

namespace MemeArena.Network
{
    /// <summary>
    /// Namespaced project constants for the MemeArena network system.
    /// This provides the actual implementation that is referenced by the global
    /// ProjectConstants class to avoid circular dependencies.
    /// </summary>
    public static class ProjectConstants
    {
        public static class Match
        {
            /// <summary>
            /// Length of a match in seconds. Five minutes by default.
            /// </summary>
            public const float MatchLength = 300f;

            /// <summary>
            /// Minimum time between coin deposit actions in seconds.
            /// </summary>
            public const float DepositCooldown = 1f;

            /// <summary>
            /// Time before an AI respawns after death. In seconds.
            /// </summary>
            public const float RespawnDelay = 5f;
        }

        public static class AI
        {
            /// <summary>
            /// How often the finite state machine ticks, in updates per second.
            /// </summary>
            public const int AITickRate = 10;

            /// <summary>
            /// How often the behaviour tree runs, in updates per second.
            /// </summary>
            public const int BehaviorTickRate = 5;
        }

        public static class Tags
        {
            public const string Player = "Player";
            public const string AI = "AI";
            public const string Projectile = "Projectile";
        }

        public static class Layers
        {
            public const int Default = 0;
            public const int Environment = 8;
            public const int Player = 9;
            public const int AI = 10;
            public const int Projectile = 11;
        }
    }
}