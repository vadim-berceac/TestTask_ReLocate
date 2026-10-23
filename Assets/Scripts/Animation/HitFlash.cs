using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class HitFlash : MonoBehaviour
{
    [SerializeField] private Damageable damageable;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color flashColor = new (1f, 0.4f, 0.15f);
    [SerializeField] private float intensity = 3000f;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");
    private MaterialPropertyBlock _block;
    private float _time;

    private void Awake()
    {
        _block = new MaterialPropertyBlock();
        damageable.HitDataReceived += OnHit;
        enabled = false;
    }

    private void OnDestroy()
    {
        if (damageable)
            damageable.HitDataReceived -= OnHit;
    }

    private void OnHit(HitData hit)
    {
        _time = 0f;
        enabled = true;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        var k = _time / duration;

        if (k >= 1f)
        {
            foreach (var r in renderers)
                r.SetPropertyBlock(null);

            enabled = false;
            return;
        }

        var color = flashColor * (intensity * falloff.Evaluate(k));

        foreach (var r in renderers)
        {
            r.GetPropertyBlock(_block);
            _block.SetColor(EmissiveColorId, color);
            r.SetPropertyBlock(_block);
        }
    }
}