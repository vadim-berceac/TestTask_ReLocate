using Cinemachine;
using UnityEngine;
using Zenject;

public class CameraSystem : MonoBehaviour
{
    [field: SerializeField] public Transform MainCamera { get; private set; }
    [field: SerializeField] public CinemachineVirtualCameraBase VirtualCamera { get; private set; }
    [field: SerializeField] public CinemachineImpulseListener ImpulseListener { get; private set; }
    [field: SerializeField] public bool LockCameraPosition { get; private set; }
    [field: SerializeField] public float TopClamp { get; private set; } = 45.0f;
    [field: SerializeField] public float BottomClamp { get; private set; } = -5.0f;
    [field: SerializeField] public float CameraAngleOverride { get; private set; }
    
    private const float Threshold = 0.01f;
    private Transform _target;
    private float _targetYaw;
    private float _targetPitch;
    
    [Inject] private readonly CharacterInputHandler _input;

    public void SetTarget(Transform target)
    {
        if (!VirtualCamera)
        {
            return;
        }
        _target = target;
        VirtualCamera.Follow = _target;
        VirtualCamera.LookAt = _target;
        
        _targetYaw = _target.rotation.eulerAngles.y;
    }

    private void LateUpdate()
    {
        CameraRotation();
    }
    
    private void CameraRotation()
    {
        if (_input.Look.sqrMagnitude >= Threshold && !LockCameraPosition)
        {
          _targetYaw += _input.Look.x;
            _targetPitch += _input.Look.y;
        }
        
        _targetYaw = ClampAngle(_targetYaw, float.MinValue, float.MaxValue);
        _targetPitch = ClampAngle(_targetPitch, BottomClamp, TopClamp);
        
        _target.rotation = Quaternion.Euler(_targetPitch + CameraAngleOverride,
            _targetYaw, 0.0f);
    }
    
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
