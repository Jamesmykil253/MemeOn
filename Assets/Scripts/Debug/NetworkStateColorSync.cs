using UnityEngine;
using Unity.Netcode;
using MemeArena.AI;

namespace MemeArena.Debugging
{
    /// <summary>
    /// NetworkStateColorSync is a simple client-side helper that listens to an AIController
    /// and updates a material colour based on its current state. This provides an
    /// at-a-glance visualisation of the AI finite state machine when running in
    /// multiplayer. It relies on the AIController exposing its CurrentStateName.
    /// </summary>
    [DisallowMultipleComponent]
    public class NetworkStateColorSync : NetworkBehaviour
    {
        [Tooltip("Renderer whose material colour will reflect the AI's current state.")]
        public Renderer targetRenderer;
        [Header("State Colours")]
        public Color idleColor = Color.gray;
        public Color alertColor = Color.yellow;
        public Color pursueColor = Color.red;
        public Color meleeColor = new Color(1f, 0.5f, 0f);
        public Color rangedColor = Color.cyan;
        public Color evadeColor = Color.green;
        public Color stunColor = Color.blue;
        public Color returnColor = Color.magenta;
        public Color deadColor = Color.black;

        private AIController _controller;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // Only clients need to visualise state; server doesn't run this.
            if (!IsClient)
                return;
            _controller = GetComponent<AIController>();
        }

        private void Update()
        {
            if (!IsClient || _controller == null || targetRenderer == null)
                return;
            // Choose a colour based on the current state name.
            string state = _controller.CurrentStateName;
            Color colour = idleColor;
            switch (state)
            {
                case nameof(IdleState):
                    colour = idleColor;
                    break;
                case nameof(AlertState):
                    colour = alertColor;
                    break;
                case nameof(PursueState):
                    colour = pursueColor;
                    break;
                case nameof(MeleeAttackState):
                    colour = meleeColor;
                    break;
                case nameof(RangedAttackState):
                    colour = rangedColor;
                    break;
                case nameof(EvadeState):
                    colour = evadeColor;
                    break;
                case nameof(StunnedState):
                    colour = stunColor;
                    break;
                case nameof(ReturnToSpawnState):
                    colour = returnColor;
                    break;
                case nameof(DeadState):
                    colour = deadColor;
                    break;
            }
            // Apply the colour to the first material on the renderer.
            var mat = targetRenderer.material;
            mat.color = colour;
        }
    }
}