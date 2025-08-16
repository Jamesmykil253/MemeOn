using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// ScriptableObject containing tunable parameters for AI behaviour. Values can be
    /// adjusted per enemy archetype via the Unity Inspector. Having a single asset
    /// for each archetype allows designers to tweak without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "MemeArena/AI/Config", fileName = "AIConfig")]
    public class AIConfig : ScriptableObject
    {
        [Header("Aggro Settings")]
        [Tooltip("Radius within which the AI will pursue a target once aggroed.")]
        public float aggroRadius = 10f;
        [Tooltip("Time in seconds before the AI gives up chasing if it hasn't landed a hit.")]
        public float giveUpTimeout = 5f;
        [Tooltip("Maximum distance from spawn the AI will pursue before returning.")]
        public float maxPursueRadius = 15f;

        [Header("Attack Settings")]
        [Tooltip("Distance at which the AI uses its melee attack.")]
        public float meleeRange = 2f;
        [Tooltip("Distance at which the AI uses its ranged attack.")]
        public float rangedRange = 8f;
        [Tooltip("Cooldown between consecutive attacks in seconds.")]
        public float attackCooldown = 1f;

        [Header("Evade Settings")]
        [Tooltip("Duration of an evade manoeuvre in seconds.")]
        public float evadeDuration = 0.5f;
        [Tooltip("Distance covered during an evade manoeuvre.")]
        public float evadeDistance = 2f;

        [Header("Health")]
        [Tooltip("Maximum health for this AI archetype.")]
        public int maxHealth = 100;

        [Header("Reaction")]
        [Tooltip("Mean reaction delay (seconds) after the AI is alerted.")]
        public float reactionDelayMean = 0.2f;
        [Tooltip("Standard deviation for reaction delay (seconds).")]
        public float reactionDelayStdDev = 0.05f;
    }
}