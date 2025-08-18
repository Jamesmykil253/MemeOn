using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.Serialization;

// Health binding removed; camera no longer depends on NetworkHealth

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
    public enum RotationSource { PitchYaw, Transform }
    [Tooltip("Choose how the camera's orbit rotation is determined (fields vs current Transform).")]
    public RotationSource rotationSource = RotationSource.PitchYaw;
    [FormerlySerializedAs("useTransformRotation")][HideInInspector]
    public bool useTransformRotation = false; // Back-compat: migrated to rotationSource in OnValidate
    [Tooltip("If true and RotationSource is PitchYaw, forces the camera to look at the target each frame.")]
    public bool lockLookAtTarget = true;

        // Live pan removed for simplicity; camera follows target and optional manual pan is available.

    [Header("Manual free pan (optional)")]
    [Tooltip("Hold Move input to nudge the camera position manually. Not tied to health/death.")]
    public bool enableManualFreePan = false;
    public InputActionReference moveAction;   // Vector2 ("Move") optional; falls back to generated actions
    public InputActionReference jumpAction;   // kept for future
    public float freePanSpeed = 12f;
    public float freeElevateSpeed = 8f;
    public float freePanDamping = 10f;
    [Range(0f, 1f)] public float inputDeadZone = 0.15f;

    private float _zoom = 8f;
    private Vector3 _livePanOffset;   // offset added on top of follow offset while alive
    private Vector3 _freeCamVelocity; // for smooth damp in free mode

        // Fallback actions if references are not wired
        private InputSystem_Actions _actions;
        private InputAction _moveFallback;
    private InputAction _jumpFallback;

        [Header("Debug")] public bool debugLogs = false;

        private void Update()
        {
            // NOTE: Use Input System for gameplay; here we only use legacy mouse wheel for editor convenience if available.
            try
            {
                float scroll = UnityEngine.Input.mouseScrollDelta.y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    _zoom = Mathf.Clamp(_zoom - scroll * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
                }
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

            // Optional manual free pan any time (not tied to health)
            if (enableManualFreePan)
            {
                var move = ReadMove();
                if (move.sqrMagnitude < inputDeadZone * inputDeadZone) move = Vector2.zero;
                Vector3 desiredDelta = Vector3.zero;
                if (Mathf.Abs(move.x) > 0.001f)
                {
                    var right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
                    desiredDelta += right * (move.x * freePanSpeed * Time.deltaTime);
                }
                if (Mathf.Abs(move.y) > 0.001f)
                {
                    desiredDelta += Vector3.up * (move.y * freeElevateSpeed * Time.deltaTime);
                }
                if (desiredDelta.sqrMagnitude > 0f)
                {
                    var targetPos = transform.position + desiredDelta;
                    transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Exp(-freePanDamping * Time.deltaTime));
                }
            }

            // Follow mode (alive) — simple follow, no live-pan
            Vector3 followAnchor = target.position;
            _livePanOffset = Vector3.zero;

            var camRot = rotationSource == RotationSource.Transform ? transform.rotation : Quaternion.Euler(pitchDeg, yawDeg, 0f);
            var desiredFollow = followAnchor + (camRot * offset.normalized) * _zoom + _livePanOffset;
            transform.position = Vector3.Lerp(transform.position, desiredFollow, 1f - Mathf.Exp(-smooth * Time.deltaTime));
            if (rotationSource == RotationSource.PitchYaw && lockLookAtTarget)
            {
                transform.LookAt(followAnchor + _livePanOffset);
            }
        }

        private void EnsureInputs()
        {
            // If any action refs are used, ensure they're enabled
            if (moveAction != null && !moveAction.action.enabled) moveAction.action.Enable();
            if (jumpAction != null && !jumpAction.action.enabled) jumpAction.action.Enable();

            if (_actions == null && (moveAction == null))
            {
                _actions = new InputSystem_Actions();
                _actions.Player.Enable();
                _actions.UI.Enable();
                if (moveAction == null) _moveFallback = _actions.Player.Move;
                if (jumpAction == null) _jumpFallback = _actions.Player.Jump;
                if (debugLogs) Debug.Log("UniteCameraController: Using InputSystem_Actions fallback (Move/Jump).");
            }
        }

        private Vector2 ReadMove()
        {
            if (moveAction != null) return moveAction.action.ReadValue<Vector2>();
            if (_moveFallback != null) return _moveFallback.ReadValue<Vector2>();
            return Vector2.zero;
        }

        private void OnEnable()
        {
            // Nothing to bind; health is not used to control camera
        }

        private void OnDisable()
        {
            if (_actions != null)
            {
                _actions.Player.Disable();
                _actions.UI.Disable();
                _actions.Dispose();
                _actions = null;
                _moveFallback = null; _jumpFallback = null;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            _livePanOffset = Vector3.zero;
        }
        private void OnValidate()
        {
            // Migrate legacy bool to enum for convenience if users toggle it in inspector
            if (useTransformRotation)
            {
                rotationSource = RotationSource.Transform;
            }
        }
    // Health bindings removed; camera no longer reacts to health changes
    }
}
