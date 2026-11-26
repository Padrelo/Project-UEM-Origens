using UnityEngine;

public class TesteDialogo : MonoBehaviour
{
    public enum ModoDeInicio { AoComecar, AoEntrarNaArea, AoInteragir }

    [SerializeField] private Dialogo dialogo;

    [Header("Falas")]
    [SerializeField, TextArea(2, 5)] private string[] falas;

    [Header("Configurações")]
    [SerializeField] private ModoDeInicio modo = ModoDeInicio.AoInteragir;
    [SerializeField] private KeyCode teclaInteragir = KeyCode.E;
    [SerializeField] private string tagDoJogador = "Player";
    [SerializeField] private bool apenasUmaVez = false;

    private bool jogadorNaArea;
    private bool jaIniciou;

    void Start()
    {
        if (modo == ModoDeInicio.AoComecar)
            IniciarDialogo();
    }

    void Update()
    {
        if (modo == ModoDeInicio.AoInteragir && jogadorNaArea && Input.GetKeyDown(teclaInteragir))
            IniciarDialogo();
    }

    // ---------- Colliders 3D ----------
    private void OnTriggerEnter(Collider other) => Entrou(other.gameObject);
    private void OnTriggerExit(Collider other) => Saiu(other.gameObject);

    // ---------- Colliders 2D (opcional) ----------
    private void OnTriggerEnter2D(Collider2D other) => Entrou(other.gameObject);
    private void OnTriggerExit2D(Collider2D other) => Saiu(other.gameObject);

    private void Entrou(GameObject obj)
    {
        if (!obj.CompareTag(tagDoJogador)) return;

        jogadorNaArea = true;

        if (modo == ModoDeInicio.AoEntrarNaArea)
            IniciarDialogo();
    }

    private void Saiu(GameObject obj)
    {
        if (obj.CompareTag(tagDoJogador))
            jogadorNaArea = false;
    }

    // Pode ser chamado por outros scripts ou por eventos de botão
    public void IniciarDialogo()
    {
        if (dialogo.Ativo) return;
        if (apenasUmaVez && jaIniciou) return;

        jaIniciou = true;
        dialogo.Iniciar(falas);
    }
}