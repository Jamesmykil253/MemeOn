using UnityEngine;
using System.Collections;
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

    [Header("Input filtering")]
    [Range(0f, 1f)] public float inputDeadZone = 0.15f;

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
    // Jump assist state (server)
    float _lastGroundedTime;
    float _jumpBufferTimer;

    [Header("Jumping")]
    [Min(0.1f)] public float firstJumpHeight = 1.5f; // meters per spec
    [Min(0.1f)] public float doubleJumpHeight = 1.5f; // base double jump height (bonus applied at apex)
    [Tooltip("Extra height added if double-jump is pressed near the apex of first jump.")]
    public float doubleJumpApexBonus = 0.75f;
    [Tooltip("Seconds after upward velocity crosses zero that still count as apex window.")]
    public float apexWindowSeconds = 0.12f;
    [Header("Jump Assist (optional)")]
    [Tooltip("Enable coyote time and jump input buffering for more forgiving jumps.")]
    public bool enableJumpAssist = false;
    [Tooltip("Grace period after leaving ground where a ground jump is still allowed.")]
    public float coyoteTimeSeconds = 0.1f;
    [Tooltip("If jump is pressed slightly before landing, execute within this buffer window.")]
    public float jumpBufferSeconds = 0.1f;
    [Header("Audit Logs")]
    [SerializeField] bool auditLogs = true;
    int _auditJumpPressCount;
    int _auditAttackPressCount;
    int _auditAttackSentCount;
    int _serverJumpAcceptedCount;
    [Header("Debug")]
    [SerializeField] bool debugLogs = false;
    [SerializeField] bool verboseMovement = false;
    bool _loggedOwnerNonZero;
    bool _loggedServerNonZero;

    [Header("Grounding")]
    [Tooltip("Server: on spawn, raycast down and snap to ground.")]
    public bool snapToGroundOnSpawn = false;
    [Tooltip("Extra vertical offset above hit point when snapping (added to CharacterController.skinWidth).")]
    public float snapSkinOffset = 0.05f;
    [Tooltip("Max distance to search for ground below when snapping.")]
    public float maxGroundSnapDistance = 100f;
    [Tooltip("Layers considered as ground when snapping.")]
    public LayerMask groundLayers = ~0;

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

                // Legacy PlayerController has been deprecated; no need to check/disable here.
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

            // Server optionally snaps player to ground shortly after spawn
            if (IsServer && snapToGroundOnSpawn)
            {
                StartCoroutine(SnapToGroundNextFixed());
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
    float _attackBufferTimer;
    float _jumpBufferClientTimer;
    void Update()
    {
        if (!IsOwner) return;
        // Capture button presses in Update for maximum responsiveness
        if (attackAction != null || _attackFallback != null)
        {
            bool pressed = attackAction != null ? attackAction.action.WasPressedThisFrame() : _attackFallback.WasPressedThisFrame();
            if (pressed)
            {
                _attackBufferTimer = 0.15f; // small buffer allows FixedUpdate to catch it
                if (auditLogs) { _auditAttackPressCount++; Debug.Log($"AUDIT PlayerMovement(Client): Attack pressed count={_auditAttackPressCount} (Update)"); }
            }
        }
        if (jumpAction != null || _jumpFallback != null)
        {
            bool pressed = jumpAction != null ? jumpAction.action.WasPressedThisFrame() : _jumpFallback.WasPressedThisFrame();
            if (pressed)
            {
                _jumpBufferClientTimer = 0.15f;
                if (auditLogs) { _auditJumpPressCount++; Debug.Log($"AUDIT PlayerMovement(Client): Jump pressed count={_auditJumpPressCount} (Update)"); }
            }
        }
    }

    void FixedUpdate()
        {
            if (IsOwner)
            {
        var mv = moveAction != null ? moveAction.action.ReadValue<Vector2>() : (_moveFallback != null ? _moveFallback.ReadValue<Vector2>() : Vector2.zero);
        // Client-side filtering to reduce drift from tiny inputs (e.g., controller noise)
        if (mv.sqrMagnitude < inputDeadZone * inputDeadZone) mv = Vector2.zero;
        if (mv.sqrMagnitude > 1f) mv.Normalize();
                if (debugLogs && !_loggedOwnerNonZero && mv.sqrMagnitude > 0.0001f)
                {
                    _loggedOwnerNonZero = true;
                    Debug.Log($"PlayerMovement: Owner input detected {mv}");
                }
        SubmitInputServerRpc(mv);

                // Attack input: trigger a server RPC on the combat controller with a small cooldown
                if (attackAction != null || _attackFallback != null)
                {
                    _fireCooldownTimer -= Time.fixedDeltaTime;
                    _attackBufferTimer -= Time.fixedDeltaTime;
                    bool pressed = _attackBufferTimer > 0f; // buffered in Update
                    if (pressed && auditLogs)
                    {
                        _auditAttackPressCount++;
                        Debug.Log($"AUDIT PlayerMovement(Client): Attack pressed count={_auditAttackPressCount} cooldown={(Mathf.Max(0f,_fireCooldownTimer)):F2}");
                    }
                    if (pressed && _fireCooldownTimer <= 0f)
                    {
                        var combat = GetComponent<PlayerCombatController>();
                        if (auditLogs) { _auditAttackSentCount++; Debug.Log($"AUDIT PlayerMovement(Client): Attack sent via RPC count={_auditAttackSentCount}"); }
                        combat?.FireServerRpc();
                        _fireCooldownTimer = 0.2f;
                        _attackBufferTimer = 0f;
                    }
                    else if (pressed && _fireCooldownTimer > 0f && auditLogs)
                    {
                        Debug.Log($"AUDIT PlayerMovement(Client): Attack blocked by cooldown remaining={_fireCooldownTimer:F2}s");
                    }
                }

                // Jump input: request jump (server validates ground/double jump)
                _jumpBufferClientTimer -= Time.fixedDeltaTime;
                bool jumpPressed = _jumpBufferClientTimer > 0f;
                if (jumpPressed)
                {
                    if (auditLogs) { _auditJumpPressCount++; Debug.Log($"AUDIT PlayerMovement(Client): Jump pressed count={_auditJumpPressCount}"); }
                    RequestJumpServerRpc();
                    _jumpBufferClientTimer = 0f;
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
                    // Notify clients grounded state change
                    SetGroundedClientRpc(true);
                }
                else if (!_cc.isGrounded && _wasGrounded)
                {
                    SetGroundedClientRpc(false);
                }
                _wasGrounded = _cc.isGrounded;

                // Track last time grounded for coyote window
                if (_cc.isGrounded)
                {
                    _lastGroundedTime = Time.time;
                }

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

                // Jump assist: execute buffered jump upon landing or within coyote window
                if (enableJumpAssist && _jumpBufferTimer > 0f)
                {
                    _jumpBufferTimer -= Time.fixedDeltaTime;
                    bool withinCoyote = (Time.time - _lastGroundedTime) <= coyoteTimeSeconds;
                    if (_cc.isGrounded || withinCoyote)
                    {
                        float g2 = Mathf.Abs(gravity);
                        float jumpVel2 = Mathf.Sqrt(2f * g2 * Mathf.Max(0.1f, firstJumpHeight));
                        _serverVelocity.y = jumpVel2;
                        _canDoubleJump = true;
                        _serverJumpAcceptedCount++;
                        if (debugLogs || auditLogs) Debug.Log($"AUDIT PlayerMovement(Server): Buffered Jump executed (ground) total={_serverJumpAcceptedCount}");
                        _jumpBufferTimer = 0f;
                    }
                    else if (_jumpBufferTimer <= 0f)
                    {
                        if (debugLogs || auditLogs) Debug.Log("AUDIT PlayerMovement(Server): Buffered Jump expired without execution");
                        _jumpBufferTimer = 0f;
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

                // Watchdog: if no input received for a short time, zero client move to avoid stale drift.
                if (Time.time - _lastInputReceiveTime > 0.35f && _lastClientMove.sqrMagnitude > 0f)
                {
                    _lastClientMove = Vector2.zero;
                }
            }
        }

    [ServerRpc(RequireOwnership = false)]
        public void SubmitInputServerRpc(Vector2 move, ServerRpcParams rpcParams = default)
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
            // Sanitize input: deadzone and clamp to unit circle
            if (move.sqrMagnitude < inputDeadZone * inputDeadZone) move = Vector2.zero;
            if (move.sqrMagnitude > 1f) move.Normalize();
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

        bool withinCoyoteTime = enableJumpAssist && ((Time.time - _lastGroundedTime) <= coyoteTimeSeconds);
        if (_cc.isGrounded || withinCoyoteTime)
        {
            _serverVelocity.y = jumpVel;
            _canDoubleJump = true; // allow one more in air
            _serverJumpAcceptedCount++;
            if (debugLogs || auditLogs) Debug.Log($"AUDIT PlayerMovement(Server): Jump accepted (ground/coyote={withinCoyoteTime}) total={_serverJumpAcceptedCount}");
            FlashJumpClientRpc(false);
        }
        else if (_canDoubleJump)
        {
            float bonus = _apexWindowOpen ? Mathf.Sqrt(2f * g * Mathf.Max(0f, doubleJumpApexBonus)) : 0f;
            _serverVelocity.y = doubleBaseVel + bonus;
            _canDoubleJump = false;
            _serverJumpAcceptedCount++;
            if (debugLogs || auditLogs) Debug.Log($"AUDIT PlayerMovement(Server): Jump accepted (double) apexWindow={_apexWindowOpen} total={_serverJumpAcceptedCount}");
            FlashJumpClientRpc(true);
        }
        else
        {
            if (enableJumpAssist)
            {
                _jumpBufferTimer = Mathf.Max(_jumpBufferTimer, jumpBufferSeconds);
                if (debugLogs || auditLogs) Debug.Log($"AUDIT PlayerMovement(Server): Jump queued (buffer) for up to {jumpBufferSeconds:F2}s; grounded={_cc.isGrounded} canDouble={_canDoubleJump}");
            }
            else
            {
                if (debugLogs || auditLogs)
                {
                    Debug.Log($"AUDIT PlayerMovement(Server): Jump rejected grounded={_cc.isGrounded} canDouble={_canDoubleJump} velY={_serverVelocity.y:F2}");
                }
            }
        }

    }

    [ClientRpc]
    private void SetGroundedClientRpc(bool grounded)
    {
        var sync = GetComponent<MemeArena.Debugging.PlayerStateColorSync>();
        sync?.SetGrounded(grounded);
    }

    [ClientRpc]
    private void FlashJumpClientRpc(bool isDouble)
    {
        var sync = GetComponent<MemeArena.Debugging.PlayerStateColorSync>();
        sync?.FlashJump(isDouble);
    }
    /// <summary>Apply slow/heal effects externally via zones. 1 = normal.</summary>
        public void SetExternalSpeedMultiplier(float mult)
        {
            if (!IsServer) return;
            _externalSpeedMultiplier = Mathf.Clamp(mult, 0.1f, 3f);
        }

        IEnumerator SnapToGroundNextFixed()
        {
            // Wait one fixed step to ensure controller is initialized and transforms settled
            yield return new WaitForFixedUpdate();
            if (_cc == null) yield break;

            Vector3 origin = transform.position + Vector3.up * 0.25f;
            if (Physics.Raycast(origin, Vector3.down, out var hit, maxGroundSnapDistance, groundLayers, QueryTriggerInteraction.Ignore))
            {
                bool wasEnabled = _cc.enabled;
                if (wasEnabled) _cc.enabled = false;
                float y = hit.point.y + _cc.skinWidth + snapSkinOffset;
                var pos = transform.position;
                pos.y = y;
                transform.position = pos;
                if (wasEnabled) _cc.enabled = true;
                if (debugLogs) Debug.Log($"PlayerMovement: Snapped to ground at {hit.point} (set y={y:F2})");
            }
            else
            {
                if (debugLogs) Debug.LogWarning("PlayerMovement: SnapToGround did not find ground below within range.");
            }
        }
    }
}