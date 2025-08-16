using UnityEngine;
using Unity.Netcode;

namespace MemeArena.AI
{
    /// <summary>
    /// AIBlackboard stores runtime data used by both the FSM and Behaviour Tree. Values
    /// are local to the AI instance and are never directly exposed to clients. Only a
    /// small subset of keys are replicated for debugging or visualisation as needed.
    /// </summary>
    public class AIBlackboard : MonoBehaviour
    {
        [Tooltip("AI configuration defining attack ranges, aggro radius, etc.")]
        public AIConfig config;
        [Tooltip("Character stats for this AI instance.")]
        public CharacterStats stats;

        // Data keys defined in ProjectConstants.BlackboardKeys
        [HideInInspector] public Vector3 spawnPosition;
        [HideInInspector] public ulong targetId;
        [HideInInspector] public bool aggroed;
        [HideInInspector] public float lastHitTimestamp;
        [HideInInspector] public float timeSinceLastSuccessfulHit;
        [HideInInspector] public Vector3 lastKnownTargetPos;
        [HideInInspector] public int failedHitCounter;
        [HideInInspector] public int patienceThreshold = 3;

        private NetworkObject _networkObject;

        public NetworkObject NetworkObject => _networkObject ??= GetComponent<NetworkObject>();

        private void Start()
        {
            // Capture the spawn position on startup so the AI knows where to return.
            spawnPosition = transform.position;
            aggroed = false;
            targetId = 0;
        }

        /// <summary>
        /// Clears the blackboard values except for the immutable ones like spawn position. Call
        /// this when the AI respawns or disengages.
        /// </summary>
        public void ResetBlackboard()
        {
            targetId = 0;
            aggroed = false;
            lastHitTimestamp = 0f;
            timeSinceLastSuccessfulHit = 0f;
            lastKnownTargetPos = spawnPosition;
            failedHitCounter = 0;
        }
    }
}