using UnityEngine;

/// <summary>
/// Global copy of project constants for projects that do not include
/// namespace-qualified usages.  Some existing scripts may reference
/// <c>ProjectConstants</c> without specifying the <c>MemeArena.Network</c>
/// namespace.  To support those scripts, this duplicate exposes the
/// same fields defined in <see cref="MemeArena.Network.ProjectConstants"/>.
/// New code should prefer the namespaced version to avoid ambiguity.
/// </summary>
public static class ProjectConstants
{
    public static class Match
    {
        /// <summary>
        /// Length of a match in seconds.  Five minutes by default.
        /// </summary>
        public const float MatchLength = MemeArena.Network.ProjectConstants.Match.MatchLength;

        /// <summary>
        /// Minimum time between coin deposit actions in seconds.
        /// </summary>
        public const float DepositCooldown = MemeArena.Network.ProjectConstants.Match.DepositCooldown;

        /// <summary>
        /// Time before an AI respawns after death.  In seconds.
        /// </summary>
        public const float RespawnDelay = MemeArena.Network.ProjectConstants.Match.RespawnDelay;
    }

    public static class AI
    {
        /// <summary>
        /// How often the finite state machine ticks, in updates per second.
        /// </summary>
        public const int AITickRate = MemeArena.Network.ProjectConstants.AI.AITickRate;

        /// <summary>
        /// How often the behaviour tree runs, in updates per second.
        /// </summary>
        public const int BehaviorTickRate = MemeArena.Network.ProjectConstants.AI.BehaviorTickRate;
    }

    public static class Tags
    {
        public const string Player = MemeArena.Network.ProjectConstants.Tags.Player;
        public const string AI = MemeArena.Network.ProjectConstants.Tags.AI;
        public const string Projectile = MemeArena.Network.ProjectConstants.Tags.Projectile;
    }

    public static class Layers
    {
        public const int Default = MemeArena.Network.ProjectConstants.Layers.Default;
        public const int Environment = MemeArena.Network.ProjectConstants.Layers.Environment;
        public const int Player = MemeArena.Network.ProjectConstants.Layers.Player;
        public const int AI = MemeArena.Network.ProjectConstants.Layers.AI;
        public const int Projectile = MemeArena.Network.ProjectConstants.Layers.Projectile;
    }

    /// <summary>
    /// Game‑level constants mirrored from the namespaced version.  Some
    /// legacy scripts refer to ProjectConstants.Game for parameters such
    /// as match length, deposit cooldown and respawn delay.  These fields
    /// simply forward to their equivalents in <see cref="MemeArena.Network.ProjectConstants"/>.
    /// </summary>
    public static class Game
    {
        public const float MatchLength = MemeArena.Network.ProjectConstants.Match.MatchLength;
        public const float DepositCooldown = MemeArena.Network.ProjectConstants.Match.DepositCooldown;
        public const float RespawnDelay = MemeArena.Network.ProjectConstants.Match.RespawnDelay;
    }
}