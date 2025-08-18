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
    public InputActionReference jumpAction; // Button action ("Jump")

        // Fallback when InputActionReferences are not wired in the inspector.
        InputSystem_Actions _actions; // generated class from Assets/InputSystem_Actions.inputactions
        InputAction _moveFallback;
    InputAction _attackFallback;
    InputAction _jumpFallback;

        CharacterController _cc;
        Vector3 _serverVelocity;
        Vector2 _lastClientMove;
        float _externalSpeedMultiplier = 1f;
    float _lastInputReceiveTime;
    bool _canDoubleJump; // server-owned
    bool _wasGrounded;   // server-owned
    bool _apexWindowOpen; // server-owned
    float _apexWindowTimer; // server-owned

    [Header("Jumping")]
    [Min(0.1f)] public float firstJumpHeight = 1.5f; // meters per spec
    [Min(0.1f)] public float doubleJumpHeight = 1.5f; // base double jump height (bonus applied at apex)
    [Tooltip("Extra height added if double-jump is pressed near the apex of first jump.")]
    public float doubleJumpApexBonus = 0.75f;
    [Tooltip("Seconds after upward velocity crosses zero that still count as apex window.")]
    public float apexWindowSeconds = 0.12f;
    [Header("Debug")]
    [SerializeField] bool debugLogs = false;
    [SerializeField] bool verboseMovement = false;
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
                if (jumpAction != null) jumpAction.action.Enable();

                // Auto-bind fallback if references are missing
                if (moveAction == null || attackAction == null || jumpAction == null)
                {
                    _actions = new InputSystem_Actions();
                    _actions.Player.Enable();
                    if (moveAction == null) _moveFallback = _actions.Player.Move;
                    if (attackAction == null) _attackFallback = _actions.Player.Attack;
                    if (jumpAction == null) _jumpFallback = _actions.Player.Jump;
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
                if (jumpAction != null) jumpAction.action.Disable();
                if (_actions != null)
                {
                    _actions.Player.Disable();
                    _actions.Dispose();
                    _actions = null;
                    _moveFallback = null;
                    _attackFallback = null;
                    _jumpFallback = null;
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

                // Jump input: request jump (server validates ground/double jump)
                bool jumpPressed = false;
                if (jumpAction != null) jumpPressed = jumpAction.action.WasPressedThisFrame();
                else if (_jumpFallback != null) jumpPressed = _jumpFallback.WasPressedThisFrame();
                if (jumpPressed)
                {
                    RequestJumpServerRpc();
                }
            }

            if (IsServer)
            {
                // gravity
                if (_cc.isGrounded && _serverVelocity.y < 0f)
                    _serverVelocity.y = -1f;
                else
                    _serverVelocity.y += gravity * Time.fixedDeltaTime;

                // Grounded state management for double jump reset
                if (_cc.isGrounded && !_wasGrounded)
                {
                    _canDoubleJump = true; // touching ground restores double-jump availability
                    _apexWindowOpen = false;
                    _apexWindowTimer = 0f;
                }
                _wasGrounded = _cc.isGrounded;

                // Detect apex crossing: when vertical velocity changes from >0 to <=0 while airborne
                if (!_cc.isGrounded)
                {
                    // Use last frame est by checking sign change — approximate by opening window once we start falling
                    if (_serverVelocity.y <= 0f && !_apexWindowOpen)
                    {
                        _apexWindowOpen = true;
                        _apexWindowTimer = apexWindowSeconds;
                    }
                    if (_apexWindowOpen)
                    {
                        _apexWindowTimer -= Time.fixedDeltaTime;
                        if (_apexWindowTimer <= 0f) _apexWindowOpen = false;
                    }
                }

                Vector3 moveDir = new Vector3(_lastClientMove.x, 0f, _lastClientMove.y);
                if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
                float speed = moveSpeed * _externalSpeedMultiplier;

                Vector3 disp = (moveDir * speed + new Vector3(0f, _serverVelocity.y, 0f)) * Time.fixedDeltaTime;
                _cc.Move(disp);
                if (verboseMovement)
                {
                    Debug.Log($"PlayerMovement(Server): moveDir={moveDir} speed={speed} disp={disp} grounded={_cc.isGrounded}");
                }

                if (moveDir.sqrMagnitude > 0.0001f)
                {
                    Quaternion target = Quaternion.LookRotation(moveDir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeedDeg * Time.fixedDeltaTime);
                }

                // Watchdog: if no input received for > 1s, zero client move to avoid stale drift.
                if (Time.time - _lastInputReceiveTime > 1.0f && _lastClientMove.sqrMagnitude > 0f)
                {
                    _lastClientMove = Vector2.zero;
                }
            }
        }

    [ServerRpc(RequireOwnership = false)]
        public void SubmitInputServerRpc(Vector2 move, float dt, ServerRpcParams rpcParams = default)
        {
            // Validate the caller is the owner of this object to prevent spoofing.
            if (NetworkObject == null)
                return;
            if (rpcParams.Receive.SenderClientId != NetworkObject.OwnerClientId)
            {
                if (debugLogs)
                    Debug.LogWarning($"PlayerMovement: Ignoring input from non-owner {rpcParams.Receive.SenderClientId} (owner={NetworkObject.OwnerClientId}).");
                return;
            }
            _lastClientMove = move;
            _lastInputReceiveTime = Time.time;
            if (debugLogs && !_loggedServerNonZero && move.sqrMagnitude > 0.0001f)
            {
                _loggedServerNonZero = true;
                Debug.Log($"PlayerMovement: Server received input {move}");
            }
        }

    [ServerRpc(RequireOwnership = false)]
    private void RequestJumpServerRpc(ServerRpcParams rpcParams = default)
    {
        if (NetworkObject == null) return;
        if (rpcParams.Receive.SenderClientId != NetworkObject.OwnerClientId)
        {
            if (debugLogs) Debug.LogWarning($"PlayerMovement: Ignoring jump from non-owner {rpcParams.Receive.SenderClientId} (owner={NetworkObject.OwnerClientId}).");
            return;
        }
        if (!IsServer) return;

        float g = Mathf.Abs(gravity);
        float jumpVel = Mathf.Sqrt(2f * g * Mathf.Max(0.1f, firstJumpHeight));
        float doubleBaseVel = Mathf.Sqrt(2f * g * Mathf.Max(0.1f, doubleJumpHeight));

        if (_cc.isGrounded)
        {
            _serverVelocity.y = jumpVel;
            _canDoubleJump = true; // allow one more in air
            if (debugLogs) Debug.Log("PlayerMovement(Server): Jump (ground)");
        }
        else if (_canDoubleJump)
        {
            float bonus = _apexWindowOpen ? Mathf.Sqrt(2f * g * Mathf.Max(0f, doubleJumpApexBonus)) : 0f;
            _serverVelocity.y = doubleBaseVel + bonus;
            _canDoubleJump = false;
            if (debugLogs) Debug.Log("PlayerMovement(Server): Double jump");
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