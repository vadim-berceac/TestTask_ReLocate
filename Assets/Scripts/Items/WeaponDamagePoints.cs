using UnityEngine;

public class WeaponDamagePoints : MonoBehaviour
{
    [field: SerializeField] public AttackPoint[] AttackPoints { get; private set; }
    [field: SerializeField] public LayerMask DamageableLayer { get; private set; } = Physics.DefaultRaycastLayers;

    private static readonly Collider[] OverlapResults = new Collider[64];

    private Vector3[] _previousPos;
    private bool _inAttack;
    private bool _hasDealtDamageThisAttack;

    public void BeginAttack()
    {
        if (AttackPoints == null)
            return;

        _inAttack = true;
        _hasDealtDamageThisAttack = false;

        _previousPos = new Vector3[AttackPoints.Length];

        for (var i = 0; i < AttackPoints.Length; ++i)
        {
            var point = AttackPoints[i];
            if (point == null || point.AttackRoot == null)
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
        if (!_inAttack || AttackPoints == null || _hasDealtDamageThisAttack)
            return;

        for (var i = 0; i < AttackPoints.Length; ++i)
        {
            var point = AttackPoints[i];
            if (point == null || point.AttackRoot == null)
                continue;

            var currentPos = point.AttackRoot.position + point.AttackRoot.TransformVector(point.Offset);
            var previousPos = _previousPos[i];

            SweepAndApplyDamage(point, previousPos, currentPos);

            _previousPos[i] = currentPos;

#if UNITY_EDITOR
            point.PreviousPositions.Add(currentPos);
#endif

            if (_hasDealtDamageThisAttack)
                break;
        }
    }

    private void SweepAndApplyDamage(AttackPoint point, Vector3 start, Vector3 end)
    {
        var direction = end - start;
        var distance = direction.magnitude;

        CheckOverlap(point, start, point.Damage);
        if (_hasDealtDamageThisAttack)
            return;

        if (distance < 1e-4f)
            return;

        direction /= distance;

        var sampleCount = Mathf.CeilToInt(distance / (point.Radius * 2f));
        sampleCount = Mathf.Clamp(sampleCount, 1, 8);

        for (var i = 1; i <= sampleCount; i++)
        {
            var t = i / (float)(sampleCount + 1);
            var samplePos = start + distance * t * direction;

            CheckOverlap(point, samplePos, point.Damage);
            if (_hasDealtDamageThisAttack)
                return;
        }

        CheckOverlap(point, end, point.Damage);
    }

    private void CheckOverlap(AttackPoint point, Vector3 position, float damage)
    {
        var hitCount = Physics.OverlapSphereNonAlloc(position, point.Radius, OverlapResults, DamageableLayer);
        ApplyDamageToHits(OverlapResults, hitCount, damage);
    }

    private void ApplyDamageToHits(Collider[] hits, int count, float damage)
    {
        for (var i = 0; i < count; ++i)
        {
            var col = hits[i];
            if (col == null)
                continue;

            if (TryApplyDamage(col, damage))
                return;
        }
    }

    private bool TryApplyDamage(Collider collider, float damage)
    {
        if (_hasDealtDamageThisAttack)
            return false;

        if (collider.transform.root == transform.root)
            return false;

        var damageable = collider.GetComponentInParent<IDamageable>();
        if (damageable == null)
            return false;

        damageable.TakeDamage(damage);
        _hasDealtDamageThisAttack = true;
        return true;
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