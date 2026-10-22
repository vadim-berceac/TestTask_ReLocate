using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/Items/WeaponData")]
public class WeaponData : WearableItemData
{
   [field: Header("Sounds")]
   [field: SerializeField] public AudioClip SwitchPositionSound { get; set; }
   [field: SerializeField] public AudioClip[] SwingSounds { get; set; }
}
