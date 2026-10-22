using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    void TakeDamage(float amount);
}

[RequireComponent(typeof(Collider))]
public class Damageable : MonoBehaviour, IDamageable
{
    [field: SerializeField] public float MaxHealth { get; private set; } = 100f;
    [field: SerializeField] public float CurrentHealth { get; private set; }

    public bool IsAlive => CurrentHealth > 0f;
    public bool IsDead => CurrentHealth <= 0f;

    public UnityEvent<float, float> HealthChanged; 
    public UnityEvent Died;

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
        if (!IsAlive || amount <= 0f)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (IsDead)
        {
            Died?.Invoke();
        }
    }

    public virtual void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}