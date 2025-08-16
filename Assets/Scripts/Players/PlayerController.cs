using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MemeArena.Players
{
    /// <summary>
    /// Handles player movement and input on the client and submits input to the
    /// server for authoritative processing.  When firing, it calls the
    /// FireServerRpc() method on the PlayerCombatController.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        private CharacterController _cc;
        private Vector2 _move;
        private Vector2 _look;
        private bool _fire;

        [Header("Movement Tuning")]
        public float moveSpeed = 4f;
        public float rotationSpeed = 360f;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        /// <summary>
        /// Input callback for movement.  Bound via the Input System.
        /// </summary>
        public void OnMove(InputAction.CallbackContext ctx) => _move = ctx.ReadValue<Vector2>();

        /// <summary>
        /// Input callback for look.  Bound via the Input System.
        /// </summary>
        public void OnLook(InputAction.CallbackContext ctx) => _look = ctx.ReadValue<Vector2>();

        /// <summary>
        /// Input callback for firing.  Sets a flag to fire on the next fixed update.
        /// </summary>
        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) _fire = true;
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                SubmitInputServerRpc(_move, _look, _fire);
                _fire = false;
            }
        }

        /// <summary>
        /// Sends player input to the server for authoritative movement and firing.
        /// </summary>
        [ServerRpc]
        private void SubmitInputServerRpc(Vector2 move, Vector2 look, bool fire)
        {
            var dt = Time.fixedDeltaTime;
            var w = new Vector3(move.x, 0f, move.y) * moveSpeed;
            _cc.Move(w * dt);

            if (w.sqrMagnitude > 0.0001f)
            {
                var targetRot = Quaternion.LookRotation(w.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * dt);
            }

            if (fire)
            {
                var pc = GetComponent<PlayerCombatController>();
                if (pc != null)
                {
                    // Call the RPC on the combat controller.  The method name ends
                    // with "ServerRpc" to satisfy NGO conventions.
                    pc.FireServerRpc();
                }
            }
        }
    }
}