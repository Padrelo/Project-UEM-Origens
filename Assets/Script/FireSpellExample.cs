using UnityEngine;

// Exemplo: projétil de fogo 2D. Precisa de Collider2D (Is Trigger) e Rigidbody2D.
public class FireSpellExample : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private StatusEffectData burning;          // arraste o asset "Queimando"
    [SerializeField, Range(0f, 1f)] private float burnChance = 0.75f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out CharacterStats stats))
            stats.TakeDamage(damage);

        if (other.TryGetComponent(out StatusEffectController status) && Random.value <= burnChance)
            status.AddStatus(burning);

        Destroy(gameObject);
    }
}

// ---------- Como usar no combate por turnos ----------
// No seu gerenciador de turnos, no fim do turno de cada personagem:
//     character.GetComponent<StatusEffectController>().OnTurnEnd();
//
// No início do turno, antes de liberar as ações:
//     if (!statusController.CanAct()) { /* pula o turno */ }
