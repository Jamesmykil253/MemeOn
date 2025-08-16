// Meme MOBA - DebugColorState
// Simple local color flash for quick visual confirmation. No networking required.
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class DebugColorState : MonoBehaviour
{
    [SerializeField] private float defaultDuration = 0.3f;

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private Color _original;
    private bool _hasOriginal;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>(true);
        _mpb = new MaterialPropertyBlock();
        CacheOriginal();
    }

    private void CacheOriginal()
    {
        if (_renderer == null) return;
        _renderer.GetPropertyBlock(_mpb);
        _original = _mpb.GetColor(BaseColor);
        if (_original == default) _original = Color.white;
        _hasOriginal = true;
    }

    public void SetStateColor(Color c, float duration = -1f)
    {
        if (_renderer == null) return;
        if (!_hasOriginal) CacheOriginal();
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(c, duration < 0f ? defaultDuration : duration));
    }

    private IEnumerator FlashRoutine(Color c, float seconds)
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(BaseColor, c);
        _renderer.SetPropertyBlock(_mpb);

        yield return new WaitForSeconds(seconds);

        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(BaseColor, _original);
        _renderer.SetPropertyBlock(_mpb);
    }
}
