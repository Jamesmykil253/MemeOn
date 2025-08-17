using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MemeArena.Combat;
using MemeArena.Network;

namespace MemeArena.AI
{
    /// <summary>
    /// AIController drives the finite state machine and optionally a behaviour
    /// tree.  It executes only on the server to preserve the
    /// server‑authoritative model; clients receive position and rotation
    /// updates through Netcode components such as NetworkTransform.  This
    /// controller also exposes helper methods for moving, facing and
    /// attacking.
    /// </summary>
    [RequireComponent(typeof(AIBlackboard))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkHealth))]
    // Note: declare AIController as partial to support extension in other files.
    public partial class AIController : NetworkBehaviour
    {
        [Header("AI References")]
        [SerializeField]
        private AIConfig _config;

        [SerializeField]
        private CharacterStats _stats;

        [Tooltip("Prefab used for ranged attacks.  Must include a NetworkObject and ProjectileServer component.")]
        public GameObject projectilePrefab;

        [Tooltip("Transform representing the muzzle position for projectile spawning.")]
        public Transform muzzle;

        // Internal references
        private AIBlackboard _blackboard;
        private CharacterController _characterController;
        private NetworkHealth _health;

        // FSM data
        private readonly Dictionary<string, AIState> _states = new();
        private AIState _currentState;
        private float _aiTickInterval;
        private float _btTickInterval;
        private float _aiTimer;
        private float _btTimer;
        private bool _initialized;

        // Attack cooldown timer used by legacy helper methods has been removed.
        // The new FSM handles attack timings within its own states.

        // Simple behaviour tree placeholder; not fully implemented but
        // included for extensibility.
        private BehaviorTree _behaviorTree;

        /// <summary> Exposes the blackboard for states and BT nodes. </summary>
        public AIBlackboard Blackboard => _blackboard;

        /// <summary> Exposes the AI configuration. </summary>
        public AIConfig Config => _config;

        /// <summary> Exposes the character stats. </summary>
        public CharacterStats Stats => _stats;

        /// <summary> Name of the current state; server only. </summary>
        public string CurrentStateName => _currentState?.Name ?? string.Empty;

        private void Awake()
        {
            _blackboard = GetComponent<AIBlackboard>();
            _characterController = GetComponent<CharacterController>();
            _health = GetComponent<NetworkHealth>();

            // Copy config from blackboard if one was not set via inspector.
            if (_config == null && _blackboard.config != null)
            {
                _config = _blackboard.config;
            }

            // Persist stats to blackboard for debugging.
            if (_stats == null)
            {
                Debug.LogWarning($"{name}: CharacterStats not assigned.  AI will not move or rotate correctly.");
            }

            // Set spawn position on blackboard.
            _blackboard.spawnPosition = transform.position;

            // Subscribe to health events on the server.  Do not subscribe on clients.
            _health.OnDamageReceived += OnDamageReceived;
            _health.OnDeath += OnDeath;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;

            // Configure tick intervals.  Avoid dividing by zero.
            _aiTickInterval = ProjectConstants.AI.AITickRate > 0 ? 1f / ProjectConstants.AI.AITickRate : 0.1f;
            _btTickInterval = ProjectConstants.AI.BehaviorTickRate > 0 ? 1f / ProjectConstants.AI.BehaviorTickRate : 0.2f;

            // Create state instances.  Keys match class names for simplicity.
            _states[nameof(IdleState)] = new IdleState(this);
            _states[nameof(AlertState)] = new AlertState(this);
            _states[nameof(PursueState)] = new PursueState(this);
            _states[nameof(MeleeAttackState)] = new MeleeAttackState(this);
            _states[nameof(RangedAttackState)] = new RangedAttackState(this);
            _states[nameof(EvadeState)] = new EvadeState(this);
            _states[nameof(StunnedState)] = new StunnedState(this);
            _states[nameof(ReturnToSpawnState)] = new ReturnToSpawnState(this);
            _states[nameof(DeadState)] = new DeadState(this);

            // Initialize state to Idle.
            ChangeState(nameof(IdleState));
            _initialized = true;

        }

        private void Update()
        {
            if (!IsServer || !_initialized) return;

            float dt = Time.deltaTime;
            _aiTimer += dt;
            _btTimer += dt;

            // Run behaviour tree at its own cadence.
            if (_btTimer >= _btTickInterval)
            {
                _btTimer -= _btTickInterval;
                _behaviorTree?.Tick(_btTickInterval);
            }

            // Run the current state at its own cadence.
            if (_aiTimer >= _aiTickInterval)
            {
                float step = _aiTimer;
                _aiTimer = 0f;
                _currentState?.Tick(step);
                // Update time since last successful hit.
                _blackboard.timeSinceLastSuccessfulHit += step;
            }
        }

        /// <summary>
        /// Changes the current state to the given state name.  If the state
        /// does not exist in the dictionary, a warning is logged.
        /// </summary>
        public void ChangeState(string stateName)
        {
            if (_currentState != null && _currentState.Name == stateName)
            {
                return; // Already in this state.
            }
            if (_states.TryGetValue(stateName, out var newState))
            {
                _currentState?.Exit();
                _currentState = newState;
                _currentState.Enter();
            }
            else
            {
                Debug.LogWarning($"{name}: Attempted to change to unknown state '{stateName}'.");
            }
        }

        /// <summary>
        /// Called when the AI takes damage.  Assigns the attacker as the
        /// current target and enters the alert state if idle.  Executed on
        /// server only.
        /// </summary>
        private void OnDamageReceived(int amount, ulong attackerId)
        {
            if (!IsServer) return;
            // Record the time of the last hit.
            _blackboard.lastHitTimestamp = Time.time;
            _blackboard.timeSinceLastSuccessfulHit = 0f;
            // Mark as aggroed and set target.
            _blackboard.aggroed = true;
            _blackboard.targetId = attackerId;
            // Attempt to remember the last known position of the attacker.
            NetworkObject targetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(attackerId)
                ? NetworkManager.Singleton.SpawnManager.SpawnedObjects[attackerId]
                : null;
            if (targetObj != null)
            {
                _blackboard.lastKnownTargetPos = targetObj.transform.position;
            }
            // If currently idle or returning to spawn, transition to alert.
            if (_currentState is IdleState || _currentState is ReturnToSpawnState)
            {
                ChangeState(nameof(AlertState));
            }
        }

        /// <summary>
        /// Called when the AI's attack hits a valid target.  Resets the
        /// timeSinceLastSuccessfulHit counter.  Executed on server only.
        /// </summary>
        public void OnSuccessfulHit()
        {
            if (!IsServer) return;
            _blackboard.timeSinceLastSuccessfulHit = 0f;
        }

        /// <summary>
        /// Called when the AI's attack fails to hit a valid target.  No
        /// default behaviour; states may override to perform evasive actions.
        /// </summary>
        public void OnFailedHit()
        {
            // Intentionally left blank.  States may choose to override.
        }

        /// <summary>
        /// Called when this AI's health reaches zero.  Transitions to the
        /// DeadState.  Executed on server only.
        /// </summary>
        private void OnDeath()
        {
            if (!IsServer) return;
            ChangeState(nameof(DeadState));
        }

        #region Movement Helpers
        /// <summary>
        /// Moves the AI forward along the provided direction at the speed
        /// defined in the CharacterStats.  This method ignores vertical
        /// movement; the CharacterController handles gravity internally if a
        /// collider is present on the environment.  Should only be called on
        /// the server.
        /// </summary>
        public void Move(Vector3 direction, float dt)
        {
            if (!IsServer || _stats == null) return;
            Vector3 flatDir = new Vector3(direction.x, 0f, direction.z);
            if (flatDir.sqrMagnitude > 1e-5f)
            {
                flatDir.Normalize();
                _characterController.Move(flatDir * _stats.moveSpeed * dt);
            }
        }

        /// <summary>
        /// Rotates the AI to face the given world position.  Rotation is
        /// performed gradually based on CharacterStats.rotationSpeed.  Should
        /// only be called on the server.
        /// </summary>
        public void FaceTowards(Vector3 position, float dt)
        {
            if (!IsServer || _stats == null) return;
            Vector3 dir = new Vector3(position.x - transform.position.x, 0f, position.z - transform.position.z);
            if (dir.sqrMagnitude < 1e-5f) return;
            dir.Normalize();
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _stats.rotationSpeed * dt);
        }
        #endregion

        #region Attack Helpers
        /// <summary>
        /// Performs a melee attack by checking for NetworkHealth components
        /// within melee range and applying damage.  Only damages targets on
        /// opposing teams.  Returns true if at least one valid hit occurred.
        /// </summary>
        public bool PerformMeleeAttack()
        {
            if (!IsServer || _stats == null) return false;
            bool hit = false;
            float range = _config != null ? _config.meleeRange : 2f;
            Collider[] results = Physics.OverlapSphere(transform.position, range);
            foreach (var col in results)
            {
                if (col.gameObject == gameObject) continue;
                NetworkHealth targetHealth = col.GetComponent<NetworkHealth>();
                HealthNetwork targetHealthLegacy = col.GetComponent<HealthNetwork>();
                TeamId targetTeam = col.GetComponent<TeamId>();
                TeamId myTeam = GetComponent<TeamId>();
                if ((targetHealth != null || targetHealthLegacy != null) && targetTeam != null && myTeam != null && targetTeam.team != myTeam.team)
                {
                    if (targetHealth != null) targetHealth.TakeDamageServerRpc(10, OwnerClientId);
                    else targetHealthLegacy.Damage(10);
                    hit = true;
                }
            }
            if (hit) OnSuccessfulHit();
            else OnFailedHit();
            return hit;
        }

        /// <summary>
        /// Spawns a projectile and sets its initial velocity in the forward
        /// direction of the muzzle.  Only executed on the server.  Returns
        /// true if the projectile was successfully spawned.
        /// </summary>
        public bool PerformRangedAttack()
        {
            if (!IsServer || projectilePrefab == null || muzzle == null) return false;
            GameObject go = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);
            NetworkObject no = go.GetComponent<NetworkObject>();
            ProjectileServer proj = go.GetComponent<ProjectileServer>();
            if (no == null || proj == null)
            {
                Debug.LogWarning($"{name}: Projectile prefab missing NetworkObject or ProjectileServer.");
                Destroy(go);
                return false;
            }
            // Set owner and team on the projectile.
            proj.ownerClientId = OwnerClientId;
            TeamId myTeam = GetComponent<TeamId>();
            if (myTeam != null)
            {
                proj.ownerTeam = myTeam.team;
            }
            no.Spawn(true);
            proj.Launch();
            return true;
        }
        #endregion
    }
}