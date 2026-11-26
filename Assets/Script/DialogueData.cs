using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueChoice
{
    public string text;
    [Tooltip("Índice do nó para onde esta escolha leva. -1 = encerra o diálogo.")]
    public int nextNode = -1;
}

[Serializable]
public class DialogueNode
{
    public string speaker;
    public Sprite portrait;
    [TextArea(2, 6)] public string text;

    [Tooltip("Índice do próximo nó. -1 = segue para o próximo da lista.")]
    public int nextNode = -1;
    [Tooltip("Marque para encerrar o diálogo depois deste nó.")]
    public bool endsDialogue;

    [Tooltip("Se tiver escolhas, elas aparecem no lugar do 'continuar'.")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    [Tooltip("Id opcional disparado quando o nó aparece (ex.: aceitar_missao, abrir_loja).")]
    public string eventId;
}

[CreateAssetMenu(fileName = "NovoDialogo", menuName = "RPG/Dialogue")]
public class DialogueData : ScriptableObject
{
    public List<DialogueNode> nodes = new List<DialogueNode>();
}
