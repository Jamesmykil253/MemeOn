// Meme Online Battle Arena - ObjectPool (non-networked general-purpose pool)
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight pooling system for non-networked GameObjects.
/// For NetworkObjects, prefer NGO's NetworkObjectPool or a prefab handler.
/// </summary>
public sealed class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    private class Pool
    {
        public GameObject prefab;
        public int prewarm = 0;
    }

    [SerializeField] private List<Pool> prewarmed = new List<Pool>();

    private readonly Dictionary<GameObject, Stack<GameObject>> poolMap = new Dictionary<GameObject, Stack<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        foreach (var p in prewarmed)
        {
            if (p.prefab == null || p.prewarm <= 0) continue;
            for (int i = 0; i < p.prewarm; i++)
            {
                var go = Instantiate(p.prefab, transform);
                go.SetActive(false);
                if (!poolMap.TryGetValue(p.prefab, out var stack))
                {
                    stack = new Stack<GameObject>(p.prewarm);
                    poolMap[p.prefab] = stack;
                }
                stack.Push(go);
                instanceToPrefab[go] = p.prefab;
            }
        }
    }

    public T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        var go = Get(prefab.gameObject, position, rotation);
        return go.GetComponent<T>();
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!poolMap.TryGetValue(prefab, out var stack) || stack.Count == 0)
        {
            var go = Instantiate(prefab, position, rotation, transform);
            instanceToPrefab[go] = prefab;
            go.SetActive(true);
            return go;
        }
        else
        {
            var go = stack.Pop();
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            return go;
        }
    }

    public void Release(GameObject instance)
    {
        if (instance == null) return;
        if (!instanceToPrefab.TryGetValue(instance, out var prefab))
        {
            // Not pooled originally; destroy to avoid leaks.
            Destroy(instance);
            return;
        }
        instance.SetActive(false);
        instance.transform.SetParent(transform, false);
        if (!poolMap.TryGetValue(prefab, out var stack))
        {
            stack = new Stack<GameObject>();
            poolMap[prefab] = stack;
        }
        stack.Push(instance);
    }
}
