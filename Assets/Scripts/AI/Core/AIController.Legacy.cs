using UnityEngine;
using Unity.Netcode;

namespace MemeArena.AI
{
    /// <summary>
    /// Partial class containing helper methods that mirror the legacy AI
    /// controller interface.  Older behaviour tree nodes and FSM states
    /// reference methods like <c>OffCooldown()</c>, <c>Fire()</c> and
    /// <c>MoveDeterministic()</c>.  These helpers delegate to the new
    /// deterministic movement and attack implementations while preserving
    /// backwards compatibility.  They are intentionally kept simple: if you
    /// require more nuanced behaviour (e.g. different projectile speeds per
    /// ability), update the corresponding methods here.
    /// </summary>
    public partial class AIController
    {
        /// <summary>
        /// Returns true if the AI is ready to attack.  This checks the
        /// internal cooldown timer used by legacy BT nodes.  When the timer
        /// reaches zero the AI can fire again.  This cooldown is
        /// independent of other state transitions.
        /// </summary>
        /// <returns>True if the attack cooldown has expired.</returns>
        public bool OffCooldown()
        {
            return _attackCooldownTimer <= 0f;
        }

        /// <summary>
        /// Resets the internal attack cooldown timer.  Legacy BT nodes call
        /// this after a successful attack to enforce a short delay before
        /// the next attack.  The duration is taken from the AIConfig
        /// asset; if none is assigned a sensible default is used.
        /// </summary>
        public void StartCooldown()
        {
            float cooldown = 0.5f;
            if (_config != null)
            {
                cooldown = _config.attackCooldown;
            }
            _attackCooldownTimer = cooldown;
        }

        /// <summary>
        /// Performs an attack using the appropriate ranged attack method.  If
        /// there is no projectile prefab or muzzle assigned, this method
        /// returns false.  After firing, the attack cooldown is started.
        /// </summary>
        /// <returns>True if a projectile was successfully spawned.</returns>
        public bool Fire()
        {
            bool result = PerformRangedAttack();
            if (result)
            {
                StartCooldown();
            }
            return result;
        }

        /// <summary>
        /// Moves the AI deterministically along the provided direction.  This
        /// delegates to the server‑side Move() implementation.  Only the
        /// horizontal components of the direction are considered.
        /// </summary>
        /// <param name="direction">The desired movement direction.</param>
        /// <param name="dt">Delta time in seconds.</param>
        public void MoveDeterministic(Vector3 direction, float dt)
        {
            Move(direction, dt);
        }

        /// <summary>
        /// Returns the Transform of the current target if available.  If
        /// there is no target or the target is not spawned, null is
        /// returned.  Behaviour tree nodes use this to aim at targets.
        /// </summary>
        public Transform TargetTransform()
        {
            if (!IsServer) return null;
            ulong id = _blackboard.targetId;
            if (id == 0) return null;
            if (NetworkManager.Singleton == null) return null;
            var spawns = NetworkManager.Singleton.SpawnManager;
            if (spawns == null) return null;
            if (!spawns.SpawnedObjects.ContainsKey(id)) return null;
            return spawns.SpawnedObjects[id].transform;
        }

        /// <summary>
        /// Rotates the AI to face the given world position.  This helper
        /// wraps the new FaceTowards() implementation.  If the provided
        /// transform is null, no action is taken.
        /// </summary>
        public void FaceToward(Transform target, float dt)
        {
            if (target == null) return;
            FaceTowards(target.position, dt);
        }

        /// <summary>
        /// Determines whether the current target is within attack range.
        /// This compares the squared distance to the target against the
        /// configured melee and ranged ranges.  Returns false if there is
        /// no target.  Behaviour tree nodes use this to decide whether to
        /// attack.
        /// </summary>
        public bool InAttackRange()
        {
            var t = TargetTransform();
            if (t == null) return false;
            float distSqr = (t.position - transform.position).sqrMagnitude;
            float range = 1f;
            if (_config != null)
            {
                range = Mathf.Max(_config.meleeRange, _config.rangedRange);
            }
            return distSqr <= range * range;
        }

        /// <summary>
        /// Returns true if the AI has a valid target assigned.  Behaviour
        /// tree nodes use this to guard actions that require a target.
        /// </summary>
        public bool HasTarget()
        {
            return _blackboard != null && _blackboard.targetId != 0;
        }
    }
}