using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
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
    [RequireComponent(typeof(NetworkTransform))]
    public class ProjectileServer : NetworkBehaviour
    {
        [Tooltip("Units per second that this projectile travels.")]
        public float speed = 20f;

        [Tooltip("Lifetime in seconds before the projectile despawns if it doesn't hit anything.")]
        public float lifeSeconds = 3f;

        [Tooltip("Damage applied on hit.")]
        public int damage = 10;
    [Header("Debugging")]
    [SerializeField] private bool debugLogs = false;

        // Owner and team assigned at spawn.  Owner is the clientId of the
        // entity that fired the projectile; ownerTeam is used to prevent
        // friendly fire.
    [HideInInspector] public ulong ownerClientId;
    // NetworkObjectId of the owner, used to look up components reliably.
    [HideInInspector] public ulong ownerObjectId;
        [HideInInspector] public int ownerTeam;

        private float _timer;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _timer = lifeSeconds;
                if (debugLogs)
                    Debug.Log($"ProjectileServer(Server) spawned. ownerClient={ownerClientId} ownerObj={ownerObjectId} team={ownerTeam} dmg={damage} speed={speed} life={lifeSeconds}");
            }
        }

        private void Update()
        {
            if (!IsServer) return;
            float dt = Time.deltaTime;
            // Move forward in local space.
            transform.position += transform.forward * speed * dt;
            _timer -= dt;
            if (debugLogs)
            {
                Debug.Log($"ProjectileServer(Server) step: pos={transform.position} remain={_timer:F2}");
            }
            if (_timer <= 0f)
            {
                if (debugLogs) Debug.Log("ProjectileServer(Server) expired");
                NotifyOwnerFailure();
                Despawn();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            // Ignore collisions with self or owner.
            if (other.gameObject == gameObject) return;
            // Check for damage interface first (preferred), then legacy health.
            var damageable = other.GetComponentInParent<IDamageable>();
            NetworkHealth health = other.GetComponentInParent<NetworkHealth>();
            TeamId targetTeam = other.GetComponentInParent<TeamId>();
            if (targetTeam != null)
            {
                // Only hit opposing teams.
                if (targetTeam.team != ownerTeam)
                {
                    if (debugLogs) Debug.Log($"ProjectileServer(Server) hit candidate target {other.name} team={targetTeam.team}");
                    bool applied = false;
                    if (damageable != null)
                    {
                        damageable.ApplyDamage(damage, gameObject, other.ClosestPoint(transform.position));
                        applied = true;
                    }
                    else if (health != null)
                    {
                        health.ApplyDamageServer(damage, ownerClientId);
                        applied = true;
                    }
                    if (applied)
                    {
                        // Raise unified success event when possible
                        ulong victimId = 0UL;
                        var victimNO = other.GetComponentInParent<NetworkObject>();
                        if (victimNO) victimId = victimNO.NetworkObjectId;
                        CombatEvents.RaiseSuccessfulHit(ownerObjectId, victimId, damage);
                        NotifyOwnerSuccess();
                        Despawn();
                        return;
                    }
                }
                else if (debugLogs)
                {
                    Debug.Log($"ProjectileServer(Server) ignored friendly target {other.name}");
                }
            }
            // Otherwise ignore or pass through.  Note: environment collisions
            // cause the projectile to despawn without hitting.
            if (other.gameObject.layer == ProjectConstants.Layers.Environment)
            {
                if (debugLogs) Debug.Log($"ProjectileServer(Server) hit environment: {other.name}");
                CombatEvents.RaiseFailedHit(ownerObjectId, 0UL, "Environment");
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
                if (debugLogs) Debug.Log("ProjectileServer(Server) collider set to trigger");
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
            if (debugLogs) Debug.Log($"ProjectileServer(Server) launched dir={direction} speed={speed} life={life} dmg={damage}");
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
            ownerObjectId = n.NetworkObjectId;
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
            if (debugLogs) Debug.Log($"ProjectileServer(Server) launched by owner={owner?.name} dmg={damage} speed={speed} life={lifeSeconds}");
        }

        private void NotifyOwnerSuccess()
        {
            if (!IsServer) return;
            // Find the owner's AIController and inform it of a successful hit.
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ownerObjectId))
            {
                var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ownerObjectId].gameObject;
                var ai = obj.GetComponent<AIController>();
                ai?.OnSuccessfulHit();
                if (debugLogs) Debug.Log("ProjectileServer(Server) notified owner of success");
            }
        }

        private void NotifyOwnerFailure()
        {
            if (!IsServer) return;
            // Notify the owner AI of a failed hit.
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ownerObjectId))
            {
                var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ownerObjectId].gameObject;
                var ai = obj.GetComponent<AIController>();
                ai?.OnFailedHit();
                if (debugLogs) Debug.Log("ProjectileServer(Server) notified owner of failure");
            }
        }

        private void Despawn()
        {
            // Safely despawn the projectile on the server.
            if (IsSpawned)
            {
                GetComponent<NetworkObject>().Despawn(true);
                if (debugLogs) Debug.Log("ProjectileServer(Server) despawned network object");
            }
            else
            {
                Destroy(gameObject);
                if (debugLogs) Debug.Log("ProjectileServer(Server) destroyed (was not spawned)");
            }
        }
    }
}