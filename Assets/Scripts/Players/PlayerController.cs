using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MemeArena.Players
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        private CharacterController _cc;
        private Vector2 _move;
        private Vector2 _look;
        private bool _fire;

        [Header("Tuning")]
        public float moveSpeed = 4f;
        public float rotationSpeed = 360f;

        private void Awake() { _cc = GetComponent<CharacterController>(); }

        public void OnMove(InputAction.CallbackContext ctx) => _move = ctx.ReadValue<Vector2>();
        public void OnLook(InputAction.CallbackContext ctx) => _look = ctx.ReadValue<Vector2>();
        public void OnFire(InputAction.CallbackContext ctx) { if (ctx.performed) _fire = true; }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                SubmitInputServerRpc(_move, _look, _fire);
                _fire = false;
            }
        }

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
                if (pc) pc.ServerFire();
            }
        }
    }
}
