using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

using MemeArena.Combat; // NetworkHealth

namespace MemeArena.CameraSystem
{
    public class UniteCameraController : MonoBehaviour
    {
        [Header("Follow settings")]
        public Transform target;
        public Vector3 offset = new Vector3(0, 6, -6);
        [Min(0.01f)] public float smooth = 10f;
        public float zoomSpeed = 5f;
        public float minZoom = 4f;
        public float maxZoom = 12f;
    [Range(-89f, 89f)] public float pitchDeg = 35f;
    [Range(-180f, 180f)] public float yawDeg = 0f;

        [Header("Live pan (while alive)")]
        [Tooltip("If true, holding the pan modifier or using Look input allows panning the camera without detaching from the player.")]
        public bool enableLivePan = true;
        [Tooltip("Optional input to require while panning (e.g., Right Mouse Button). If null, Look alone enables panning.")]
        public InputActionReference panModifierAction; // optional
        public InputActionReference lookAction; // Vector2 ("Look") optional; falls back to generated actions
        public float livePanHorizontalSpeed = 10f; // world-space XZ along camera right/forward (horizontal only)
        public float livePanVerticalSpeed = 6f;    // affects Y only (Look.y -> Y), matching spec for vertical arena
        public float livePanMaxRadius = 6f;        // clamp horizontal pan from player
        public float livePanRecenterSpeed = 5f;    // spring back to player when not panning

        [Header("Free cam on death")]
        [Tooltip("When the target dies, detach and allow free camera motion controlled by input.")]
        public bool freeCamOnDeath = true;
        public InputActionReference moveAction;   // Vector2 ("Move") optional; falls back to generated actions
        public InputActionReference jumpAction;   // Button ("Jump") optional; not mandatory for spec but kept for future
        public float freePanSpeed = 12f;          // applies to X using Move.x
        public float freeElevateSpeed = 8f;       // applies to Y using Move.y (per spec: up/down -> Y axis)
        public float freePanDamping = 10f;        // smoothing for free cam motion

        private float _zoom = 8f;
        private Vector3 _livePanOffset;   // offset added on top of follow offset while alive
        private Vector3 _freeCamVelocity; // for smooth damp in free mode
        private bool _targetIsDead;
        private NetworkHealth _observedHealth;

        // Fallback actions if references are not wired
        private InputSystem_Actions _actions;
        private InputAction _lookFallback;
        private InputAction _moveFallback;
    private InputAction _jumpFallback;
    private InputAction _panModFallback; // Use Player.Sprint as pan modifier (Left Shift)

        [Header("Debug")] public bool debugLogs = false;

        private void Update()
        {
            // NOTE: Use Input System for gameplay; here we only use legacy mouse wheel for editor convenience if available.
            try
            {
                _zoom = Mathf.Clamp(_zoom - UnityEngine.Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
            }
            catch { /* Ignore if legacy input disabled */ }
        }

        private void LateUpdate()
        {
            // Lazily ensure inputs available
            EnsureInputs();

            if (!target)
            {
                // No target: just freeze in place
                return;
            }

            if (freeCamOnDeath && _targetIsDead)
            {
                // Free camera mode: Move.x => world/camera-right X, Move.y => world Y (per spec)
                var move = ReadMove();

                // Horizontal along camera right axis
                Vector3 desiredDelta = Vector3.zero;
                if (Mathf.Abs(move.x) > 0.001f)
                {
                    var right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
                    desiredDelta += right * (move.x * freePanSpeed * Time.deltaTime);
                }
                // Vertical along world up using Move.y
                if (Mathf.Abs(move.y) > 0.001f)
                {
                    desiredDelta += Vector3.up * (move.y * freeElevateSpeed * Time.deltaTime);
                }

                // Smooth towards new position
                var targetPos = transform.position + desiredDelta;
                transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-freePanDamping * Time.deltaTime));

                // Look at the last known target position to keep context
                transform.LookAt(target.position);
                return;
            }

            // Follow mode (alive)
            Vector3 followAnchor = target.position;

            // Live pan support: allow panning relative to target without detaching
            if (enableLivePan)
            {
                bool mod = ReadPanModifier();
                Vector2 look = ReadLook();
                if (mod || (!panModifierAction && look.sqrMagnitude > 0.0001f))
                {
                    // Horizontal pan on camera right (X component), ignore forward to avoid changing Z per spec
                    if (Mathf.Abs(look.x) > 0.001f)
                    {
                        var right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
                        _livePanOffset += right * (look.x * livePanHorizontalSpeed * Time.deltaTime);
                    }
                    // Vertical pan (Look.y → world Y)
                    if (Mathf.Abs(look.y) > 0.001f)
                    {
                        _livePanOffset += Vector3.up * (look.y * livePanVerticalSpeed * Time.deltaTime);
                    }

                    // Clamp horizontal radius from player on XZ plane
                    Vector2 horiz = new Vector2(_livePanOffset.x, _livePanOffset.z);
                    if (horiz.magnitude > livePanMaxRadius)
                    {
                        horiz = horiz.normalized * livePanMaxRadius;
                        _livePanOffset.x = horiz.x; _livePanOffset.z = horiz.y;
                    }
                }
                else
                {
                    // Recenter with spring
                    _livePanOffset = Vector3.Lerp(_livePanOffset, Vector3.zero, 1f - Mathf.Exp(-livePanRecenterSpeed * Time.deltaTime));
                }
            }

