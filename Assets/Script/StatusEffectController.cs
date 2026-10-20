using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActiveStatus
{
    public StatusEffectData data;
    public float remaining;
    public int stacks = 1;
    public float tickTimer;
}

// Coloque este componente em todo personagem (jogador e inimigos).
// O mesmo GameObject precisa ter o CharacterStats.
public class StatusEffectController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer; // opcional: para tingir o sprite

    private readonly List<ActiveStatus> active = new List<ActiveStatus>();
    private CharacterStats target;
    private Color baseColor = Color.white;

    public event Action<ActiveStatus> OnStatusAdded;
    public event Action<ActiveStatus> OnStatusRemoved;

    public IReadOnlyList<ActiveStatus> ActiveStatuses => active;

    private void Awake()
    {
        target = GetComponent<CharacterStats>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) baseColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        if (target != null) target.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (target != null) target.OnDeath -= HandleDeath;
    }

    private void HandleDeath() => ClearAll();

    // ---------- API pública ----------

    public void AddStatus(StatusEffectData data)
    {
        if (data == null) return;

        ActiveStatus existing = active.Find(s => s.data == data);
        if (existing != null)
        {
            existing.remaining = data.duration; // renova a duração
            if (data.stackable && existing.stacks < data.maxStacks) existing.stacks++;
            return;
        }

        var status = new ActiveStatus { data = data, remaining = data.duration };
        active.Add(status);
        UpdateVisual();
        OnStatusAdded?.Invoke(status);
    }

    public void RemoveStatus(StatusEffectData data)
    {
        int i = active.FindIndex(s => s.data == data);
        if (i >= 0) RemoveAt(i);
    }

    public void ClearAll()
    {
        for (int i = active.Count - 1; i >= 0; i--) RemoveAt(i);
    }

    public bool HasStatus(StatusType type) => active.Exists(s => s.data.type == type);

    // Use antes de deixar o personagem agir (atordoado não age)
    public bool CanAct() => !active.Exists(s => s.data.preventsAction);

    // Use para multiplicar a velocidade de movimento (lentidão)
    public float SpeedMultiplier()
    {
        float m = 1f;
        foreach (var s in active) m *= s.data.speedMultiplier;
        return m;
    }

    // Chame no FIM do turno do personagem (combate por turnos)
    public void OnTurnEnd()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (i >= active.Count) continue; // lista pode mudar se o alvo morrer
            var s = active[i];
            if (s.data.tickMode != TickMode.PerTurn) continue;

            Tick(s);
            s.remaining -= 1f;
            if (s.remaining <= 0f && i < active.Count) RemoveAt(i);
        }
    }

    // ---------- Tempo real ----------

    private void Update()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (i >= active.Count) continue;
            var s = active[i];
            if (s.data.tickMode != TickMode.RealTime) continue;

            s.remaining -= Time.deltaTime;
            s.tickTimer += Time.deltaTime;

            if (s.tickTimer >= s.data.tickInterval)
            {
                s.tickTimer -= s.data.tickInterval;
                Tick(s);
            }

            if (s.remaining <= 0f && i < active.Count) RemoveAt(i);
        }
    }

    // ---------- Internos ----------

    private void Tick(ActiveStatus s)
    {
        if (target == null || target.IsDead) return;

        float amount = s.data.damagePerTick * s.stacks;
        if (amount > 0f) target.TakeDamage(amount, s.data.ignoreDefense);
        else if (amount < 0f) target.Heal(-amount);
    }

    private void RemoveAt(int index)
    {
        var status = active[index];
        active.RemoveAt(index);
        UpdateVisual();
        OnStatusRemoved?.Invoke(status);
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = active.Count > 0 ? active[active.Count - 1].data.tint : baseColor;
    }
}
