using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace MemeArena.Players
{
    /// <summary>
    /// Server-authoritative CharacterController movement with client input.
    /// - Owner client reads Input System "Move" action
    /// - Sends input to server via ServerRpc each FixedUpdate
    /// - Server applies movement (gravity + optional external speed multiplier)
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

        CharacterController _cc;
        Vector3 _serverVelocity;            // server-side vertical velocity
        Vector2 _lastClientMove;            // last received input from owner
        float _externalSpeedMultiplier = 1f; // e.g., enemy goal slow

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
        }

        void OnDisable()
        {
            if (IsOwner && moveAction != null)
            {
                moveAction.action.Disable();
            }
        }

        void FixedUpdate()
        {
            if (IsOwner)
            {
                var mv = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
                SubmitInputServerRpc(mv, Time.fixedDeltaTime);
            }

            if (IsServer)
            {
                // Apply gravity
                if (_cc.isGrounded && _serverVelocity.y < 0f)
                    _serverVelocity.y = -1f; // stick to ground
                else
                    _serverVelocity.y += gravity * Time.fixedDeltaTime;

                // Move horizontal from last client input
                Vector3 moveDir = new Vector3(_lastClientMove.x, 0f, _lastClientMove.y);
                if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
                float speed = moveSpeed * _externalSpeedMultiplier;

                Vector3 disp = (moveDir * speed + new Vector3(0f, _serverVelocity.y, 0f)) * Time.fixedDeltaTime;
                _cc.Move(disp);

                // Rotate toward movement
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

        /// <summary> Apply slow/heal effects externally via zones. 1 = normal. </summary>
        [Server] public void SetExternalSpeedMultiplier(float mult)
        {
            _externalSpeedMultiplier = Mathf.Clamp(mult, 0.1f, 3f);
        }
    }
}
