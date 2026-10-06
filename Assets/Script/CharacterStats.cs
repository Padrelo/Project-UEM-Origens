using System;
using UnityEngine;

/// <summary>
/// Controla HP e MP de qualquer personagem (jogador, inimigo, NPC).
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healthRegenPerSecond = 0f;

    [Header("MP")]
    [SerializeField] private float maxMana = 50f;
    [SerializeField] private float manaRegenPerSecond = 2f;

    [Header("Defesa")]
    [SerializeField] private float defense = 0f; // reduz dano físico

    public float CurrentHealth { get; private set; }
    public float CurrentMana { get; private set; }
    public float MaxHealth => maxHealth;
    public float MaxMana => maxMana;
    public bool IsDead { get; private set; }

    // Eventos úteis para UI (barras de vida/mana) e efeitos
    public event Action<float, float> OnHealthChanged; // (atual, máximo)
    public event Action<float, float> OnManaChanged;   // (atual, máximo)
    public event Action<float> OnDamaged;              // dano recebido
    public event Action OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        CurrentMana = maxMana;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

    private void Update()
    {
        if (IsDead) return;

        if (healthRegenPerSecond > 0f && CurrentHealth < maxHealth)
            Heal(healthRegenPerSecond * Time.deltaTime);

        if (manaRegenPerSecond > 0f && CurrentMana < maxMana)
            RestoreMana(manaRegenPerSecond * Time.deltaTime);
    }

    // ---------- HP ----------
    public void TakeDamage(float amount, bool ignoreDefense = false)
    {
        if (IsDead || amount <= 0f) return;

        float finalDamage = ignoreDefense ? amount : Mathf.Max(1f, amount - defense);
        CurrentHealth = Mathf.Max(0f, CurrentHealth - finalDamage);

        OnDamaged?.Invoke(finalDamage);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();
    }

    // ---------- MP ----------
    public bool HasMana(float cost) => CurrentMana >= cost;

    /// <returns>true se havia mana suficiente e ela foi gasta.</returns>
    public bool TryUseMana(float cost)
    {
        if (cost <= 0f) return true;
        if (!HasMana(cost)) return false;

        CurrentMana -= cost;
        OnManaChanged?.Invoke(CurrentMana, maxMana);
        return true;
    }

    public void RestoreMana(float amount)
    {
        if (IsDead || amount <= 0f) return;
        CurrentMana = Mathf.Min(maxMana, CurrentMana + amount);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }
}
