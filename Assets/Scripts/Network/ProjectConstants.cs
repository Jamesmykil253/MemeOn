using UnityEngine;

namespace MemeArena.Network
{
    /// <summary>
    /// Centralised project constants used throughout the AI, combat and network systems.  
    /// This file defines match settings, tick rates, tags and layer indices.  
    /// All code should reference these fields rather than hard‑coded numbers to ensure 
    /// consistency with design documents.
    /// </summary>
    public static class ProjectConstants
    {
        public static class Match
        {
            /// <summary>
            /// Length of a match in seconds.  Five minutes (300 seconds) by default.
            /// </summary>
            public const float MatchLength = 300f;

            /// <summary>
            /// Minimum time between coin deposit actions in seconds.  Prevents spamming the deposit action.
            /// </summary>
            public const float DepositCooldown = 0.5f;

            /// <summary>
            /// Time before an AI respawns after death.  In seconds.
            /// </summary>
            public const float RespawnDelay = 3f;
        }

        public static class AI
        {
            /// <summary>
            /// How often the finite state machine ticks, in updates per second.  
            /// A value of 10 means the AI will update every 0.1 seconds.
            /// </summary>
            public const int AITickRate = 10;

            /// <summary>
            /// How often the behaviour tree runs, in updates per second.  
            /// A value of 5 means the behaviour tree will tick every 0.2 seconds.
            /// </summary>
            public const int BehaviorTickRate = 5;
        }

        public static class Tags
        {
            /// <summary>
            /// Tag assigned to player controlled characters.  AI uses this tag to look up targets.
            /// </summary>
            public const string Player = "Player";

            /// <summary>
            /// Tag assigned to AI controlled characters.
            /// </summary>
            public const string AI = "AI";

            /// <summary>
            /// Tag assigned to projectile prefabs.
            /// </summary>
            public const string Projectile = "Projectile";
        }

        public static class Layers
        {
            // Unity layer indices used by physics and line of sight checks.  These values should
            // correspond to your project's layer manager settings.  Adjust as needed.
            public const int Default = 0;
            // Align with TagManager: Environment=3, Player=6, Enemy=7, AI=8, Projectiles=9
            public const int Environment = 3;
            public const int Player = 6;
            public const int Enemy = 7;
            public const int AI = 8;
            public const int Projectile = 9;
        }
    }
}
