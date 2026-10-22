using UnityEngine;

[CreateAssetMenu(fileName = "WearableItemData", menuName = "Scriptable Objects/Items/WearableItemData")]
public class WearableItemData : ItemData
{
    [field: SerializeField] public WearableItemSettings[] WearableElements { get; private set; }
}

[System.Serializable]
public class WearableItemSettings
{
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public PropBoneSettings[] Positions { get; private set; }
}