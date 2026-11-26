using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testedialogo : MonoBehaviour
{
    [SerializeField] private CutsceneDialogue dialogue;
    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
       dialogue.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
