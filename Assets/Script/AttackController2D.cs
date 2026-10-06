using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ataques em tempo real 2D: corpo a corpo e projéteis.
/// Consome MP, respeita cooldown. Requer CharacterStats.
/// </summary>
[RequireComponent(typeof(CharacterStats))]
public class AttackController2D : MonoBehaviour
{
    [Header("Ataques")]
    [SerializeField] private AttackData basicAttack;
    [SerializeField] private List<AttackData> skills = new List<AttackData>();

    [Header("Origem do golpe/tiro (opcional)")]
    [SerializeField] private Transform attackOrigin;

    [Header("Mira")]
    [SerializeField] private bool aimWithMouse = true;
    [SerializeField] private Camera cam;

    [Header("Input Manager")]
    [SerializeField] private bool useInput = true; // desligue em inimigos controlados por IA
    [SerializeField] private KeyCode basicAttackKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode[] skillKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

    private CharacterStats stats;
    private Vector2 lastAimDirection = Vector2.right;
    private readonly Dictionary<AttackData, float> nextAvailableTime = new Dictionary<AttackData, float>();

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        if (attackOrigin == null) attackOrigin = transform;
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (!useInput || stats.IsDead) return;

        if (Input.GetKeyDown(basicAttackKey)) TryAttack(basicAttack);

        for (int i = 0; i < skillKeys.Length && i < skills.Count; i++)
            if (Input.GetKeyDown(skillKeys[i])) TryAttack(skills[i]);
    }

    private Vector2 GetAimDirection()
    {
        if (aimWithMouse && cam != null)
        {
            Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = mouseWorld - (Vector2)attackOrigin.position;
            if (dir.sqrMagnitude > 0.0001f) lastAimDirection = dir.normalized;
        }
        else
        {
            // sem mouse: usa o último input de movimento (ajuste conforme seu controle)
            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (move.sqrMagnitude > 0.01f) lastAimDirection = move.normalized;
        }
        return lastAimDirection;
    }

    public bool IsOnCooldown(AttackData attack) =>
        nextAvailableTime.TryGetValue(attack, out float t) && Time.time < t;

    /// <summary>Ataca na direção da mira atual.</summary>
    public bool TryAttack(AttackData attack) => TryAttack(attack, GetAimDirection());

    /// <summary>Ataca numa direção específica (útil para a IA dos inimigos).</summary>
    public bool TryAttack(AttackData attack, Vector2 direction)
    {
        if (attack == null || stats.IsDead || IsOnCooldown(attack)) return false;
        if (!stats.TryUseMana(attack.manaCost)) return false;

        nextAvailableTime[attack] = Time.time + attack.cooldown;
        direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : lastAimDirection;

        if (attack.sound != null)
            AudioSource.PlayClipAtPoint(attack.sound, attackOrigin.position);

        if (attack.type == AttackType.Projectile) FireProjectile(attack, direction);
        else MeleeHit(attack, direction);

        return true;
    }

    private float RollDamage(AttackData attack)
    {
        float dmg = attack.damage;
        if (Random.value < attack.critChance) dmg *= attack.critMultiplier;
        return dmg;
    }

    private void FireProjectile(AttackData attack, Vector2 direction)
    {
        if (attack.projectilePrefab == null)
        {
            Debug.LogWarning($"{attack.attackName}: projectilePrefab não definido.");
            return;
        }

        GameObject go = Instantiate(attack.projectilePrefab, attackOrigin.position, Quaternion.identity);
        Projectile2D proj = go.GetComponent<Projectile2D>();
        proj.Launch(direction, attack.projectileSpeed, attack.projectileLifetime,
                    RollDamage(attack), attack.ignoreDefense, attack.targetLayers,
                    transform, attack.hitEffectPrefab);
    }

    private void MeleeHit(AttackData attack, Vector2 direction)
    {
        Vector2 center = (Vector2)attackOrigin.position + direction * attack.range;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attack.radius, attack.targetLayers);

        foreach (Collider2D hit in hits)
        {
            if (hit.transform.root == transform.root) continue;

            CharacterStats target = hit.GetComponentInParent<CharacterStats>();
            if (target == null || target.IsDead) continue;

            target.TakeDamage(RollDamage(attack), attack.ignoreDefense);

            if (attack.hitEffectPrefab != null)
                Instantiate(attack.hitEffectPrefab, hit.ClosestPoint(center), Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (basicAttack == null || basicAttack.type != AttackType.Melee) return;
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin.position + origin.right * basicAttack.range, basicAttack.radius);
    }
}
