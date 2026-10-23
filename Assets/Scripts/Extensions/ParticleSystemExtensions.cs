using UnityEngine;

public static class ParticleSystemExtensions
{
    public static void PlayAtHitData(this ParticleSystem vfx, HitData hitData)
    {
        if (!vfx)
        {
            return;
        }
        var position = hitData.Point + hitData.Normal * 0.01f;
        var rotation = Quaternion.LookRotation(hitData.Normal);
        var pool = VFXPoolManager.GetPool(vfx.gameObject, null);
        var go = pool.Get();
        go.transform.SetPositionAndRotation(position, rotation);
        var ps = go.GetComponent<ParticleSystem>();
        ps.Play();
        var main = ps.main;
        var destroyTime = main.duration + main.startLifetime.constantMax;
        var pooledLifetime = go.GetComponent<PooledVFXLifetime>();
        if (!pooledLifetime)
        {
            pooledLifetime = go.AddComponent<PooledVFXLifetime>();
        }
        pooledLifetime.ScheduleReturn(g => pool.Release(g), destroyTime);
    }
}
