using UnityEngine;

public readonly struct HitData
{
    public readonly AttackPoint AttackPoint;
    public readonly Collider Collider;
    public readonly IDamageable Damageable;
    public readonly Vector3 Point;
    public readonly Vector3 Normal;
    public readonly Vector3 AttackPointPosition;
    public readonly Vector3 SwingDirection;
    public readonly float Damage;

    public HitData(AttackPoint attackPoint, Collider collider, IDamageable damageable,
        Vector3 point, Vector3 normal, Vector3 attackPointPosition, Vector3 swingDirection, float damage)
    {
        AttackPoint = attackPoint;
        Collider = collider;
        Damageable = damageable;
        Point = point;
        Normal = normal;
        AttackPointPosition = attackPointPosition;
        SwingDirection = swingDirection;
        Damage = damage;
    }
}