using UnityEngine;

public class HitVfxSpawner : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfxPrefab;
    [SerializeField] private float surfaceOffset = 0.01f;

    private VFXPool _pool;

    private void Awake()
    {
        if (vfxPrefab != null) _pool = VFXPoolManager.GetPool(vfxPrefab.gameObject, null);
    }

    public void Spawn(Vector3 point, Vector3 normal)
    {
        if (vfxPrefab == null) return;
        var rotation = normal.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(normal) : Quaternion.identity;
        var go = _pool.Get();
        go.transform.SetPositionAndRotation(point + normal * surfaceOffset, rotation);
        var ps = go.GetComponent<ParticleSystem>();
        ps.Play();
        var main = ps.main;
        var lifetime = main.duration + main.startLifetime.constantMax;
        var pooledLifetime = go.GetComponent<PooledVFXLifetime>();
        if (pooledLifetime == null) pooledLifetime = go.AddComponent<PooledVFXLifetime>();
        pooledLifetime.ScheduleReturn(g => _pool.Release(g), lifetime);
    }
}