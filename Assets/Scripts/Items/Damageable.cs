using System;
using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    void TakeDamage(float amount);
}

public interface IHitDamageable : IDamageable
{
    void TakeDamage(float amount, in HitData hit);
}

[RequireComponent(typeof(Collider))]
public class Damageable : MonoBehaviour, IHitDamageable
{
    [field: SerializeField] public float MaxHealth { get; private set; } = 100f;
    [field: SerializeField] public float CurrentHealth { get; private set; }

    public bool IsAlive => CurrentHealth > 0f;
    public bool IsDead => CurrentHealth <= 0f;

    public UnityEvent<float, float> HealthChanged;
    public UnityEvent Died;
    public UnityEvent<Vector3, Vector3> HitReceived;

    public event Action<HitData> HitDataReceived;

    private void Reset()
    {
        CurrentHealth = MaxHealth;
    }

    private void Awake()
    {
        if (CurrentHealth <= 0f)
            CurrentHealth = MaxHealth;
    }

    public virtual void TakeDamage(float amount)
    {
        ApplyDamage(amount);
    }

    public virtual void TakeDamage(float amount, in HitData hit)
    {
        if (!ApplyDamage(amount))
            return;

        HitReceived?.Invoke(hit.Point, hit.Normal);
        HitDataReceived?.Invoke(hit);
    }

    private bool ApplyDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
            return false;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (IsDead)
            Died?.Invoke();

        return true;
    }

    public virtual void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}