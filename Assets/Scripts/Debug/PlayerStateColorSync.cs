using UnityEngine;
using Unity.Netcode;
using System.Collections;

namespace MemeArena.Debugging
{
    /// <summary>
    /// Client-side visual state colorizer for players. Maintains a persistent base color
    /// for the player's current state and supports short transient flashes with priority.
    /// Uses MaterialPropertyBlock on _BaseColor when possible to avoid material instances.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStateColorSync : NetworkBehaviour
    {
        [Header("Renderer")]
        [Tooltip("Renderer whose material colour will reflect the player's state.")]
        public Renderer targetRenderer;

        [Header("Colors")]
        public Color groundedColor = Color.green;
        public Color airborneColor = Color.yellow;
        public Color jumpColor = Color.blue;
        public Color doubleJumpColor = Color.magenta;
        public Color attackColor = new Color(1f, 0.5f, 0f);
        public Color boostedReadyColor = new Color(1f, 0.84f, 0f); // gold
        public Color deadColor = Color.black;

        [Header("Timing")]
        public float attackFlashSeconds = 0.12f;
        public float jumpFlashSeconds = 0.10f;

        private MaterialPropertyBlock _mpb;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private Color _original;
        private Color _baseStateColor;
        private Coroutine _flashCo;

        private void Awake()
        {
            if (!targetRenderer)
            {
                targetRenderer = GetComponentInChildren<Renderer>(true);
            }
            _mpb = new MaterialPropertyBlock();
            CacheOriginal();
            _baseStateColor = _original == default ? Color.white : _original;
        }

        private void CacheOriginal()
        {
            if (!targetRenderer) return;
            targetRenderer.GetPropertyBlock(_mpb);
            _original = _mpb.GetColor(BaseColorId);
            if (_original == default) _original = Color.white;
        }

        private void SetColour(Color c)
        {
            if (!targetRenderer) return;
            targetRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorId, c);
            targetRenderer.SetPropertyBlock(_mpb);
        }

        private IEnumerator Flash(Color c, float seconds)
        {
            SetColour(c);
            yield return new WaitForSeconds(seconds);
            _flashCo = null;
            SetColour(_baseStateColor);
        }

        // Public API called from other scripts (typically via ClientRpc wrappers)
        public void SetGrounded(bool grounded)
        {
            _baseStateColor = grounded ? groundedColor : airborneColor;
            if (_flashCo == null) SetColour(_baseStateColor);
        }
        public void FlashJump(bool isDouble)
        {
            if (_flashCo != null) StopCoroutine(_flashCo);
            _flashCo = StartCoroutine(Flash(isDouble ? doubleJumpColor : jumpColor, jumpFlashSeconds));
        }
        public void FlashAttack(bool boosted)
        {
            if (_flashCo != null) StopCoroutine(_flashCo);
            var c = boosted ? boostedReadyColor : attackColor;
            _flashCo = StartCoroutine(Flash(c, attackFlashSeconds));
        }
        public void SetBoostReady(bool ready)
        {
            // Persistent overlay via base color until consumed; choose a blended hint
            _baseStateColor = ready ? Color.Lerp(_baseStateColor, boostedReadyColor, 0.6f) : (_baseStateColor == deadColor ? deadColor : _baseStateColor);
            if (_flashCo == null) SetColour(_baseStateColor);
        }
        public void SetDead()
        {
            _baseStateColor = deadColor;
            if (_flashCo != null) { StopCoroutine(_flashCo); _flashCo = null; }
            SetColour(_baseStateColor);
        }
    }
}
