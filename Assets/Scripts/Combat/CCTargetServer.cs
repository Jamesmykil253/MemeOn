// Meme Online Battle Arena - CCTargetServer
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[DisallowMultipleComponent]
public sealed class CCTargetServer : NetworkBehaviour
{
    private NavMeshAgent _agent;
    private bool _rooted;
    private float _rootUntil;

    private bool _stunned;
    private float _stunUntil;

    private void Awake(){ _agent = GetComponent<NavMeshAgent>(); }

    private void Update()
    {
        if (!IsServer) return;
        float t = Time.time;
        if (_rooted && t >= _rootUntil) _rooted = false;
        if (_stunned && t >= _stunUntil) _stunned = false;
        if (_agent) _agent.isStopped = _rooted || _stunned;
    }

    public void ApplyKnockback(Vector3 direction, float distance)
    {
        if (!IsServer || direction.sqrMagnitude < 0.0001f || distance <= 0f) return;
        Vector3 target = transform.position + direction.normalized * distance;
        if (_agent) _agent.Warp(target); else transform.position = target;
    }
    public void ApplyRoot(float duration)
    {
        if (!IsServer) return;
        _rooted = true;
        _rootUntil = Mathf.Max(_rootUntil, Time.time + Mathf.Max(0f, duration));
    }
    public void ApplyStun(float duration)
    {
        if (!IsServer) return;
        _stunned = true;
        _stunUntil = Mathf.Max(_stunUntil, Time.time + Mathf.Max(0f, duration));
    }
}
