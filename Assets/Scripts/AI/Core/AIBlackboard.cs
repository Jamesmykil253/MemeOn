using UnityEngine;

namespace MemeArena.AI
{
    /// <summary>
    /// AIBlackboard stores dynamic runtime values for an AI agent.  These
    /// variables are not synchronised across the network; they live only on
    /// the server because all AI logic is server‑side.  Expose the fields
    /// publicly for debugging but hide them in the inspector to avoid
    /// accidental tampering.  Consumers should use the AIController property
    /// to access the blackboard.
    /// </summary>
    public class AIBlackboard : MonoBehaviour
    {
        [HideInInspector] public AIConfig config;

        // Position where the AI spawned.  Used as the return destination when
        // the AI disengages from its target.
        [HideInInspector] public Vector3 spawnPosition;

        // NetworkObjectId of the current target.  Zero indicates no target.
        [HideInInspector] public ulong targetId;

        // Whether the AI has been provoked and should be in an alert state.
        [HideInInspector] public bool aggroed;

        // Time of the last hit received (in Unity's Time.time).  Used to track
        // how long the AI has gone without being hit.
        [HideInInspector] public float lastHitTimestamp;

        // Time since the last successful hit on the target.  Resets when the
        // AI deals damage.  If this exceeds giveUpTimeout, the AI gives up.
        [HideInInspector] public float timeSinceLastSuccessfulHit;

        // Last known position of the target.  Allows the AI to move toward
        // where the target was even if it is no longer visible.
        [HideInInspector] public Vector3 lastKnownTargetPos;
    }
}