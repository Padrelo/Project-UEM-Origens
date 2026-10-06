using UnityEngine;

/// <summary>
/// Projétil 2D. Prefab precisa de: Rigidbody2D (Gravity Scale 0),
/// Collider2D com "Is Trigger" marcado e este script.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile2D : MonoBehaviour
{
    [Tooltip("Camadas que fazem o projétil sumir ao bater (paredes, cenário)")]
    [SerializeField] private LayerMask obstacleLayers;

    private Rigidbody2D rb;
    private float damage;
    private bool ignoreDefense;
    private LayerMask targetLayers;
    private Transform shooterRoot;
    private GameObject hitEffectPrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Launch(Vector2 direction, float speed, float lifetime, float damage,
                       bool ignoreDefense, LayerMask targetLayers, Transform shooter,
                       GameObject hitEffectPrefab = null)
    {
        this.damage = damage;
        this.ignoreDefense = ignoreDefense;
        this.targetLayers = targetLayers;
        this.shooterRoot = shooter.root;
        this.hitEffectPrefab = hitEffectPrefab;

        direction.Normalize();
        transform.right = direction; // sprite apontando para a direita
        rb.velocity = direction * speed; // Unity 2023.1: ainda é "velocity" (linearVelocity só na Unity 6)

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root == shooterRoot) return;

        int otherLayerBit = 1 << other.gameObject.layer;

        if ((targetLayers.value & otherLayerBit) != 0)
        {
            CharacterStats target = other.GetComponentInParent<CharacterStats>();
            if (target == null || target.IsDead) return;

            target.TakeDamage(damage, ignoreDefense);
            SpawnHitEffect();
            Destroy(gameObject);
        }
        else if ((obstacleLayers.value & otherLayerBit) != 0)
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
    }
}
