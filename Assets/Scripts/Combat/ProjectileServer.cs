using UnityEngine;
using Unity.Netcode;

namespace MemeArena.Combat
{
    /// <summary>
    /// Server-side projectile that moves in a straight line and applies damage upon
    /// collision with a HealthServer. The projectile despawns after a lifetime.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    public class ProjectileServer : NetworkBehaviour
    {
        [Tooltip("Speed of the projectile in meters per second.")]
        public float speed = 10f;
        [Tooltip("Damage dealt on hit.")]
        public int damage = 10;
        [Tooltip("Lifetime of the projectile in seconds.")]
        public float lifetime = 5f;

        private Vector3 _direction;
        private ulong _ownerId;

        /// <summary>
        /// Launches the projectile from a given owner with specified parameters.  This
        /// wrapper exists for compatibility with older scripts that call
        /// ProjectileServer.Launch().  It assigns the damage, speed and lifetime
        /// fields and calls Init() with the forward direction of the owner.
        /// </summary>
        /// <param name="owner">The GameObject that fired this projectile.</param>
        /// <param name="damage">Damage value to apply on hit.</param>
        /// <param name="speed">Projectile travel speed in meters per second.</param>
        /// <param name="lifetime">How many seconds the projectile should live.</param>
        public void Launch(GameObject owner, int damage, float speed, float lifetime)
        {
            this.damage = damage;
            this.speed = speed;
            this.lifetime = lifetime;
            Vector3 direction = owner != null ? owner.transform.forward : Vector3.forward;
            ulong ownerId = 0;
            if (owner != null)
            {
                var netObj = owner.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    ownerId = netObj.NetworkObjectId;
                }
            }
            Init(direction, ownerId);
        }

        /// <summary>
        /// Initializes the projectile's movement direction and owner. Must be called
        /// immediately after spawning on the server.
        /// </summary>
        public void Init(Vector3 direction, ulong ownerId)
        {
            _direction = direction.normalized;
            _ownerId = ownerId;
        }

        private void Update()
        {
            if (!IsServer) return;
            transform.position += _direction * speed * Time.deltaTime;
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                NetworkObject.Despawn();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            // Ignore trigger if collided with owner.
            NetworkObject otherNetObj = other.GetComponent<NetworkObject>();
            if (otherNetObj != null && otherNetObj.NetworkObjectId == _ownerId)
                return;
            var health = other.GetComponent<HealthServer>();
            if (health != null)
            {
                health.ApplyDamageServerRpc(damage, _ownerId);
                NetworkObject.Despawn();
            }
        }
    }
}