using System;
using UnityEngine;

public enum SkillKind
{
    Attack,  // causa dano
    Defend   // reduz o dano de ataques recebidos
}

/// <summary>Faixa de um dado: rola um inteiro entre min e max (inclusive).</summary>
[Serializable]
public struct DieRange
{
    public int min;
    public int max;

    public int Roll() => UnityEngine.Random.Range(min, max + 1); // int é max-exclusivo, por isso +1
}

/// <summary>
/// Carta/habilidade do combate por turno.
/// Crie em: Assets > Create > RPG > Combat Skill
/// </summary>
[CreateAssetMenu(fileName = "NovaHabilidade", menuName = "RPG/Combat Skill")]
public class CombatSkill : ScriptableObject
{
    public string skillName = "Habilidade";
    public SkillKind kind = SkillKind.Attack;
    public float manaCost = 0f;

    [Tooltip("Um item por dado. Ex.: 3 dados de 2~5 = golpe de 3 acertos")]
    public DieRange[] dice = { new DieRange { min = 2, max = 5 } };

    [TextArea] public string description;
}
