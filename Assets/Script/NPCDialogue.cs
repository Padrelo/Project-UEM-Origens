using UnityEngine;

// Coloque no NPC junto com um Collider2D marcado como "Is Trigger" (área de interação).
// O jogador precisa ter a tag "Player" e um Collider2D + Rigidbody2D.
[RequireComponent(typeof(Collider2D))]
public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject interactPrompt; // ex.: um "[E] Falar" acima do NPC

    private bool playerInRange;

    private void Start()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange) return;

        DialogueManager manager = DialogueManager.Instance;
        if (manager == null) return;

        if (interactPrompt != null) interactPrompt.SetActive(!manager.IsOpen);

        if (!manager.IsOpen && Input.GetKeyDown(interactKey))
            manager.StartDialogue(dialogue);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsLocalPlayer(other)) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsLocalPlayer(other)) return;

        playerInRange = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // Saiu de perto: fecha o diálogo
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
            DialogueManager.Instance.End();
    }

    // No multiplayer, o trigger também detecta os outros jogadores.
    // Sobrescreva para aceitar só o SEU personagem (ex.: checar IsOwner / isLocalPlayer).
    protected virtual bool IsLocalPlayer(Collider2D other) => other.CompareTag(playerTag);
}
