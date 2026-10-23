using UnityEngine;

public static class ParticleSystemExtensions
{
    public static void PlayAtHitData(this ParticleSystem vfx, HitData hitData)
    {
        var position = hitData.Point + hitData.Normal * 0.01f;
        var rotation = Quaternion.LookRotation(hitData.Normal);

        var ps = Object.Instantiate(vfx, position, rotation);

        ps.Play();
        var destroyTime = ps.main.duration + ps.main.startLifetime.constantMax;
        Object.Destroy(ps.gameObject, destroyTime);
    }
}
