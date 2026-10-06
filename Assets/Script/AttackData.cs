using UnityEngine;

public enum AttackType
{
    Melee,       // golpe em área à frente do personagem
    Projectile   // dispara um projétil
}

/// <summary>
/// Dados de um ataque/habilidade em TEMPO REAL.
/// Crie em: Assets > Create > RPG > Attack Data
/// </summary>
[CreateAssetMenu(fileName = "NovoAtaque", menuName = "RPG/Attack Data")]
public class AttackData : ScriptableObject
{
    public string attackName = "Ataque";
    public AttackType type = AttackType.Melee;

    [Header("Custo e tempo")]
    public float manaCost = 0f;
    public float cooldown = 0.5f;

    [Header("Dano")]
    public float damage = 10f;
    public bool ignoreDefense = false;
    [Range(0f, 1f)] public float critChance = 0.1f;
    public float critMultiplier = 2f;

    [Header("Alvos")]
    public LayerMask targetLayers;

    [Header("Corpo a corpo")]
    public float range = 1f;    // distância do centro do golpe até o personagem
    public float radius = 0.75f;

    [Header("Projétil")]
    public GameObject projectilePrefab; // precisa ter Projectile2D, Rigidbody2D e Collider2D (Is Trigger)
    public float projectileSpeed = 10f;
    public float projectileLifetime = 3f;

    [Header("Extras (opcional)")]
    public AudioClip sound;
    public GameObject hitEffectPrefab;
}
