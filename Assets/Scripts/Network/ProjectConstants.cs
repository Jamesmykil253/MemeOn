using UnityEngine;

namespace MemeArena.Network
{
    /// <summary>
    /// Centralized constants for gameplay and networking.  Keeping these values
    /// in a single location prevents drift between design documents and code.
    /// Update these values to tune match length, AI tick rates and other
    /// globally defined parameters.  All code referencing magic numbers should
    /// instead use fields from ProjectConstants.
    /// </summary>
    public static class ProjectConstants
    {
        public static class Match
        {
            /// <summary>
            /// Length of a match in seconds.  Five minutes by default.  This
            /// value is mirrored in the master specification.
            /// </summary>
            public const float MatchLength = 300f;

            /// <summary>
            /// Minimum time between coin deposit actions in seconds.  Prevents
            /// spamming the deposit action.  Half a second by default.
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
            /// How often the finite state machine ticks, in updates per
            /// second.  A value of 10 means the AI will update every 0.1
            /// seconds.  Lower values reduce CPU usage and improve
            /// determinism on mobile hardware.
            /// </summary>
            public const int AITickRate = 10;

            /// <summary>
            /// How often the behaviour tree runs, in updates per second.
            /// Behaviour tree updates are lighter weight and primarily handle
            /// perception and high‑level intent generation.  A value of 5
            /// means the BT will tick every 0.2 seconds.
            /// </summary>
            public const int BehaviorTickRate = 5;
        }

        public static class Tags
        {
            /// <summary>
            /// Tag assigned to player controlled characters.  AI queries for
            /// potential targets by looking for GameObjects with this tag.
            /// </summary>
            public const string Player = "Player";

            /// <summary>
            /// Tag assigned to AI controlled characters.  Used to filter
            /// collisions and target selection.
            /// </summary>
            public const string AI = "AI";

            /// <summary>
            /// Tag assigned to projectile prefabs.  Useful for quickly
            /// identifying objects that should not be considered as targets.
            /// </summary>
            public const string Projectile = "Projectile";
        }

        public static class Layers
        {
            // Unity layer indices.  Customize these to match your project’s
            // Layer Manager.  These defaults assume the following layout:
            // 0: Default, 6: Environment, 7: Player, 8: AI, 9: Projectile.
            public const int Default = 0;
            public const int Environment = 6;
            public const int Player = 7;
            public const int AI = 8;
            public const int Projectile = 9;
        }
    }
}