using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition: MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private string sceneName;

    private void Awake()
    {
        //canvasGroup.gameObject.SetActive(false);
        FadeIn();
    }

    private void Start()
    {

    }

    public void FadeOut()
    {
        canvasGroup.gameObject.SetActive(true);
        StartCoroutine(FadeRoutine(0f, 1f, () => SceneManager.LoadScene(sceneName)));
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, Action finalFunction)
    {
        float elaspedTime =0F;
        canvasGroup.alpha = startAlpha;

        while (elaspedTime < fadeDuration)
        {
            elaspedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elaspedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        finalFunction();
    }

    void Update()
    {
        
    }

    public void FadeIn()
    {
        canvasGroup.gameObject.SetActive(true);
        StartCoroutine(FadeRoutine(1f, 0f, () => canvasGroup.gameObject.SetActive(false)));
    }
}
