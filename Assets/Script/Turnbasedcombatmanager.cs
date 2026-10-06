using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Combate por turno com dados de velocidade, escolhas e confronto de dados
/// (inspirado em Library of Ruina / Limbus Company).
///
/// Fluxo de cada turno:
///  1) PLANEJAMENTO: todos rolam dados de velocidade. A IA escolhe sozinha;
///     o jogador chama Combatant.Assign(...) (ex.: pelos botões da UI).
///  2) Ao chamar ConfirmPlan(), começa a RESOLUÇÃO: os slots são executados
///     do mais rápido para o mais lento.
///
/// Regras (simplificadas e fáceis de ajustar):
///  - ATAQUE x ATAQUE (ambos se escolheram) = CONFRONTO: cada lado rola um dado por rodada;
///    quem tirar menos perde um dado (empate: ambos perdem um). Quando um lado fica sem dados,
///    os dados restantes do vencedor acertam sem oposição.
///  - ATAQUE x DEFESA: cada dado de defesa reduz o dano de um dado de ataque (mín. 0).
///  - ATAQUE sem oposição: todos os dados causam dano total.
///  - Mana é gasta quando o slot entra em ação; sem mana, o slot falha.
/// </summary>
public class TurnBasedCombatManager : MonoBehaviour
{
    public enum Phase { Planning, Resolving, Finished }

    [SerializeField] private List<Combatant> playerTeam = new List<Combatant>();
    [SerializeField] private List<Combatant> enemyTeam = new List<Combatant>();
    [SerializeField] private float stepDelay = 0.6f; // pausa entre rolagens (para animações/UI)

    public IReadOnlyList<Combatant> PlayerTeam => playerTeam;
    public IReadOnlyList<Combatant> EnemyTeam => enemyTeam;

    public Phase CurrentPhase { get; private set; }
    public int TurnNumber { get; private set; }

    // Eventos para a UI/animações
    public event Action<int> OnTurnStarted;                         // número do turno
    public event Action<CombatSlot, CombatSlot, int, int> OnClash;  // slotA, slotB, rolagemA, rolagemB
    public event Action<CombatSlot, Combatant, int> OnHit;          // quem atacou, alvo, dano final
    public event Action<string> OnLog;                              // texto para log de combate
    public event Action<bool> OnCombatEnded;                        // true = jogador venceu

    private void Start() => BeginPlanning();

    // ---------------- PLANEJAMENTO ----------------
    public void BeginPlanning()
    {
        CurrentPhase = Phase.Planning;
        TurnNumber++;

        foreach (var c in playerTeam.Concat(enemyTeam).Where(c => c.IsAlive))
            c.RollSpeedDice();

        // IA planeja automaticamente; jogador escolhe pela UI
        foreach (var c in playerTeam.Where(c => c.IsAlive && !c.IsPlayerControlled))
            c.AutoPlan(enemyTeam);
        foreach (var c in enemyTeam.Where(c => c.IsAlive))
            c.AutoPlan(playerTeam);

        OnTurnStarted?.Invoke(TurnNumber);
        Log($"--- Turno {TurnNumber} ---");
    }

    /// <summary>Chame quando o jogador terminar de escolher (botão "Confirmar").</summary>
    public void ConfirmPlan()
    {
        if (CurrentPhase != Phase.Planning) return;
        StartCoroutine(ResolveTurn());
    }

    // ---------------- RESOLUÇÃO ----------------
    private IEnumerator ResolveTurn()
    {
        CurrentPhase = Phase.Resolving;

        // Todos os slots com habilidade, do mais rápido ao mais lento (empate: aleatório)
        List<CombatSlot> order = playerTeam.Concat(enemyTeam)
            .Where(c => c.IsAlive)
            .SelectMany(c => c.Slots)
            .Where(s => s.skill != null)
            .OrderByDescending(s => s.speed)
            .ThenBy(_ => UnityEngine.Random.value)
            .ToList();

        foreach (CombatSlot slot in order)
        {
            if (slot.resolved || !slot.owner.IsAlive) continue;
            if (slot.skill.kind == SkillKind.Defend) continue; // defesa só age quando alguém ataca

            if (!slot.owner.Stats.TryUseMana(slot.skill.manaCost))
            {
                slot.resolved = true;
                Log($"{slot.owner.name} não tem mana para {slot.skill.skillName}!");
                continue;
            }

            Combatant target = slot.target;
            if (target == null || !target.IsAlive)
            {
                slot.resolved = true;
                Log($"{slot.owner.name}: alvo inválido, {slot.skill.skillName} falhou.");
                continue;
            }

            slot.resolved = true;
            Log($"{slot.owner.name} usa {slot.skill.skillName} em {target.name} (vel. {slot.speed})");

            CombatSlot counter = FindCounterAttack(slot, target);
            if (counter != null)
            {
                yield return StartCoroutine(Clash(slot, counter));
            }
            else
            {
                CombatSlot defense = FindDefense(target);
                if (defense != null) yield return StartCoroutine(AttackVsDefense(slot, defense));
                else yield return StartCoroutine(UnopposedAttack(slot, target));
            }

            if (CheckCombatEnd()) yield break;
            yield return new WaitForSeconds(stepDelay);
        }

        BeginPlanning();
    }

