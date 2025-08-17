using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace MemeArena.Players
{
    /// <summary>
    /// Server-authoritative CharacterController movement with client input.
    /// Owner reads input; server applies motion. No NGO-incompatible attributes.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Movement")]
        [Min(0.1f)] public float moveSpeed = 6f;
        [Min(0.1f)] public float rotationSpeedDeg = 720f;
        public float gravity = -25f;

    [Header("Input")]
    public InputActionReference moveAction; // Vector2 action ("Move")
    public InputActionReference attackAction; // Button action ("Attack")

        CharacterController _cc;
        Vector3 _serverVelocity;
        Vector2 _lastClientMove;
        float _externalSpeedMultiplier = 1f;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner && moveAction != null)
            {
                moveAction.action.Enable();
            }
            if (IsOwner && attackAction != null)
            {
                attackAction.action.Enable();
            }
        }

        void OnDisable()
        {
            if (IsOwner && moveAction != null)
            {
                moveAction.action.Disable();
            }
            if (IsOwner && attackAction != null)
            {
                attackAction.action.Disable();
            }
        }

        float _fireCooldownTimer;
        void FixedUpdate()
        {
            if (IsOwner)
            {
                var mv = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
                SubmitInputServerRpc(mv, Time.fixedDeltaTime);

                // Attack input: trigger a server RPC on the combat controller with a small cooldown
                if (attackAction != null)
                {
                    _fireCooldownTimer -= Time.fixedDeltaTime;
                    bool pressed = attackAction.action.WasPressedThisFrame();
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
        void SubmitInputServerRpc(Vector2 move, float dt)
        {
            _lastClientMove = move;
        }

        /// <summary>Apply slow/heal effects externally via zones. 1 = normal.</summary>
        public void SetExternalSpeedMultiplier(float mult)
        {
            if (!IsServer) return;
            _externalSpeedMultiplier = Mathf.Clamp(mult, 0.1f, 3f);
        }
    }
}