using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class AttackController : MonoBehaviour
{
   [SerializeField] private WeaponData weaponData;
   
   [Inject] private readonly Animator _animator;
   [Inject] private readonly PropBones _propBones;
   [Inject] private readonly CharacterInputHandler _characterInputHandler;
   [Inject] private readonly AnimatorID _animatorID;
   [Inject] private readonly DamageMask _damageMask;
   
   private WeaponInstance _weapon;
   private int _currentWeaponPosition;
   private CancellationTokenSource _trailCts;

   private void Awake()
   {
      _weapon = (WeaponInstance)WearableItemInstanceFactory.Create(_propBones, weaponData);
      _weapon?.SwitchPosition(0);
      _weapon?.SetDamageMask(_damageMask.DamageableLayer);
   }
   
   private void Update()
   {
      _animator.SetBool(_animatorID.AnimIDAttack, _characterInputHandler.Attack);
   }
   
   public void SwitchWeaponPosition(AnimationEvent animationEvent)
   {
      _weapon?.SwitchPosition(animationEvent.intParameter);
   }

   public void ShowTrail(AnimationEvent animationEvent)
   {
     _ = ShowTrailAsync(animationEvent.floatParameter);
   }

   public void BeginAttack(AnimationEvent animationEvent)
   {
      if (animationEvent.animatorClipInfo.weight <= 0.5f)
      {
         return;
      }
      _weapon?.BeginAttack();
   }

   public void EndAttack(AnimationEvent animationEvent)
   {
      if (animationEvent.animatorClipInfo.weight <= 0.5f)
      {
         return;
      }
      
      var damageDealt = _weapon?.HasDealtDamageThisAttack ?? false;
      var hitCollider = _weapon?.HasHitColliderThisAttack ?? false;
      _weapon?.EndAttack();

      if (_weapon != null && hitCollider && !damageDealt)
      {
         _animator.SetTrigger(_animatorID.AnimIDHitStopTrigger);
      }
   }
   
   private async Task ShowTrailAsync(float duration)
   {
      _trailCts?.Cancel();
      _trailCts?.Dispose();
      _trailCts = new CancellationTokenSource();

      var token = _trailCts.Token;

      try
      {
         _weapon?.PlayRandomSwingSound();
         _weapon?.ShowTrail(true);

         await Task.Delay(TimeSpan.FromSeconds(duration), token);

         if (!token.IsCancellationRequested)
            _weapon?.ShowTrail(false);
      }
      catch (OperationCanceledException)
      {
         
      }
      catch (Exception ex)
      {
         Debug.LogException(ex);
         _weapon?.ShowTrail(false);
      }
      finally
      {
         if (_trailCts?.Token == token)
         {
            _trailCts.Dispose();
            _trailCts = null;
         }
      }
   }
   
   private void OnDestroy()
   {
      _trailCts?.Cancel();
      _trailCts?.Dispose();
      _trailCts = null;
   }
}
