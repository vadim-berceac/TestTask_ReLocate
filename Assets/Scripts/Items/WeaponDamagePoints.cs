using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDamagePoints : MonoBehaviour
{
    [field: SerializeField] public AttackPoint[] AttackPoints { get; private set; }
    [field: SerializeField] public LayerMask DamageableLayer { get; private set; } = Physics.DefaultRaycastLayers;

    private static readonly Collider[] OverlapResults = new Collider[64];
    private static readonly float[] OverlapKeys = new float[64];
    private static readonly RaycastHit[] CastResults = new RaycastHit[32];
    private static readonly IComparer<RaycastHit> DistanceComparer = new HitDistanceComparer();

    private sealed class HitDistanceComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }

    private Vector3[] _previousPos;
    private bool _inAttack;
    private bool _hasDealtDamageThisAttack;
    private bool _hasHitColliderThisAttack;

    private bool _hasDamageCandidate;
    private bool _hasObstacleCandidate;
    private HitData _damageCandidate;
    private HitData _obstacleCandidate;

    public bool HasHitColliderThisAttack => _hasHitColliderThisAttack;
    public bool HasDealtDamageThisAttack => _hasDealtDamageThisAttack;

    public event Action<HitData> OnHitStop;
    public event Action<HitData> OnDamageDealt;

    public void SetDamageableLayer(LayerMask layer)
    {
        DamageableLayer = layer;
    }

    public void BeginAttack()
    {
        if (AttackPoints == null)
            return;

        _inAttack = true;
        _hasDealtDamageThisAttack = false;
        _hasHitColliderThisAttack = false;

        _previousPos = new Vector3[AttackPoints.Length];

        for (var i = 0; i < AttackPoints.Length; ++i)
        {
            var point = AttackPoints[i];
            if (point == null || !point.AttackRoot)
                continue;

            var worldPos = point.AttackRoot.position + point.AttackRoot.TransformVector(point.Offset);
            _previousPos[i] = worldPos;

#if UNITY_EDITOR
            point.PreviousPositions.Clear();
            point.PreviousPositions.Add(worldPos);
#endif
        }
    }

    public void EndAttack()
    {
        _inAttack = false;
        _hasDealtDamageThisAttack = false;
        _hasHitColliderThisAttack = false;

#if UNITY_EDITOR
        if (AttackPoints == null)
            return;

        foreach (var point in AttackPoints)
        {
            if (point != null)
                point.PreviousPositions.Clear();
        }
#endif
    }

    private void FixedUpdate()
    {
        if (!_inAttack || AttackPoints == null)
            return;

        _hasDamageCandidate = false;
        _hasObstacleCandidate = false;

        for (var i = 0; i < AttackPoints.Length; ++i)
        {
            var point = AttackPoints[i];
            if (point == null || !point.AttackRoot)
                continue;

            var currentPos = point.AttackRoot.position + point.AttackRoot.TransformVector(point.Offset);
            var previousPos = _previousPos[i];

            CollectContacts(point, previousPos, currentPos);

            _previousPos[i] = currentPos;

#if UNITY_EDITOR
            point.PreviousPositions.Add(currentPos);
#endif
        }

        ResolveContacts();
    }

    private void ResolveContacts()
    {
        if (_hasDamageCandidate)
        {
            var data = _damageCandidate;
            _hasHitColliderThisAttack = true;
            _hasDealtDamageThisAttack = true;
            _inAttack = false;
            data.Damageable.TakeDamage(data.Damage);
            OnDamageDealt?.Invoke(data);
            return;
        }

        if (_hasObstacleCandidate)
        {
            var data = _obstacleCandidate;
            _hasHitColliderThisAttack = true;
            _inAttack = false;
            OnHitStop?.Invoke(data);
        }
    }

    private void CollectContacts(AttackPoint point, Vector3 start, Vector3 end)
    {
        var delta = end - start;
        var distance = delta.magnitude;
        var dir = distance > 1e-4f ? delta / distance : Vector3.zero;

        CollectStartOverlap(point, start, dir);

        if (distance < 1e-4f)
            return;

        var count = Physics.SphereCastNonAlloc(start, point.Radius, dir, CastResults,
            distance, DamageableLayer, QueryTriggerInteraction.Ignore);
        if (count == 0)
            return;

        Array.Sort(CastResults, 0, count, DistanceComparer);

        for (var i = 0; i < count; i++)
        {
            var hit = CastResults[i];
            if (hit.distance <= 0f)
                continue;

            var center = start + dir * hit.distance;
            RegisterContact(point, hit.collider, hit.point, hit.normal, center, dir);
        }
    }

    private void CollectStartOverlap(AttackPoint point, Vector3 position, Vector3 dir)
    {
        var count = Physics.OverlapSphereNonAlloc(position, point.Radius, OverlapResults,
            DamageableLayer, QueryTriggerInteraction.Ignore);
        if (count == 0)
            return;

        for (var i = 0; i < count; i++)
        {
            var closest = GetClosestPoint(OverlapResults[i], position);
            OverlapKeys[i] = (closest - position).sqrMagnitude;
        }

        Array.Sort(OverlapKeys, OverlapResults, 0, count);

        for (var i = 0; i < count; i++)
        {
            var col = OverlapResults[i];
            if (!col)
                continue;

            var closest = GetClosestPoint(col, position);
            var normal = position - closest;
            normal = normal.sqrMagnitude > 1e-8f ? normal.normalized : -dir;

            RegisterContact(point, col, closest, normal, position, dir);
        }
    }

    private void RegisterContact(AttackPoint point, Collider col, Vector3 contactPoint,
        Vector3 normal, Vector3 attackPointPos, Vector3 dir)
    {
        if (col.transform.root == transform.root)
            return;

        var damageable = col.GetComponentInParent<IDamageable>();
        var data = new HitData(point, col, damageable, contactPoint, normal, attackPointPos, dir, point.Damage);

        if (damageable != null)
        {
            if (_hasDamageCandidate)
                return;

            _hasDamageCandidate = true;
            _damageCandidate = data;
            return;
        }

        if (_hasObstacleCandidate)
            return;

        _hasObstacleCandidate = true;
        _obstacleCandidate = data;
    }

    private static Vector3 GetClosestPoint(Collider col, Vector3 position)
    {
        if (col is MeshCollider { convex: false })
        {
            var toCenter = col.bounds.center - position;
            var ray = new Ray(position, toCenter);
            if (col.Raycast(ray, out var rayHit, toCenter.magnitude + col.bounds.extents.magnitude))
                return rayHit.point;

            return col.bounds.ClosestPoint(position);
        }

        return col.ClosestPoint(position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (AttackPoints == null || AttackPoints.Length == 0)
            return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);

        foreach (var point in AttackPoints)
        {
            if (point == null || point.AttackRoot == null)
                continue;

            var worldPos = point.AttackRoot.position + point.AttackRoot.TransformVector(point.Offset);
            Gizmos.DrawSphere(worldPos, point.Radius);

            if (point.PreviousPositions.Count > 1)
            {
                UnityEditor.Handles.DrawAAPolyLine(10, point.PreviousPositions.ToArray());
            }
        }
    }
#endif
}