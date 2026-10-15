using System.Collections;
using TMPro;
using UnityEngine;

public class Dialogo : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private TMP_Text textoUI;
    [SerializeField] private GameObject caixaDeDialogo;

    [Header("Configurações")]
    [SerializeField] private float tempoPorLetra = 0.04f;
    [SerializeField] private float pausaPontuacao = 0.25f;
    [SerializeField] private KeyCode teclaAvancar = KeyCode.Z;
    [SerializeField] private KeyCode teclaPularTudo = KeyCode.X;

    private string[] falas;
    private int indiceFala;
    private Coroutine rotinaDigitar;

    public bool Digitando { get; private set; }
    public bool Ativo { get; private set; }

    // Chame este método para iniciar uma conversa
    public void Iniciar(string[] novasFalas)
    {
        falas = novasFalas;
        indiceFala = 0;
        Ativo = true;
        caixaDeDialogo.SetActive(true);
        MostrarFala();
    }

    private void Update()
    {
        if (!Ativo) return;

        // X pula o diálogo inteiro, a qualquer momento
        if (Input.GetKeyDown(teclaPularTudo))
        {
            PularDialogo();
            return;
        }

        if (Digitando)
        {
            // Z completa a fala atual
            if (Input.GetKeyDown(teclaAvancar))
                CompletarFala();
        }
        else if (Input.GetKeyDown(teclaAvancar))
        {
            ProximaFala();
        }
    }

    private void MostrarFala()
    {
        if (rotinaDigitar != null) StopCoroutine(rotinaDigitar);
        rotinaDigitar = StartCoroutine(Digitar(falas[indiceFala]));
    }

    private IEnumerator Digitar(string fala)
    {
        Digitando = true;
        textoUI.text = fala;
        textoUI.ForceMeshUpdate();
        textoUI.maxVisibleCharacters = 0;

        int total = textoUI.textInfo.characterCount;

        for (int i = 0; i < total; i++)
        {
            textoUI.maxVisibleCharacters = i + 1;
            char c = textoUI.textInfo.characterInfo[i].character;

            float espera = tempoPorLetra;
            if (c == '.' || c == '!' || c == '?' || c == ',')
                espera += pausaPontuacao;

            yield return new WaitForSeconds(espera);
        }

        Digitando = false;
    }

    private void CompletarFala()
    {
        if (rotinaDigitar != null) StopCoroutine(rotinaDigitar);
        textoUI.maxVisibleCharacters = int.MaxValue;
        Digitando = false;
    }

    private void ProximaFala()
    {
        indiceFala++;
        if (indiceFala < falas.Length)
            MostrarFala();
        else
            EncerrarDialogo();
    }

    private void PularDialogo()
    {
        if (rotinaDigitar != null) StopCoroutine(rotinaDigitar);
        Digitando = false;
        EncerrarDialogo();
    }

    private void EncerrarDialogo()
    {
        Ativo = false;
        caixaDeDialogo.SetActive(false);
    }
}
//Como chamar a conversa

/*public class NPC : MonoBehaviour
{
    [SerializeField] private Dialogo dialogo;

    void Start()
    {
        dialogo.Iniciar(new string[]
        {
            "Olá, viajante!",
            "Você <color=red>nunca</color> deveria ter vindo aqui...",
            "Mas já que veio, vamos lutar!"
        });
    }
}*/