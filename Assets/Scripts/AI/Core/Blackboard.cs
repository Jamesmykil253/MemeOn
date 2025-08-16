using System;
using Unity.Netcode;
using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// Minimal, typed blackboard. Lives on the AI GameObject for easy inspector setup.
    /// Network-only values are resolved on the server.
    /// </summary>
    public class Blackboard : MonoBehaviour
    {
        [Header("References")]
        public AIConfig config;
        public CharacterStats stats;

        [Header("Runtime (Read-Only at Edit-Time)")]
        public Vector3 spawnPosition;
        public NetworkObject currentTarget;
        public Vector3 lastKnownTargetPosition;
        public float timeSinceLastSuccessfulHit;

        [Header("Debug")]
        public bool hasLineOfSight;
        public bool isUnderFire;
    }
}
