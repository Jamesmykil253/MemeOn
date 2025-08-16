using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Defines generic character statistics that can be reused across players and AI. These
    /// stats are meant for initial prototypes; later systems may derive runtime values
    /// from progression or equipment. Designers should assign this asset on prefabs.
    /// </summary>
    [CreateAssetMenu(menuName = "MemeArena/Character Stats", fileName = "CharacterStats")]
    public class CharacterStats : ScriptableObject
    {
        [Header("Health")]
        public int maxHealth = 100;

        [Header("Movement")]
        [Tooltip("Movement speed in meters per second.")]
        public float moveSpeed = 3f;
        [Tooltip("Rotation speed in degrees per second.")]
        public float rotationSpeed = 720f;

        [Header("Team")]
        [Tooltip("Team identifier (0 for neutral, positive values for teams).")]
        public int teamId = 0;
    }
}