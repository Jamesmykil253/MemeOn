using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;

namespace MemeArena.Players
{
    /// <summary>
    /// Server-authoritative CharacterController movement with client input.
    /// Owner reads input; server applies motion. No NGO-incompatible attributes.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransform))]
    [DisallowMultipleComponent]
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Movement")]
        [Min(0.1f)] public float moveSpeed = 6f;
        [Min(0.1f)] public float rotationSpeedDeg = 720f;
        public float gravity = -25f;

    [Header("Input")]
    public InputActionReference moveAction; // Vector2 action ("Move")
    public InputActionReference attackAction; // Button action ("Attack")

        // Fallback when InputActionReferences are not wired in the inspector.
        InputSystem_Actions _actions; // generated class from Assets/InputSystem_Actions.inputactions
        InputAction _moveFallback;
        InputAction _attackFallback;

        CharacterController _cc;
        Vector3 _serverVelocity;
        Vector2 _lastClientMove;
        float _externalSpeedMultiplier = 1f;
    [Header("Debug")]
    [SerializeField] bool debugLogs = false;
    bool _loggedOwnerNonZero;
    bool _loggedServerNonZero;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // Enable assigned references if present
                if (moveAction != null) moveAction.action.Enable();
                if (attackAction != null) attackAction.action.Enable();

                // Auto-bind fallback if references are missing
                if (moveAction == null || attackAction == null)
                {
                    _actions = new InputSystem_Actions();
                    _actions.Player.Enable();
                    if (moveAction == null) _moveFallback = _actions.Player.Move;
                    if (attackAction == null) _attackFallback = _actions.Player.Attack;
                    Debug.Log("PlayerMovement: Using auto-bound InputSystem_Actions fallback.");
                }

                // Warn about duplicate movement controllers
                var ctrl = GetComponent<PlayerController>();
                if (ctrl != null)
                {
                    Debug.LogWarning("PlayerMovement: PlayerController also present. Disabling legacy controller to avoid conflicts.");
                    ctrl.enabled = false;
                }
                // Ensure NetworkTransform exists for replication; add at runtime if missing.
                var nt = GetComponent<Unity.Netcode.Components.NetworkTransform>();
                if (nt == null)
                {
                    nt = gameObject.AddComponent<Unity.Netcode.Components.NetworkTransform>();
                    Debug.LogWarning("PlayerMovement: NetworkTransform was missing; added at runtime for movement sync. Please add it to the prefab.");
                }
            }

            // General diagnostics for common misconfigurations
            if (!IsOwner)
            {
                Debug.Log("PlayerMovement: This instance is not the owner; it will not read local input.");
            }
            if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient))
            {
                Debug.LogWarning("PlayerMovement: Netcode not running. Start as Host/Server+Client to test movement (server-authoritative).");
            }
            if (!NetworkObject.IsPlayerObject)
            {
                Debug.LogWarning("PlayerMovement: NetworkObject is not marked as PlayerObject. Ensure this prefab is configured as the Player Prefab in NetworkManager or spawned as a player.");
            }
        }

        void OnDisable()
        {
            if (IsOwner)
            {
                if (moveAction != null) moveAction.action.Disable();
                if (attackAction != null) attackAction.action.Disable();
                if (_actions != null)
                {
                    _actions.Player.Disable();
                    _actions.Dispose();
                    _actions = null;
                    _moveFallback = null;
                    _attackFallback = null;
                }
            }
        }

        float _fireCooldownTimer;
        void FixedUpdate()
        {
            if (IsOwner)
            {
                var mv = moveAction != null ? moveAction.action.ReadValue<Vector2>() : (_moveFallback != null ? _moveFallback.ReadValue<Vector2>() : Vector2.zero);
                if (debugLogs && !_loggedOwnerNonZero && mv.sqrMagnitude > 0.0001f)
                {
                    _loggedOwnerNonZero = true;
                    Debug.Log($"PlayerMovement: Owner input detected {mv}");
                }
                SubmitInputServerRpc(mv, Time.fixedDeltaTime);

                // Attack input: trigger a server RPC on the combat controller with a small cooldown
                if (attackAction != null || _attackFallback != null)
                {
                    _fireCooldownTimer -= Time.fixedDeltaTime;
                    bool pressed = attackAction != null ? attackAction.action.WasPressedThisFrame() : _attackFallback.WasPressedThisFrame();
                    if (pressed && _fireCooldownTimer <= 0f)
                    {
                        var combat = GetComponent<PlayerCombatController>();
                        combat?.FireServerRpc();
                        _fireCooldownTimer = 0.2f;
                    }
                }
            }

            if (IsServer)
            {
                // gravity
                if (_cc.isGrounded && _serverVelocity.y < 0f)
                    _serverVelocity.y = -1f;
                else
                    _serverVelocity.y += gravity * Time.fixedDeltaTime;

                Vector3 moveDir = new Vector3(_lastClientMove.x, 0f, _lastClientMove.y);
                if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
                float speed = moveSpeed * _externalSpeedMultiplier;

                Vector3 disp = (moveDir * speed + new Vector3(0f, _serverVelocity.y, 0f)) * Time.fixedDeltaTime;
                _cc.Move(disp);

                if (moveDir.sqrMagnitude > 0.0001f)
                {
                    Quaternion target = Quaternion.LookRotation(moveDir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeedDeg * Time.fixedDeltaTime);
                }
            }
        }

    [ServerRpc]
        public void SubmitInputServerRpc(Vector2 move, float dt)
        {
            _lastClientMove = move;
            if (debugLogs && !_loggedServerNonZero && move.sqrMagnitude > 0.0001f)
            {
                _loggedServerNonZero = true;
                Debug.Log($"PlayerMovement: Server received input {move}");
            }
        }

        /// <summary>Apply slow/heal effects externally via zones. 1 = normal.</summary>
        public void SetExternalSpeedMultiplier(float mult)
        {
            if (!IsServer) return;
            _externalSpeedMultiplier = Mathf.Clamp(mult, 0.1f, 3f);
        }
    }
}