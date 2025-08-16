using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MemeArena.Combat;

namespace MemeArena.AI
{
    /// <summary>
    /// AIController drives the finite state machine and coordinates with the behaviour tree.
    /// It executes only on the server to preserve the server-authoritative model. Clients
    /// receive replicated positions/rotations via Netcode components and may run a visual
    /// approximation for smoothness, but no authoritative logic is executed on clients.
    /// </summary>
    [RequireComponent(typeof(AIBlackboard))]
    [RequireComponent(typeof(CharacterController))]
    // Use the fully qualified type here so Unity can attach the HealthServer from MemeArena.Combat.
    [RequireComponent(typeof(MemeArena.Combat.HealthServer))]
    public partial class AIController : NetworkBehaviour
    {
        // Public references for inspector assignment.
        [Header("AI Subsystems")]
        [SerializeField]
        private AIConfig _config;

        [Tooltip("Projectile prefab used for ranged attacks. Must contain a NetworkObject and ProjectileServer.")]
        public GameObject projectilePrefab;

        // Cached component references.
        private AIBlackboard _blackboard;
        private CharacterController _characterController;
        private HealthServer _healthServer;

        // Behaviour tree placeholder. Not fully implemented in this prototype.
        private AIBehaviorTree _behaviorTree;

        // State machine data.
        private readonly Dictionary<string, AIState> _states = new();
        private AIState _currentState;
        private float _aiTickInterval;
        private float _btTickInterval;
        private float _aiTickTimer;
        private float _btTickTimer;
        private bool _initialized;

        /// <summary>
        /// Exposes the blackboard for easy access by states and behaviour tree nodes.
        /// </summary>
        public AIBlackboard Blackboard => _blackboard;

        /// <summary>
        /// Exposes the AI configuration used by this controller.
        /// </summary>
        public AIConfig Config => _config;

        /// <summary>
        /// Name of the current FSM state. Exposed for debugging and inspection. Note
        /// that this value is only valid on the server; clients should not rely on it
        /// for gameplay logic.
        /// </summary>
        public string CurrentStateName => _currentState?.Name ?? string.Empty;

        /// <summary>
        /// Enumeration of AI state identifiers. This mirrors the FSM states defined in
        /// the design documents. Having this enum available resolves references in
        /// legacy scripts such as AcquireTargetState that expect AIController.AIStateId.
        /// </summary>
        public enum AIStateId
        {
            Idle,
            Alert,
            Pursue,
            MeleeAttack,
            RangedAttack,
            Evade,
            Stunned,
            ReturnToSpawn,
            Dead
        }

        private void Awake()
        {
            _blackboard = GetComponent<AIBlackboard>();
            _characterController = GetComponent<CharacterController>();
            _healthServer = GetComponent<HealthServer>();

            // Assign config from blackboard if not set explicitly.
            if (_config == null && _blackboard.config != null)
            {
                _config = _blackboard.config;
            }

            // Subscribe to damage events on the server to trigger aggro.
            _healthServer.OnDamageReceived += OnDamageReceived;
            _healthServer.OnDeath += OnDeath;

            // Set initial blackboard values.
            _blackboard.spawnPosition = transform.position;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                // Set tick intervals based on constants.
                _aiTickInterval = 1f / ProjectConstants.AI.AITickRate;
                _btTickInterval = 1f / ProjectConstants.AI.BehaviorTickRate;
                // Construct state instances on the server. These are lightweight and hold
                // references back to this controller.
                _states[nameof(IdleState)] = new IdleState(this);
                _states[nameof(AlertState)] = new AlertState(this);
                _states[nameof(PursueState)] = new PursueState(this);
                _states[nameof(MeleeAttackState)] = new MeleeAttackState(this);
                _states[nameof(RangedAttackState)] = new RangedAttackState(this);
                _states[nameof(EvadeState)] = new EvadeState(this);
                _states[nameof(StunnedState)] = new StunnedState(this);
                _states[nameof(ReturnToSpawnState)] = new ReturnToSpawnState(this);
                _states[nameof(DeadState)] = new DeadState(this);
                // Set initial state to Idle.
                ChangeState(nameof(IdleState));
                _initialized = true;
            }
        }

        private void Update()
        {
            if (!IsServer || !_initialized) return;
            float dt = Time.deltaTime;
            _aiTickTimer += dt;
            _btTickTimer += dt;
            // Behaviour Tree tick at its own interval.
            if (_btTickTimer >= _btTickInterval)
            {
                _btTickTimer -= _btTickInterval;
                _behaviorTree?.Tick(_btTickInterval);
            }
            // AI state tick at its own interval.
            if (_aiTickTimer >= _aiTickInterval)
            {
                float step = _aiTickTimer;
                _aiTickTimer = 0f;
                _currentState?.Tick(step);
            }
        }

        /// <summary>
        /// Switches the current state to the given name if it exists. Handles exit and
        /// entry calls appropriately.
        /// </summary>
        /// <param name="stateName">Name of the state to switch to.</param>
        public void ChangeState(string stateName)
        {
            if (!_states.TryGetValue(stateName, out AIState next))
            {
                Debug.LogError($"AIController: Unknown state '{stateName}'.");
                return;
            }
            if (_currentState == next) return;
            _currentState?.Exit();
            _currentState = next;
            _currentState.Enter();
            // Optionally update debug indicators here (e.g. colour changes).
        }

