using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/Items/WeaponData")]
public class WeaponData : WearableItemData
{
   [field: Header("Sounds")]
   [field: SerializeField] public AudioClip HitStopSound { get; set; }
   [field: SerializeField] public AudioClip[] SwingSounds { get; set; }
   
   [field: Header("VFX")]
   [field: SerializeField] public ParticleSystem HitStopVFX { get; set; }
}
