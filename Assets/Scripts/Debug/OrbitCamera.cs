using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 30f;

    private void LateUpdate()
    {
        transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
        transform.LookAt(target);
    }
}