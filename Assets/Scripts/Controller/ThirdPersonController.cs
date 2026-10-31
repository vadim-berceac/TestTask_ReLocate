using UnityEngine;
using Zenject;

namespace StarterAssets
{
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can Jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to Jump again. Set to 0f to instantly Jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;
        
        [Header("Attack Rotation")]
        [Tooltip("Во сколько раз медленнее поворачиваемся во время атаки (1 = как обычно)")]
        [Range(1f, 20f)]
        public float AttackRotationSlowdown = 6f;

        [Tooltip("Как быстро переключаемся между обычным и атакующим режимом поворота")]
        public float RotationModeBlendSpeed = 10f;
        
        [Inject] private readonly Animator _animator;
        [Inject] private readonly Transform _root;
        [Inject] private readonly CharacterController _controller;
        [Inject] private readonly CharacterInputHandler _input;
        [Inject] private readonly AnimatorID _animatorID;
        [Inject] private readonly CameraSystem _cameraSystem;
        [Inject] private readonly CameraRoot _cameraRoot;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _rotationSlowdownCurrent = 1f;
        private bool _isOnAttack;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
      

        private void Start()
        {
            _cameraSystem.SetTarget(_cameraRoot.Root);

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            CheckAttack();
            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void CheckAttack()
        {
            _isOnAttack = _animator.IsInTag(_animatorID.AttackStaticTag);
        }
        
        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(_root.position.x, _root.position.y - GroundedOffset,
                _root.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            _animator.SetBool(_animatorID.AnimIDGrounded, Grounded);
        }
        
        private void Move()
        {
            var targetSpeed = _input.Sprint ? SprintSpeed : MoveSpeed;
            
            if (_input.Move == Vector2.zero || _isOnAttack) targetSpeed = 0.0f;

            var currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            var speedOffset = 0.1f;
            var inputMagnitude = _input.AnalogMovement ? _input.Move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }
            
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            var inputDirection = new Vector3(_input.Move.x, 0.0f, _input.Move.y).normalized;

            var targetSlowdown = _isOnAttack ? AttackRotationSlowdown : 1f;
            _rotationSlowdownCurrent = Mathf.Lerp(_rotationSlowdownCurrent, targetSlowdown,
                Time.deltaTime * RotationModeBlendSpeed);

            if (_input.Move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _cameraSystem.MainCamera.transform.eulerAngles.y;
                var rotation = Mathf.SmoothDampAngle(_root.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime * _rotationSlowdownCurrent);

                _root.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            var moveYaw = _isOnAttack ? _root.eulerAngles.y : _targetRotation;
            var targetDirection = Quaternion.Euler(0.0f, moveYaw, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                            new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            _animator.SetFloat(_animatorID.AnimIDSpeed, _animationBlend);
            _animator.SetFloat(_animatorID.AnimIDMotionSpeed, inputMagnitude);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                _animator.SetBool(_animatorID.AnimIDJump, false);
                _animator.SetBool(_animatorID.AnimIDFreeFall, false);

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.Jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    _animator.SetBool(_animatorID.AnimIDJump, true);
                }

                // Jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the Jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    _animator.SetBool(_animatorID.AnimIDFreeFall, true);
                }

                // if we are not grounded, do not Jump
                _input.ResetJump();
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }
    }
}
