using System.Collections;
using TMPro;
using UnityEngine;

public class Dialogo : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private TMP_Text textoUI;
    [SerializeField] private GameObject caixaDeDialogo;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somLetra;

    [Header("Configurações")]
    [SerializeField] private float tempoPorLetra = 0.04f;
    [SerializeField] private float pausaPontuacao = 0.25f;
    [SerializeField] private KeyCode teclaAvancar = KeyCode.Z;
    [SerializeField] private KeyCode teclaPular = KeyCode.X;

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

        if (Digitando)
        {
            // Pula a animação e mostra a fala inteira
            if (Input.GetKeyDown(teclaPular) || Input.GetKeyDown(teclaAvancar))
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

            if (!char.IsWhiteSpace(c) && somLetra != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(somLetra);
            }

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
        {
            MostrarFala();
        }
        else
        {
            Ativo = false;
            caixaDeDialogo.SetActive(false);
        }
    }
}

Como configurar

Crie um Canvas (GameObject > UI > Canvas) e dentro dele um Panel para ser a caixa de diálogo (fundo preto com borda branca fica bem estilo Deltarune).
Dentro do Panel, adicione um Text - TextMeshPro (GameObject > UI > Text - TextMeshPro). Se o Unity pedir para importar o TMP Essentials, aceite.
Crie um objeto vazio chamado GerenciadorDialogo, adicione o script Dialogo e um AudioSource.
Arraste o texto, o Panel, o AudioSource e um som curto de "blip" (um bipe de 0.05s) para os campos do script.

Como chamar a conversa

csharp
public class NPC : MonoBehaviour
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
}





//  codigo para puxar o dialogo
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