        /// <summary>
        /// Called by HealthServer when this AI takes damage. If not already aggroed, this
        /// sets up the blackboard target and transitions to the Alert state.
        /// </summary>
        /// <param name="attackerId">NetworkObjectId of the attacker.</param>
        private void OnDamageReceived(ulong attackerId)
        {
            if (!IsServer) return;
            _blackboard.lastHitTimestamp = Time.time;
            _blackboard.timeSinceLastSuccessfulHit = 0f;
            _blackboard.aggroed = true;
            _blackboard.targetId = attackerId;
            // If currently idle or returning, enter alerted state.
            if (_currentState is IdleState or ReturnToSpawnState)
            {
                ChangeState(nameof(AlertState));
            }
        }

        /// <summary>
        /// Called by HealthServer when health reaches zero. Switch to Dead state.
        /// </summary>
        private void OnDeath()
        {
            if (!IsServer) return;
            ChangeState(nameof(DeadState));
        }

        #region Helper Methods for states

        public NetworkObject FindTargetNetworkObject()
        {
            if (_blackboard.targetId == 0) return null;
            // Use NetworkManager to find the NetworkObject by id.
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(_blackboard.targetId, out NetworkObject obj))
            {
                return obj;
            }
            return null;
        }

        public bool IsTargetAlive()
        {
            NetworkObject target = FindTargetNetworkObject();
            return target != null && target.IsSpawned;
        }

        public void MoveTowards(Vector3 destination, float speed)
        {
            Vector3 direction = destination - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;
            if (distance < 0.01f) return;
            Vector3 move = direction.normalized * speed;
            _characterController.Move(move * _aiTickInterval);
            // Rotate to face movement direction.
            if (move != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(move.normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _blackboard.stats.rotationSpeed * _aiTickInterval);
            }
        }

        public void StopMovement()
        {
            // CharacterController.Move with zero will have no effect; this method exists for clarity.
        }

        /// <summary>
        /// Performs a melee attack on the current target. This method is called by
        /// MeleeAttackState. It looks up the target's HealthServer and applies damage.
        /// </summary>
        public void PerformMeleeAttack()
        {
            NetworkObject target = FindTargetNetworkObject();
            if (target == null) return;
            HealthServer targetHealth = target.GetComponent<HealthServer>();
            if (targetHealth != null)
            {
                // Apply immediate damage. The amount could be read from config/stats.
                targetHealth.ApplyDamageServerRpc(10, NetworkObjectId);
                // Reset counters.
                _blackboard.timeSinceLastSuccessfulHit = 0f;
                _blackboard.failedHitCounter = 0;
            }
        }

        /// <summary>
        /// Performs a ranged attack on the current target. Spawns a projectile prefab
        /// server-side that travels towards the target. The projectile behaviour is
        /// defined in ProjectileServer.
        /// </summary>
        public void PerformRangedAttack()
        {
            NetworkObject target = FindTargetNetworkObject();
            if (target == null) return;
            // Attempt to spawn a projectile at a spawn point on the AI (e.g. muzzle).
            // Here we assume a child transform named "Muzzle" exists. If not, use current
            // position offset.
            Transform muzzle = transform;
            if (transform.childCount > 0)
            {
                Transform found = transform.Find("Muzzle");
                if (found != null) muzzle = found;
            }
            Vector3 spawnPos = muzzle.position;
            Quaternion spawnRot = Quaternion.LookRotation((target.transform.position - spawnPos).normalized);
            if (!IsServer) return;
            // Only spawn on the server. Compute direction and invoke RPC for proper network ownership.
            Vector3 dir = (target.transform.position - spawnPos).normalized;
            NetworkSpawnProjectileServerRpc(spawnPos, dir);
        }

        #endregion

        #region RPCs

        /// <summary>
        /// Server RPC to spawn a projectile. The actual instantiation logic lives server-side
        /// inside this method. Called by PerformRangedAttack.
        /// </summary>
        [ServerRpc]
        private void NetworkSpawnProjectileServerRpc(Vector3 spawnPosition, Vector3 direction)
        {
            if (!IsServer) return;
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"AIController on {gameObject.name} has no projectilePrefab assigned.");
                return;
            }
            GameObject proj = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
            var netObj = proj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
            var projectile = proj.GetComponent<MemeArena.Combat.ProjectileServer>();
            if (projectile != null)
            {
                projectile.Init(direction, NetworkObjectId);
            }
        }

        /// <summary>
        /// Called when the component is destroyed. Override to unsubscribe from events and
        /// invoke the base class implementation. Without this override the same method
        /// signature in another partial class would hide the NetworkBehaviour version and
        /// generate a warning.
        /// </summary>
        /// <summary>
        /// Override OnDestroy to unsubscribe from health events and call the base
        /// implementation. This method must be public to match the access level of the
        /// inherited NetworkBehaviour.OnDestroy.
        /// </summary>
        public override void OnDestroy()
        {
            if (_healthServer != null)
            {
                _healthServer.OnDamageReceived -= OnDamageReceived;
                _healthServer.OnDeath -= OnDeath;
            }
            base.OnDestroy();
        }

        #endregion
    }
}