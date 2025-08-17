using UnityEngine;
using Unity.Netcode;
using MemeArena.Spawning;

namespace MemeArena.AI
{
    /// <summary>
    /// Represents the AI when it is dead.  On entry the AI notifies the
    /// spawner that it has died and despawns the network object.  The
    /// AISpawnerManager is responsible for respawning a new instance after
    /// ProjectConstants.Match.RespawnDelay seconds.
    /// </summary>
    public class DeadState : AIState
    {
        private bool _notified;

        public DeadState(AIController controller) : base(controller, nameof(DeadState)) { }

        public override void Enter()
        {
            if (_notified) return;
            _notified = true;
            // Notify spawner of this AI's death.
            var spawner = Object.FindAnyObjectByType<AISpawnerManager>();
            if (spawner != null)
            {
                spawner.HandleAIDeath(controller.GetComponent<NetworkObject>().NetworkObjectId);
            }
            // Despawn network object after a short delay.  We do this in
            // Enter() instead of Tick() because entering this state is a
            // one‑shot event.
            controller.GetComponent<NetworkObject>().Despawn(true);
        }
    }
}