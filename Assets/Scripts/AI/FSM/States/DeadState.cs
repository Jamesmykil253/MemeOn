using UnityEngine;
using Unity.Netcode;
using MemeArena.Networking;

namespace MemeArena.AI
{
    /// <summary>
    /// DeadState handles the AI death lifecycle. It notifies the spawner manager to
    /// schedule a respawn and despawns the NetworkObject. After despawn, no further
    /// ticks occur on this instance.
    /// </summary>
    public class DeadState : AIState
    {
        public DeadState(AIController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            // Notify AISpawnerManager about death so it can respawn later.
            AISpawnerManager spawner = Object.FindObjectOfType<AISpawnerManager>();
            if (spawner != null)
            {
                spawner.HandleAIDeath(controller.NetworkObjectId);
            }
            // Despawn the network object after a short delay to allow death VFX if any.
            controller.StartCoroutine(DespawnAfterDelay());
        }

        private System.Collections.IEnumerator DespawnAfterDelay()
        {
            // Wait a single frame to ensure all events propagate.
            yield return null;
            // Only the server should despawn.
            if (controller.IsServer)
            {
                controller.NetworkObject.Despawn();
            }
        }
    }
}