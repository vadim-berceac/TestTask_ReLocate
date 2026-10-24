using Cinemachine;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [field: SerializeField] public Transform MainCamera { get; private set; }
    [field: SerializeField] public CinemachineVirtualCameraBase VirtualCamera { get; private set; }
    [field: SerializeField] public CinemachineImpulseListener ImpulseListener { get; private set; }

    public void SetTarget(Transform target)
    {
        if (!VirtualCamera)
        {
            return;
        }
        
        VirtualCamera.Follow = target;
        VirtualCamera.LookAt = target;
    }
}
