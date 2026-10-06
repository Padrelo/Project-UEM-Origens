using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// UI mínima de TESTE para o combate por turno (usa OnGUI, não precisa de Canvas).
/// Serve para validar as regras antes de montar a UI final.
/// </summary>
public class CombatDebugUI : MonoBehaviour
{
    [SerializeField] private TurnBasedCombatManager manager;

    private readonly List<string> log = new List<string>();
    private int selectedEnemy;
    private Vector2 logScroll;

    private void OnEnable()
    {
        if (manager != null) manager.OnLog += AddLog;
    }

    private void OnDisable()
    {
        if (manager != null) manager.OnLog -= AddLog;
    }

    private void AddLog(string msg)
    {
        log.Add(msg);
        if (log.Count > 60) log.RemoveAt(0);
        logScroll.y = float.MaxValue; // rola para o fim
    }

    private void OnGUI()
    {
        if (manager == null) return;

        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

        GUILayout.Label($"<b>Turno {manager.TurnNumber} — {manager.CurrentPhase}</b>");

        DrawTeam("JOGADORES", manager.PlayerTeam, showPlans: true);
        DrawTeam("INIMIGOS", manager.EnemyTeam, showPlans: true); // mostrar a intenção do inimigo, como em Ruina

        if (manager.CurrentPhase == TurnBasedCombatManager.Phase.Planning)
            DrawPlanning();

        GUILayout.Space(8);
        logScroll = GUILayout.BeginScrollView(logScroll, GUILayout.Height(160));
        foreach (string line in log) GUILayout.Label(line);
        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }

    // ---- Mostra HP/MP e o plano de cada combatente ----
    private void DrawTeam(string title, IReadOnlyList<Combatant> team, bool showPlans)
    {
        GUILayout.Label($"== {title} ==");
        foreach (Combatant c in team)
        {
            string status = c.IsAlive ? "" : " (derrotado)";
            GUILayout.Label($"{c.name}  HP {c.Stats.CurrentHealth:0}/{c.Stats.MaxHealth:0}   " +
                            $"MP {c.Stats.CurrentMana:0}/{c.Stats.MaxMana:0}{status}");

            if (!showPlans || !c.IsAlive) continue;
            for (int i = 0; i < c.Slots.Count; i++)
            {
                CombatSlot s = c.Slots[i];
                string plan = s.skill == null
                    ? "— vazio —"
                    : $"{s.skill.skillName} → {(s.target != null ? s.target.name : "?")}";
                GUILayout.Label($"     Slot {i + 1} [vel {s.speed}]: {plan}");
            }
        }
    }

    // ---- Escolhas do jogador ----
    private void DrawPlanning()
    {
        List<Combatant> aliveEnemies = manager.EnemyTeam.Where(e => e.IsAlive).ToList();
        if (aliveEnemies.Count == 0) return;

        GUILayout.Space(6);
        GUILayout.Label("ALVO dos ataques:");
        selectedEnemy = Mathf.Clamp(selectedEnemy, 0, aliveEnemies.Count - 1);
        selectedEnemy = GUILayout.Toolbar(selectedEnemy, aliveEnemies.Select(e => e.name).ToArray());
        Combatant target = aliveEnemies[selectedEnemy];

        foreach (Combatant c in manager.PlayerTeam.Where(c => c.IsAlive && c.IsPlayerControlled))
        {
            GUILayout.Label($"Escolha para {c.name}:");
            for (int i = 0; i < c.Slots.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Slot {i + 1} [vel {c.Slots[i].speed}]", GUILayout.Width(110));

                foreach (CombatSkill skill in c.Deck)
                {
                    if (GUILayout.Button($"{skill.skillName} ({skill.manaCost:0} MP)"))
                        c.Assign(i, skill, target);
                }

                if (GUILayout.Button("Limpar")) c.ClearSlot(i);
                GUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("CONFIRMAR TURNO", GUILayout.Height(32)))
            manager.ConfirmPlan();
    }
}
