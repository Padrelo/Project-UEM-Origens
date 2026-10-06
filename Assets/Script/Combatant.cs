using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Um "slot" = um dado de velocidade + a habilidade escolhida + o alvo.</summary>
public class CombatSlot
{
    public Combatant owner;
    public int speed;
    public CombatSkill skill;
    public Combatant target;
    public bool resolved;
}

/// <summary>
/// Participante do combate por turno. Requer CharacterStats (HP/MP).
/// A cada turno rola N dados de velocidade; cada um vira um slot onde
/// o jogador (ou a IA) escolhe uma habilidade e um alvo.
/// </summary>
[RequireComponent(typeof(CharacterStats))]
public class Combatant : MonoBehaviour
{
    [Header("Dados de velocidade")]
    [SerializeField] private int speedDiceCount = 2;
    [SerializeField] private int minSpeed = 1;
    [SerializeField] private int maxSpeed = 6;

    [Header("Controle")]
    [SerializeField] private bool isPlayerControlled = true;

    [Header("Habilidades disponíveis")]
    [SerializeField] private List<CombatSkill> deck = new List<CombatSkill>();

    public CharacterStats Stats { get; private set; }
    public bool IsPlayerControlled => isPlayerControlled;
    public IReadOnlyList<CombatSkill> Deck => deck;
    public List<CombatSlot> Slots { get; } = new List<CombatSlot>();
    public bool IsAlive => !Stats.IsDead;

    private void Awake() => Stats = GetComponent<CharacterStats>();

    /// <summary>Início do turno: limpa slots e rola os dados de velocidade.</summary>
    public void RollSpeedDice()
    {
        Slots.Clear();
        for (int i = 0; i < speedDiceCount; i++)
        {
            Slots.Add(new CombatSlot
            {
                owner = this,
                speed = Random.Range(minSpeed, maxSpeed + 1)
            });
        }
    }

    /// <summary>Mana já "reservada" pelas escolhas atuais (ignorando um slot, se informado).</summary>
    public float PlannedManaCost(int ignoreSlotIndex = -1)
    {
        float total = 0f;
        for (int i = 0; i < Slots.Count; i++)
        {
            if (i == ignoreSlotIndex || Slots[i].skill == null) continue;
            total += Slots[i].skill.manaCost;
        }
        return total;
    }

    /// <summary>
    /// ESCOLHA do jogador: atribui habilidade e alvo a um slot.
    /// Para habilidades de defesa, o alvo pode ser o próprio combatente.
    /// </summary>
    public bool Assign(int slotIndex, CombatSkill skill, Combatant target)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Count || skill == null) return false;

        float totalCost = PlannedManaCost(slotIndex) + skill.manaCost;
        if (totalCost > Stats.CurrentMana) return false; // sem mana para o plano inteiro

        Slots[slotIndex].skill = skill;
        Slots[slotIndex].target = skill.kind == SkillKind.Defend ? this : target;
        return true;
    }

    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Count) return;
        Slots[slotIndex].skill = null;
        Slots[slotIndex].target = null;
    }

    /// <summary>IA simples: escolhe habilidade aleatória que dá para pagar e um inimigo vivo aleatório.</summary>
    public void AutoPlan(List<Combatant> enemies)
    {
        var aliveEnemies = enemies.Where(e => e != null && e.IsAlive).ToList();
        if (aliveEnemies.Count == 0 || deck.Count == 0) return;

        for (int i = 0; i < Slots.Count; i++)
        {
            var options = deck.Where(s => PlannedManaCost(i) + s.manaCost <= Stats.CurrentMana).ToList();
            if (options.Count == 0) break;

            CombatSkill skill = options[Random.Range(0, options.Count)];
            Combatant target = aliveEnemies[Random.Range(0, aliveEnemies.Count)];
            Assign(i, skill, target);
        }
    }
}
