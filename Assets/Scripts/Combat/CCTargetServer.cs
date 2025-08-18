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

    [Header("NavMesh safety")]
    [SerializeField] private bool autoSnapToNavMesh = true;
    [SerializeField] private float navMeshSnapRadius = 4f;
    [SerializeField] private bool logNavMeshWarnings = false;

    private bool _warnedNoNavmesh;

    private void Awake(){ _agent = GetComponent<NavMeshAgent>(); }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        // Try to ensure we're placed on a NavMesh at spawn to avoid Agent errors.
        TryEnsureOnNavMesh();
    }

    private void Update()
    {
        if (!IsServer) return;
        float t = Time.time;
        if (_rooted && t >= _rootUntil) _rooted = false;
        if (_stunned && t >= _stunUntil) _stunned = false;

        if (_agent && _agent.enabled && _agent.isActiveAndEnabled)
        {
            // Only manipulate isStopped if agent is on a NavMesh; otherwise Unity throws.
            if (_agent.isOnNavMesh)
            {
                bool shouldStop = _rooted || _stunned;
                if (_agent.isStopped != shouldStop)
                    _agent.isStopped = shouldStop;
            }
            else
            {
                if (!_warnedNoNavmesh && logNavMeshWarnings)
                {
                    Debug.LogWarning($"{name}/{nameof(CCTargetServer)}(Server): NavMeshAgent is not on a NavMesh; skipping isStopped. Attempting auto-snap.");
                    _warnedNoNavmesh = true;
                }
                TryEnsureOnNavMesh();
            }
        }
    }

    public void ApplyKnockback(Vector3 direction, float distance)
    {
        if (!IsServer || direction.sqrMagnitude < 0.0001f || distance <= 0f) return;
        Vector3 target = transform.position + direction.normalized * distance;
        if (_agent && _agent.enabled && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            // Safe to warp only when currently on a NavMesh
            _agent.Warp(target);
        }
        else
        {
            // Directly move transform if agent isn't on a NavMesh; optionally try to snap.
            transform.position = target;
            TryEnsureOnNavMesh();
        }
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

    private void TryEnsureOnNavMesh()
    {
        if (!autoSnapToNavMesh || _agent == null) return;
        // If already on a NavMesh, nothing to do
        if (_agent.enabled && _agent.isActiveAndEnabled && _agent.isOnNavMesh) return;

        if (NavMesh.SamplePosition(transform.position, out var hit, Mathf.Max(0.1f, navMeshSnapRadius), NavMesh.AllAreas))
        {
            // Safest way: temporarily disable the agent, reposition, and re-enable so it attaches to the NavMesh.
            bool wasEnabled = _agent.enabled;
            if (wasEnabled) _agent.enabled = false;
            transform.position = hit.position;
            if (wasEnabled) _agent.enabled = true;
        }
        else if (logNavMeshWarnings)
        {
            Debug.LogWarning($"{name}/{nameof(CCTargetServer)}(Server): Could not find NavMesh within {navMeshSnapRadius}m to snap to.");
        }
    }
}
