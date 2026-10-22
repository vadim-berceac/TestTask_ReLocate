using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/Items/ItemData")]
public class ItemData: ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public GameObject GroundPrefab { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
}