            var camRot = Quaternion.Euler(pitchDeg, yawDeg, 0f);
            var desiredFollow = followAnchor + (camRot * offset.normalized) * _zoom + _livePanOffset;
            transform.position = Vector3.Lerp(transform.position, desiredFollow, 1f - Mathf.Exp(-smooth * Time.deltaTime));
            transform.LookAt(followAnchor + _livePanOffset);
        }

        private void EnsureInputs()
        {
            // If any action refs are used, ensure they're enabled
            if (lookAction != null && !lookAction.action.enabled) lookAction.action.Enable();
            if (moveAction != null && !moveAction.action.enabled) moveAction.action.Enable();
            if (jumpAction != null && !jumpAction.action.enabled) jumpAction.action.Enable();
            if (panModifierAction != null && !panModifierAction.action.enabled) panModifierAction.action.Enable();

            if (_actions == null && (lookAction == null || moveAction == null || panModifierAction == null))
            {
                _actions = new InputSystem_Actions();
                _actions.Player.Enable();
                _actions.UI.Enable();
                if (lookAction == null) _lookFallback = _actions.Player.Look;
                if (moveAction == null) _moveFallback = _actions.Player.Move;
                if (jumpAction == null) _jumpFallback = _actions.Player.Jump;
                if (panModifierAction == null) _panModFallback = _actions.Player.Sprint; // Left Shift
                if (debugLogs) Debug.Log("UniteCameraController: Using InputSystem_Actions fallback (Look/Move/Jump + Sprint as PanModifier).");
            }
        }

        private Vector2 ReadLook()
        {
            if (lookAction != null) return lookAction.action.ReadValue<Vector2>();
            if (_lookFallback != null) return _lookFallback.ReadValue<Vector2>();
            return Vector2.zero;
        }

        private Vector2 ReadMove()
        {
            if (moveAction != null) return moveAction.action.ReadValue<Vector2>();
            if (_moveFallback != null) return _moveFallback.ReadValue<Vector2>();
            return Vector2.zero;
        }

        private bool ReadPanModifier()
        {
            if (panModifierAction != null) return panModifierAction.action.IsPressed();
            if (_panModFallback != null) return _panModFallback.IsPressed();
            return false;
        }

        private void OnEnable()
        {
            // Rebind to target health in case target was set in editor
            BindTargetHealth(target);
        }

        private void OnDisable()
        {
            UnbindTargetHealth();
            if (_actions != null)
            {
                _actions.Player.Disable();
                _actions.UI.Disable();
                _actions.Dispose();
                _actions = null;
                _lookFallback = null; _moveFallback = null; _jumpFallback = null; _panModFallback = null;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            _livePanOffset = Vector3.zero;
            BindTargetHealth(target);
        }

        private void BindTargetHealth(Transform t)
        {
            UnbindTargetHealth();
            if (!t) return;
            _observedHealth = t.GetComponent<NetworkHealth>();
            if (_observedHealth != null)
            {
                _observedHealth.OnHealthChanged += OnObservedHealthChanged;
                _observedHealth.OnDeath += OnObservedDeath;
                // Avoid assuming dead before first replicated value; wait for OnHealthChanged
                _targetIsDead = false;
                if (debugLogs) Debug.Log($"UniteCameraController: Bound to NetworkHealth on {t.name}, awaiting first health value...");
            }
            else
            {
                _targetIsDead = false;
            }
        }

        private void UnbindTargetHealth()
        {
            if (_observedHealth != null)
            {
                _observedHealth.OnHealthChanged -= OnObservedHealthChanged;
                _observedHealth.OnDeath -= OnObservedDeath;
                _observedHealth = null;
            }
        }

        private void OnObservedHealthChanged(int current, int max)
        {
            _targetIsDead = current <= 0;
            if (debugLogs) Debug.Log($"UniteCameraController: Observed target health changed: {current}/{max} → dead={_targetIsDead}");
        }

        private void OnObservedDeath()
        {
            _targetIsDead = true;
            if (debugLogs) Debug.Log("UniteCameraController: Observed target death → entering free camera mode.");
        }
    }
}
