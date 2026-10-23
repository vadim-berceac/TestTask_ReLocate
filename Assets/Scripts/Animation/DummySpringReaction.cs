using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class DummySpringReaction : MonoBehaviour
{
    [SerializeField] private Damageable damageable;
    [SerializeField] private Transform visual;
    [SerializeField] private float stiffness = 60f;
    [SerializeField] private float damping = 6f;
    [SerializeField] private float kickPerDamage = 8f;
    [SerializeField] private float maxAngle = 35f;
    [SerializeField] private float maxVelocity = 400f;
    [SerializeField] private float restAngle = 0.05f;
    [SerializeField] private float restVelocity = 0.5f;

    private const float MaxStep = 1f / 30f;
    private Quaternion _baseLocalRotation;
    private Vector3 _angle;
    private Vector3 _velocity;

    private void Awake()
    {
        _baseLocalRotation = visual.localRotation;
        damageable.HitDataReceived += OnHit;
        enabled = false;
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.HitDataReceived -= OnHit;
    }

    private void OnHit(HitData hit)
    {
        var dir = hit.SwingDirection.sqrMagnitude > 1e-6f ? hit.SwingDirection : -hit.Normal;
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f)
            return;

        var axis = Vector3.Cross(Vector3.up, dir.normalized);
        if (visual.parent)
            axis = visual.parent.InverseTransformDirection(axis);

        _velocity += axis * (hit.Damage * kickPerDamage);
        enabled = true;
    }

    private void Update()
    {
        var dt = Mathf.Min(Time.deltaTime, MaxStep);

        _velocity += (-stiffness * _angle - damping * _velocity) * dt;
        _velocity = Vector3.ClampMagnitude(_velocity, maxVelocity);
        _angle += _velocity * dt;
        _angle = Vector3.ClampMagnitude(_angle, maxAngle);

        if (_angle.sqrMagnitude < restAngle * restAngle &&
            _velocity.sqrMagnitude < restVelocity * restVelocity)
        {
            _angle = Vector3.zero;
            _velocity = Vector3.zero;
            visual.localRotation = _baseLocalRotation;
            enabled = false;
            return;
        }

        visual.localRotation = Quaternion.Euler(_angle) * _baseLocalRotation;
    }
}