using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class HitDecalSpawner : MonoBehaviour
{
    [SerializeField] private DecalProjector decalPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private float lifetime = 8f;

    public void Spawn(Vector3 point, Vector3 normal)
    {
        if (decalPrefab == null || !parent)
        {
            return;
        }
        var rotation = Quaternion.LookRotation(-normal, Random.insideUnitSphere);
        var decal = Instantiate(decalPrefab, point, rotation);
        decal.transform.parent = parent;
        Destroy(decal.gameObject, lifetime);
    }
}