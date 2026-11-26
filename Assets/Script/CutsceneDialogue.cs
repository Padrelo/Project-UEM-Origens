using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

// Dispara um diálogo SEM colisão: por Timeline (Signal), Animation Event, UnityEvent ou código.
// Basta chamar Play(). Quando o diálogo termina, retoma a Timeline (se houver) e dispara onFinished.
public class CutsceneDialogue : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;

    [Header("Cutscene")]
    [Tooltip("Se marcado, o jogador pode fechar o diálogo com Esc.")]
    [SerializeField] private bool playerCanSkip = false;
    [Tooltip("0 = espera o jogador (Espaço/clique). Maior que 0 = avança sozinho após esse tempo.")]
    [SerializeField] private float autoAdvanceSeconds = 0f;
    [Tooltip("Opcional: a Timeline é pausada durante o diálogo e retomada no fim.")]
    [SerializeField] private PlayableDirector director;

    public UnityEvent onFinished;

    private bool running;

    public void Play()
    {
        DialogueManager manager = DialogueManager.Instance;

        if (running) { Debug.LogWarning("[CutsceneDialogue] Já está tocando.", this); return; }
        if (dialogue == null) { Debug.LogWarning("[CutsceneDialogue] Nenhum DialogueData atribuído.", this); return; }
        if (dialogue.nodes.Count == 0) { Debug.LogWarning("[CutsceneDialogue] O diálogo não tem nós.", this); return; }
        if (manager == null) { Debug.LogWarning("[CutsceneDialogue] DialogueManager.Instance é nulo: o manager não está na cena, está desativado, ou o Play() rodou antes do Awake dele.", this); return; }
        if (manager.IsOpen) { Debug.LogWarning("[CutsceneDialogue] Já existe um diálogo aberto.", this); return; }

        running = true;
        if (director != null) director.Pause();

        manager.OnDialogueEnded += HandleEnded;
        manager.StartDialogue(dialogue, playerCanSkip, autoAdvanceSeconds);

        // Diálogo vazio/inválido: não abriu, então encerra aqui
        if (!manager.IsOpen) HandleEnded();
    }

    private void HandleEnded()
    {
        if (!running) return;
        running = false;

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueEnded -= HandleEnded;

        if (director != null) director.Resume();
        onFinished?.Invoke();
    }
}