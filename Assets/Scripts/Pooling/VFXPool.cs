using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class VFXPool
{
    private readonly GameObject _prefab;
    private readonly Transform _parent;
    private readonly Stack<GameObject> _available = new Stack<GameObject>();
    private readonly HashSet<GameObject> _active = new HashSet<GameObject>();

    public VFXPool(GameObject prefab, Transform parent, int initialCapacity = 8)
    {
        _prefab = prefab;
        _parent = parent;
        for (int i = 0; i < initialCapacity; i++)
        {
            var go = UnityEngine.Object.Instantiate(prefab, parent);
            go.SetActive(false);
            _available.Push(go);
        }
    }

    public GameObject Get()
    {
        GameObject go = _available.Count > 0 ? _available.Pop() : UnityEngine.Object.Instantiate(_prefab, _parent);
        _active.Add(go);
        go.SetActive(true);
        return go;
    }

    public void Release(GameObject go)
    {
        if (!_active.Remove(go)) return;
        go.SetActive(false);
        _available.Push(go);
    }
}

public static class VFXPoolManager
{
    private static readonly Dictionary<int, VFXPool> _pools = new Dictionary<int, VFXPool>();

    public static VFXPool GetPool(GameObject prefab, Transform parent, int initialCapacity = 8)
    {
        int key = prefab.GetInstanceID();
        if (!_pools.TryGetValue(key, out var pool))
        {
            pool = new VFXPool(prefab, parent, initialCapacity);
            _pools[key] = pool;
        }
        return pool;
    }
}

public sealed class PooledVFXLifetime : MonoBehaviour
{
    private Action<GameObject> _onReturn;
    private float _timeLeft;
    private bool _active;

    public void ScheduleReturn(Action<GameObject> onReturn, float seconds)
    {
        _onReturn = onReturn;
        _timeLeft = seconds;
        _active = true;
    }

    private void Update()
    {
        if (!_active) return;
        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _active = false;
            gameObject.SetActive(false);
            _onReturn?.Invoke(gameObject);
        }
    }
}