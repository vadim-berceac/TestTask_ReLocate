using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        [field: SerializeField] public Vector2 Move { get; private set; }
        [field: SerializeField] public Vector2 Look { get; private set; }
        [field: SerializeField] public bool Jump { get; private set; }
        [field: SerializeField] public bool Sprint { get; private set; }
        public bool LightSpot { get; private set; }

        [Header("Input Buffer")]
        [Tooltip("Сколько секунд нажатие атаки остаётся «живым», если его не успели потребить")]
        [SerializeField] private float attackBufferTime = 0.15f;

        [Header("Movement Settings")]
        [field: SerializeField] public bool AnalogMovement { get; private set; }

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        public Action<Vector2> MoveAction;
        public Action<Vector2> LookAction;
        public Action<bool> JumpAction;
        public Action<bool> SprintAction;
        public Action<bool> AttackAction;
        public Action LightSpotAction;

        private float _attackPressedTime = float.NegativeInfinity;

        public bool Attack => Time.time - _attackPressedTime <= attackBufferTime;

        private void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        private void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        private void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        private void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        private void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }

        private void OnLightSpot(InputValue value)
        {
            LightSpotInput(value.isPressed);
        }

        private void MoveInput(Vector2 newMoveDirection)
        {
            Move = newMoveDirection;
            MoveAction?.Invoke(Move);
        }

        private void LookInput(Vector2 newLookDirection)
        {
            Look = newLookDirection;
            LookAction?.Invoke(Look);
        }

        private void JumpInput(bool newJumpState)
        {
            Jump = newJumpState;
            JumpAction?.Invoke(Jump);
        }

        private void SprintInput(bool newSprintState)
        {
            Sprint = newSprintState;
            SprintAction?.Invoke(Sprint);
        }

        private void AttackInput(bool isPressed)
        {
            if (isPressed)
            {
                _attackPressedTime = Time.time;
            }

            AttackAction?.Invoke(isPressed);
        }

        private void LightSpotInput(bool isPressed)
        {
            LightSpot = isPressed;
            LightSpotAction?.Invoke();
        }

        public void ResetJump()
        {
            Jump = false;
        }

        public void ResetLightSpot()
        {
            LightSpot = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}