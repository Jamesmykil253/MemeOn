using UnityEngine;
using Unity.Netcode;
using MemeArena.Network;
using MemeArena.AI;

namespace MemeArena.Combat
{
    /// <summary>
    /// Handles server‑side behaviour for projectiles.  Projectiles are
    /// authoritative: they move and apply damage only on the server.  Clients
    /// simply visualise their movement.  When a projectile hits a valid
    /// target it applies damage and despawns.  If it expires without a hit
    /// it despawns and notifies the owner via OnFailedHit.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    public class ProjectileServer : NetworkBehaviour
    {
        [Tooltip("Units per second that this projectile travels.")]
        public float speed = 20f;

        [Tooltip("Lifetime in seconds before the projectile despawns if it doesn't hit anything.")]
        public float lifeSeconds = 3f;

        [Tooltip("Damage applied on hit.")]
        public int damage = 10;

        // Owner and team assigned at spawn.  Owner is the clientId of the
        // entity that fired the projectile; ownerTeam is used to prevent
        // friendly fire.
        [HideInInspector] public ulong ownerClientId;
        [HideInInspector] public int ownerTeam;

        private float _timer;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _timer = lifeSeconds;
            }
        }

        private void Update()
        {
            if (!IsServer) return;
            float dt = Time.deltaTime;
            // Move forward in local space.
            transform.position += transform.forward * speed * dt;
            _timer -= dt;
            if (_timer <= 0f)
            {
                NotifyOwnerFailure();
                Despawn();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            // Ignore collisions with self or owner.
            if (other.gameObject == gameObject) return;
            // Check for health component.
            NetworkHealth health = other.GetComponent<NetworkHealth>();
            TeamId targetTeam = other.GetComponent<TeamId>();
            if (health != null && targetTeam != null)
            {
                // Only hit opposing teams.
                if (targetTeam.team != ownerTeam)
                {
                    health.TakeDamageServerRpc(damage, ownerClientId);
                    NotifyOwnerSuccess();
                    Despawn();
                    return;
                }
            }
            // Otherwise ignore or pass through.  Note: environment collisions
            // cause the projectile to despawn without hitting.
            if (other.gameObject.layer == ProjectConstants.Layers.Environment)
            {
                NotifyOwnerFailure();
                Despawn();
            }
        }

        /// <summary>
        /// Launches the projectile.  Called by the AI controller after spawn.
        /// Sets the collider to a trigger to avoid interfering with physics.
        /// </summary>
        public void Launch()
        {
            // Ensure the collider is a trigger on the server.  This method is
            // executed immediately after NetworkObject.Spawn().
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        /// <summary>
        /// Overload allowing callers to specify the initial direction, speed,
        /// lifetime and damage of the projectile.  This is provided for
        /// compatibility with older PlayerCombatController implementations
        /// that pass these values directly to the projectile.  After
        /// assigning the values, the method delegates to the parameterless
        /// Launch() to configure the collider.  The timer is reset based on
        /// the specified lifetime.
        /// </summary>
        /// <param name="direction">Normalized direction to travel in world space.</param>
        /// <param name="newSpeed">Units per second.</param>
        /// <param name="life">Lifetime in seconds.</param>
        /// <param name="newDamage">Damage applied on hit.</param>
        public void Launch(Vector3 direction, float newSpeed, float life, int newDamage)
        {
            // Update fields according to supplied parameters.
            if (direction.sqrMagnitude > 1e-6f)
            {
                transform.forward = direction.normalized;
            }
            speed = newSpeed;
            lifeSeconds = life;
            damage = newDamage;
            // Reset the internal life timer.
            _timer = life;
            // Delegate to the default launch behaviour to mark the collider as trigger.
            Launch();
        }

        /// <summary>
        /// Launches the projectile and sets its parameters based on the
        /// specified owner GameObject.  This overload matches the legacy
        /// signature used by PlayerCombatController.  The owner provides
        /// orientation and team/owner information.  The damage, speed and
        /// lifetime parameters override the defaults on this component.
        /// </summary>
        /// <param name="owner">The GameObject that spawned the projectile (usually the player).</param>
        /// <param name="newDamage">Damage to apply on hit.</param>
        /// <param name="newSpeed">Movement speed in units per second.</param>
        /// <param name="life">Lifetime in seconds before despawning.</param>
        public void Launch(GameObject owner, int newDamage, float newSpeed, float life)
        {
            // Set orientation to match the owner's forward direction.
            if (owner != null)
            {
                transform.rotation = owner.transform.rotation;
                // Assign owner identification if possible.
                NetworkObject n = owner.GetComponent<NetworkObject>();
                if (n != null)
                {
                    ownerClientId = n.OwnerClientId;
                }
                TeamId team = owner.GetComponent<TeamId>();
                if (team != null)
                {
                    ownerTeam = team.team;
                }
            }
            // Apply parameters.
            damage = newDamage;
            speed = newSpeed;
            lifeSeconds = life;
            _timer = life;
            // Configure collider and other launch settings.
            Launch();
        }

        private void NotifyOwnerSuccess()
        {
            if (!IsServer) return;
            // Find the owner's AIController and inform it of a successful hit.
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ownerClientId))
            {
                var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ownerClientId].gameObject;
                var ai = obj.GetComponent<AIController>();
                ai?.OnSuccessfulHit();
            }
        }

        private void NotifyOwnerFailure()
        {
            if (!IsServer) return;
            // Notify the owner AI of a failed hit.
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ownerClientId))
            {
                var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ownerClientId].gameObject;
                var ai = obj.GetComponent<AIController>();
                ai?.OnFailedHit();
            }
        }

        private void Despawn()
        {
            // Safely despawn the projectile on the server.
            if (IsSpawned)
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}