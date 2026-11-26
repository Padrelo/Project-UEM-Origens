using UnityEngine;

public enum StatusType { Burning, Poison, Stunned, Slowed, Regenerating }

// PerTurn: avança no fim de cada turno (combate por turnos)
// RealTime: avança com o tempo do jogo (combate em tempo real)
public enum TickMode { PerTurn, RealTime }

[CreateAssetMenu(fileName = "NovoStatus", menuName = "RPG/Status Effect")]
public class StatusEffectData : ScriptableObject
{
    [Header("Identificação")]
    public string displayName = "Queimando";
    public StatusType type = StatusType.Burning;
    public Sprite icon;
    public Color tint = new Color(1f, 0.55f, 0.3f);

    [Header("Duração")]
    public TickMode tickMode = TickMode.PerTurn;
    [Tooltip("Turnos (PerTurn) ou segundos (RealTime)")]
    public float duration = 3f;
    [Tooltip("Intervalo entre ticks, só para RealTime (segundos)")]
    public float tickInterval = 1f;

    [Header("Efeito por tick")]
    [Tooltip("Se verdadeiro, o dano do status ignora a defesa do personagem (bom para fogo/veneno)")]
    public bool ignoreDefense = true;
    [Tooltip("Positivo = dano, negativo = cura")]
    public float damagePerTick = 5f;

    [Header("Empilhamento")]
    [Tooltip("Se falso, reaplicar apenas renova a duração")]
    public bool stackable = false;
    public int maxStacks = 3;

    [Header("Efeitos de controle")]
    public bool preventsAction = false;            // atordoado, congelado...
    [Range(0.1f, 1f)] public float speedMultiplier = 1f; // lentidão
}
