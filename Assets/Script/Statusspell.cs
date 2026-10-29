using UnityEngine;

// Projétil/área 2D genérico: causa dano direto e pode aplicar qualquer status.
// Precisa de um Collider2D (Is Trigger) e, em um dos lados, um Rigidbody2D.
// Use no fogo (asset Queimando), veneno (asset Envenenado) ou qualquer outro.
public class StatusSpell : MonoBehaviour
{
    [Header("Dano direto")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private bool ignoreDefense = false;

    [Header("Status")]
    [SerializeField] private StatusEffectData statusToApply;
    [SerializeField, Range(0f, 1f)] private float statusChance = 0.75f;

    [Header("Comportamento")]
    [SerializeField] private bool destroyOnHit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out CharacterStats stats))
        {
            if (stats.IsDead) return;
            stats.TakeDamage(damage, ignoreDefense);
        }

        if (statusToApply != null
            && other.TryGetComponent(out StatusEffectController status)
            && Random.value <= statusChance)
        {
            status.AddStatus(statusToApply);
        }

        if (destroyOnHit) Destroy(gameObject);
    }
}