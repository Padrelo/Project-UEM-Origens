using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscroling : MonoBehaviour
{
    [SerializeField] private RectTransform rt;
    [SerializeField] private Transition transition;
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rt != null && rt.anchoredPosition.y > 650)
        {
            Debug.Log("GG");
            transition.FadeOut();
        }
    }
}