    /// <summary>Slot de ataque do alvo, ainda não usado, que mira em quem está atacando (gera confronto).</summary>
    private CombatSlot FindCounterAttack(CombatSlot attacker, Combatant target)
    {
        foreach (CombatSlot s in target.Slots.OrderByDescending(s => s.speed))
        {
            if (s.resolved || s.skill == null || s.skill.kind != SkillKind.Attack) continue;
            if (s.target != attacker.owner) continue;
            if (!s.owner.Stats.TryUseMana(s.skill.manaCost)) { s.resolved = true; continue; }

            s.resolved = true;
            return s;
        }
        return null;
    }

    /// <summary>Slot de defesa livre do alvo.</summary>
    private CombatSlot FindDefense(Combatant target)
    {
        foreach (CombatSlot s in target.Slots)
        {
            if (s.resolved || s.skill == null || s.skill.kind != SkillKind.Defend) continue;
            if (!s.owner.Stats.TryUseMana(s.skill.manaCost)) { s.resolved = true; continue; }

            s.resolved = true;
            return s;
        }
        return null;
    }

    // ---- Confronto de dados (ataque x ataque) ----
    private IEnumerator Clash(CombatSlot a, CombatSlot b)
    {
        DieRange[] diceA = a.skill.dice;
        DieRange[] diceB = b.skill.dice;
        int ia = 0, ib = 0;

        Log($"CONFRONTO: {a.owner.name} ({a.skill.skillName}) x {b.owner.name} ({b.skill.skillName})");

        while (ia < diceA.Length && ib < diceB.Length)
        {
            int rollA = diceA[ia].Roll();
            int rollB = diceB[ib].Roll();
            OnClash?.Invoke(a, b, rollA, rollB);
            Log($"  {a.owner.name} {rollA} x {rollB} {b.owner.name}");

            if (rollA > rollB) ib++;          // B perde um dado
            else if (rollB > rollA) ia++;     // A perde um dado
            else { ia++; ib++; }              // empate: ambos perdem um

            yield return new WaitForSeconds(stepDelay);
        }

        // Dados que sobraram do vencedor acertam sem oposição
        for (; ia < diceA.Length; ia++) DealDamage(a, b.owner, diceA[ia].Roll());
        for (; ib < diceB.Length; ib++) DealDamage(b, a.owner, diceB[ib].Roll());
    }

    // ---- Ataque contra defesa ----
    private IEnumerator AttackVsDefense(CombatSlot attack, CombatSlot defense)
    {
        DieRange[] atkDice = attack.skill.dice;
        DieRange[] defDice = defense.skill.dice;
        Log($"  {defense.owner.name} defende com {defense.skill.skillName}");

        for (int i = 0; i < atkDice.Length; i++)
        {
            int atk = atkDice[i].Roll();
            int def = i < defDice.Length ? defDice[i].Roll() : 0;
            if (i < defDice.Length) OnClash?.Invoke(attack, defense, atk, def);

            DealDamage(attack, defense.owner, Mathf.Max(0, atk - def), atk);
            yield return new WaitForSeconds(stepDelay * 0.5f);
        }
    }

    // ---- Ataque sem oposição ----
    private IEnumerator UnopposedAttack(CombatSlot attack, Combatant target)
    {
        foreach (DieRange die in attack.skill.dice)
        {
            DealDamage(attack, target, die.Roll());
            yield return new WaitForSeconds(stepDelay * 0.5f);
        }
    }

    private void DealDamage(CombatSlot source, Combatant target, int damage, int rawRoll = -1)
    {
        if (!target.IsAlive) return;

        // ignoreDefense = true: aqui a redução já é feita pelos dados de defesa
        target.Stats.TakeDamage(damage, true);
        OnHit?.Invoke(source, target, damage);
        Log($"  {source.owner.name} causa {damage} de dano em {target.name}" +
            (rawRoll >= 0 ? $" (rolou {rawRoll})" : ""));
    }

    // ---------------- FIM DE COMBATE ----------------
    private bool CheckCombatEnd()
    {
        bool playersAlive = playerTeam.Any(c => c.IsAlive);
        bool enemiesAlive = enemyTeam.Any(c => c.IsAlive);
        if (playersAlive && enemiesAlive) return false;

        CurrentPhase = Phase.Finished;
        Log(playersAlive ? "Vitória!" : "Derrota...");
        OnCombatEnded?.Invoke(playersAlive);
        return true;
    }

    private void Log(string message)
    {
        Debug.Log(message);
        OnLog?.Invoke(message);
    }
}
