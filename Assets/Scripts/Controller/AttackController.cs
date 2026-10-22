using UnityEngine;
using Zenject;

public class AttackController : MonoBehaviour
{
   [SerializeField] private Animator animator;
   [SerializeField] private PropBones propBones;
   [SerializeField] private WeaponData weaponData;
   
   [Inject] private readonly CharacterInputHandler _characterInputHandler;
   [Inject] private readonly AnimatorID _animatorID;
   
   private WeaponInstance _weapon;
   private int _currentWeaponPosition;

   private void Awake()
   {
      _weapon = (WeaponInstance)WearableItemInstanceFactory.Create(propBones, weaponData);
      _weapon?.SwitchPosition(0);
   }
   
   private void Update()
   {
      animator.SetBool(_animatorID.AnimIDAttack, _characterInputHandler.Attack);
   }
   
   public void SwitchWeaponPosition(int positionIndex)
   {
      _weapon?.SwitchPosition(positionIndex);
   }

   public void ShowTrail(float duration)
   {
      _weapon?.PlayRandomSwingSound();
   }
}
