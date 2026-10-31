using UnityEngine;
using Zenject;

public class TargetingController : MonoBehaviour
{
   [Tooltip("Смещение начала капсулы в локальных координатах")]
   [SerializeField] private Vector3 originOffset = new Vector3(0f, 1f, 0f);
   [Tooltip("Длина капсулы вперёд (от начала до конца оси)")]
   [SerializeField, Min(0f)] private float capsuleLength = 3f;
   [SerializeField, Min(0.01f)] private float capsuleRadius = 1f;
   [SerializeField, Min(1)] private int maxColliders = 16;
   [Tooltip("Максимальный угол до цели от взгляда персонажа (0 - без ограничения)")]
   [SerializeField, Range(0f, 180f)] private float maxTargetAngle = 90f;

   [Tooltip("Скорость доворота, градусов в секунду")]
   [SerializeField, Min(0f)] private float rotationSpeed = 360f;
   [Tooltip("Плавность: чем больше, тем резче в начале (экспоненциальное сглаживание). 0 - только линейная скорость")]
   [SerializeField, Min(0f)] private float smoothing = 12f;
   [Tooltip("Максимальный суммарный доворот за одну атаку (0 - без ограничения)")]
   [SerializeField, Range(0f, 180f)] private float maxTotalTurn = 60f;
   [Tooltip("Прекращать доворот, когда угол меньше этого значения")]
   [SerializeField, Min(0f)] private float stopAngle = 0.5f;
   [Tooltip("Доворачивать только по горизонтали")]
   [SerializeField] private bool yawOnly = true;
   
   [Inject] private readonly Animator _animator;
   [Inject] private readonly Transform _rotationRoot;
   [Inject] private readonly AnimatorID _animatorID;
   [Inject] private readonly DamageMask _damageMask;

   private bool _isOnAttack;
   private bool _targetingEnabled;

   private Collider[] _hits;
   private Collider _target;
   private float _turnedTotal;

   private void Awake()
   {
      _hits = new Collider[maxColliders];
   }

   private void Update()
   {
      CheckOnAttack();
      EnableTargeting();
      RotateToTarget();
   }

   private void CheckOnAttack()
   {
      _isOnAttack = _animator.IsInTag(_animatorID.AttackStaticTag);
   }

   private void EnableTargeting()
   {
      if (_isOnAttack && !_targetingEnabled)
      {
         _targetingEnabled = true;
         _turnedTotal = 0f;
         _target = FindTarget();
         return;
      }

      if (!_isOnAttack && _targetingEnabled)
      {
         _targetingEnabled = false;
         _target = null;
      }
   }

   private Collider FindTarget()
   {
      GetCapsule(out var p0, out var p1);

      var count = Physics.OverlapCapsuleNonAlloc(
         p0, p1, capsuleRadius, _hits, _damageMask.DamageableLayer, QueryTriggerInteraction.Ignore);

      Collider best = null;
      var bestScore = float.MaxValue;
      var forward = _rotationRoot.forward;

      for (var i = 0; i < count; i++)
      {
         var col = _hits[i];
         _hits[i] = null; 

         if (!col)
            continue;

         if (col.transform.IsChildOf(_rotationRoot))
            continue;

         var toTarget = col.bounds.center - _rotationRoot.position;
         toTarget.y = 0f;

         var distance = toTarget.magnitude;
         if (distance < 0.0001f)
            continue;

         var angle = Vector3.Angle(forward, toTarget);
         if (maxTargetAngle > 0f && angle > maxTargetAngle)
            continue;

         var score = distance + angle * 0.01f;
         if (score < bestScore)
         {
            bestScore = score;
            best = col;
         }
      }

      return best;
   }

   private void RotateToTarget()
   {
      if (!_targetingEnabled || !_target)
         return;

      if (!_target.enabled || !_target.gameObject.activeInHierarchy)
      {
         _target = null;
         return;
      }

      var direction = _target.bounds.center - _rotationRoot.position;
      if (yawOnly)
         direction.y = 0f;

      if (direction.sqrMagnitude < 0.0001f)
         return;

      var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
      var angle = Quaternion.Angle(_rotationRoot.rotation, targetRotation);

      if (angle <= stopAngle)
         return;

      if (maxTotalTurn > 0f && _turnedTotal >= maxTotalTurn)
         return;

      var dt = Time.deltaTime;

      var maxStep = rotationSpeed * dt;
      var step = maxStep;

      if (smoothing > 0f)
      {
         var smoothStep = angle * (1f - Mathf.Exp(-smoothing * dt));
         step = Mathf.Min(maxStep, smoothStep);
      }

      if (maxTotalTurn > 0f)
         step = Mathf.Min(step, maxTotalTurn - _turnedTotal);

      _rotationRoot.rotation = Quaternion.RotateTowards(_rotationRoot.rotation, targetRotation, step);
      _turnedTotal += step;
   }

   private void GetCapsule(out Vector3 p0, out Vector3 p1)
   {
      p0 = _rotationRoot.TransformPoint(originOffset);
      p1 = p0 + _rotationRoot.forward * capsuleLength;
   }
}