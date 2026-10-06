using UnityEngine;

public class TesteDialogo : MonoBehaviour
{
    [SerializeField] private Dialogo dialogo;

    [Header("Falas")]
    [SerializeField, TextArea(2, 5)] private string[] falas;

    [Header("Configurações")]
    [SerializeField] private bool iniciarAoComecar = true;

    void Start()
    {
        if (iniciarAoComecar)
            IniciarDialogo();
    }

    // Pode ser chamado por outros scripts ou por eventos de botão
    public void IniciarDialogo()
    {
        dialogo.Iniciar(falas);
    }
}