using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// CharacterStats defines baseline movement and health values for an AI
    /// entity.  These values are independent of AI-specific parameters and can
    /// be reused by player controlled characters.  Use ScriptableObjects to
    /// share the same stats across multiple prefabs without duplication.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterStats", menuName = "MemeArena/Character Stats", order = 0)]
    public class CharacterStats : ScriptableObject
    {
        [Header("Health")]
        [Tooltip("Maximum health for this character.")]
        public int maxHealth = 100;

        [Header("Movement")]
        [Tooltip("Forward movement speed in units per second.")]
        public float moveSpeed = 5f;

        [Tooltip("Angular turning speed in degrees per second.")]
        public float rotationSpeed = 720f;
    }
}