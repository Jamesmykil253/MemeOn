// Meme Online Battle Arena - NetworkDamageFlash
using UnityEngine;
using Unity.Netcode;
using System.Collections;

[DisallowMultipleComponent]
public sealed class NetworkDamageFlash : NetworkBehaviour
{
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private float flashSeconds = 0.12f;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _mpb;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private Color[] _originals;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _mpb = new MaterialPropertyBlock();
        _originals = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (!r) continue;
            r.GetPropertyBlock(_mpb);
            _originals[i] = _mpb.GetColor(BaseColorId);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) CombatEvents.OnDamageReceived += OnDamageReceivedServer;
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer) CombatEvents.OnDamageReceived -= OnDamageReceivedServer;
    }

    private void OnDamageReceivedServer(ulong victimId, ulong attackerId, float dmg)
    {
        if (!IsServer) return;
        if (victimId == OwnerClientId) FlashClientRpc();
    }

    [ClientRpc] private void FlashClientRpc()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i]; if (!r) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorId, flashColor);
            r.SetPropertyBlock(_mpb);
        }
        yield return new WaitForSeconds(flashSeconds);
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i]; if (!r) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorId, _originals[i]);
            r.SetPropertyBlock(_mpb);
        }
    }
}
