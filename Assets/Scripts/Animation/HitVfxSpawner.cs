using UnityEngine;

public class HitVfxSpawner : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfxPrefab;
    [SerializeField] private float surfaceOffset = 0.01f;

    public void Spawn(Vector3 point, Vector3 normal)
    {
        var rotation = normal.sqrMagnitude > 1e-6f
            ? Quaternion.LookRotation(normal)
            : Quaternion.identity;

        var ps = Instantiate(vfxPrefab, point + normal * surfaceOffset, rotation);
        ps.Play();

        var main = ps.main;
        Destroy(ps.gameObject, main.duration + main.startLifetime.constantMax);
    }
}