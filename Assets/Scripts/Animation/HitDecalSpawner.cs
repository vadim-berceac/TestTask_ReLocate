using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class HitDecalSpawner : MonoBehaviour
{
    [SerializeField] private DecalProjector decalPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private float lifetime = 8f;

    private VFXPool _pool;

    private void Awake()
    {
        if (decalPrefab != null) _pool = VFXPoolManager.GetPool(decalPrefab.gameObject, parent);
    }

    public void Spawn(Vector3 point, Vector3 normal)
    {
        if (decalPrefab == null || !parent) return;
        var rotation = Quaternion.LookRotation(-normal, Random.insideUnitSphere);
        var go = _pool.Get();
        go.transform.SetPositionAndRotation(point, rotation);
        go.transform.SetParent(parent, true);
        var pooledLifetime = go.GetComponent<PooledVFXLifetime>();
        if (pooledLifetime == null) pooledLifetime = go.AddComponent<PooledVFXLifetime>();
        pooledLifetime.ScheduleReturn(g => _pool.Release(g), this.lifetime);
    }
}