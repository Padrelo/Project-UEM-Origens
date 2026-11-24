using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Um por cena, no Canvas de UI. Controla a caixa de diálogo do jogador local.
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject continueIndicator;   // setinha "▼" opcional
    [SerializeField] private Transform choicesContainer;     // objeto com Vertical Layout Group
    [SerializeField] private Button choiceButtonPrefab;      // botão com um TMP_Text filho

    [Header("Configuração")]
    [SerializeField] private float charsPerSecond = 40f;
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    public bool IsOpen { get; private set; }

    // Use para travar/destravar o movimento do jogador
    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;

    // Disparado quando um nó com eventId aparece (missões, loja, etc.)
    public static event Action<string> OnDialogueEvent;

    private DialogueData current;
    private int index;
    private Coroutine typing;
    private bool isTyping;
    private bool showingChoices;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(closeKey)) { End(); return; }
        if (Input.GetKeyDown(advanceKey) || Input.GetMouseButtonDown(0)) Advance();
    }

    // ---------- API pública ----------

    public void StartDialogue(DialogueData data)
    {
        if (IsOpen || data == null || data.nodes.Count == 0) return;

        current = data;
        IsOpen = true;
        panel.SetActive(true);
        OnDialogueStarted?.Invoke();
        ShowNode(0);
    }

    // Primeiro clique completa o texto; o seguinte avança.
    public void Advance()
    {
        if (!IsOpen || showingChoices) return;

        DialogueNode node = current.nodes[index];

        if (isTyping)
        {
            if (typing != null) StopCoroutine(typing);
            FinishTyping(node);
            return;
        }

        if (node.endsDialogue) { End(); return; }

        int next = node.nextNode >= 0 ? node.nextNode : index + 1;
        ShowNode(next);
    }

    public void End()
    {
        if (!IsOpen) return;

        if (typing != null) StopCoroutine(typing);
        ClearChoices();
        isTyping = false;
        showingChoices = false;
        IsOpen = false;
        current = null;
        panel.SetActive(false);
        OnDialogueEnded?.Invoke();
    }

    // ---------- Internos ----------

    private void ShowNode(int i)
    {
        if (i < 0 || i >= current.nodes.Count) { End(); return; }

        index = i;
        DialogueNode node = current.nodes[i];

        ClearChoices();
        showingChoices = false;
        if (continueIndicator != null) continueIndicator.SetActive(false);

        speakerText.text = node.speaker;
        if (portraitImage != null)
        {
            portraitImage.sprite = node.portrait;
            portraitImage.enabled = node.portrait != null;
        }

        if (!string.IsNullOrEmpty(node.eventId)) OnDialogueEvent?.Invoke(node.eventId);

        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(TypeText(node));
    }

    private IEnumerator TypeText(DialogueNode node)
    {
        isTyping = true;
        bodyText.text = node.text;
        bodyText.maxVisibleCharacters = 0;
        bodyText.ForceMeshUpdate();

        int total = bodyText.textInfo.characterCount;
        float delay = 1f / Mathf.Max(1f, charsPerSecond);

        for (int i = 1; i <= total; i++)
        {
            bodyText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(delay);
        }

        FinishTyping(node);
    }

    private void FinishTyping(DialogueNode node)
    {
        isTyping = false;
        bodyText.maxVisibleCharacters = int.MaxValue;

        if (node.choices != null && node.choices.Count > 0)
            ShowChoices(node);
        else if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }

    private void ShowChoices(DialogueNode node)
    {
        showingChoices = true;

        foreach (DialogueChoice choice in node.choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            button.GetComponentInChildren<TMP_Text>().text = choice.text;

            int next = choice.nextNode;
            button.onClick.AddListener(() => ChooseOption(next));
        }
    }

    private void ChooseOption(int nextNode)
    {
        showingChoices = false;
        ClearChoices();

        if (nextNode < 0) End();
        else ShowNode(nextNode);
    }

    private void ClearChoices()
    {
        if (choicesContainer == null) return;
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);
    }
}
