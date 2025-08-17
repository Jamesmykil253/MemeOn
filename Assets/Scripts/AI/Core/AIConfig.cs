using UnityEngine;
using MemeArena.Network;

namespace MemeArena.AI
{
    /// <summary>
    /// ScriptableObject containing tunable parameters for AI behaviour.  One
    /// instance of this asset should be created per AI archetype and assigned
    /// to the AIController via the inspector.  Designers can tweak these
    /// values to adjust aggro radii, attack ranges, timeouts and other
    /// behaviour characteristics without modifying code.
    /// </summary>
    [CreateAssetMenu(fileName = "AIConfig", menuName = "MemeArena/AI/Config", order = 0)]
    public class AIConfig : ScriptableObject
    {
        [Header("Senses")]
        [Tooltip("How far the AI can sense targets before engaging.  Measured in units.")]
        public float aggroRadius = 12f;

        [Tooltip("Maximum distance the AI will chase its target before giving up.")]
        public float maxPursueRadius = 25f;

        [Tooltip("Time in seconds without a successful hit before the AI gives up the chase.")]
        public float giveUpTimeout = 5f;

        [Header("Attack Ranges")]
        [Tooltip("Distance within which the AI will perform a melee attack.")]
        public float meleeRange = 2.5f;

        [Tooltip("Distance within which the AI will perform a ranged attack.")]
        public float rangedRange = 10f;

        [Header("Timings")]
        [Tooltip("Cooldown between attacks, in seconds.")]
        public float attackCooldown = 0.5f;

        [Tooltip("Duration of an evade manoeuvre, in seconds.")]
        public float evadeDuration = 0.6f;

        [Tooltip("Distance the AI will move while evading.")]
        public float evadeDistance = 4f;

        [Tooltip("Duration of the stunned state, in seconds.")]
        public float stunnedDuration = 1.0f;

        [Tooltip("Mean reaction delay when entering the alert state, in seconds.")]
        public float reactionDelayMean = 0.5f;

        [Tooltip("Standard deviation of the reaction delay when entering the alert state.")]
        public float reactionDelayStdDev = 0.1f;

        [Header("Other")]
        [Tooltip("Time before this AI respawns after death.  Defaults to the project constant.")]
        public float respawnDelay = ProjectConstants.Match.RespawnDelay;
    }
}