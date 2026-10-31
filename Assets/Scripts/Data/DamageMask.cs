using UnityEngine;

[CreateAssetMenu(fileName = "DamageMask", menuName = "Scriptable Objects/DamageMask")]
public class DamageMask : ScriptableObject
{
    [field: SerializeField] public LayerMask DamageableLayer { get; private set; } =  Physics.DefaultRaycastLayers;
}
