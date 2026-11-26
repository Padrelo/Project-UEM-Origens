using UnityEngine;

// Força a área visível da câmera a manter sempre 16:9, independente da resolução real
// da tela do jogador. Se a tela for mais larga que 16:9, adiciona barras pretas nas
// laterais (pillarbox); se for mais alta/estreita, adiciona em cima/embaixo (letterbox).
// Anexar na Main Camera.
[RequireComponent(typeof(Camera))]
public class AutoAdjustCameraRatio: MonoBehaviour
{
    private const float TargetAspect = 16f / 9f;

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyAspectRatio();
    }

    private void Update()
    {
        // Só reprocessa se a resolução da tela mudou (ex: jogador redimensionou a janela),
        // evita recalcular todo frame à toa
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            ApplyAspectRatio();
    }

    private void ApplyAspectRatio()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / TargetAspect;

        if (scaleHeight < 1f)
        {
            // Tela mais estreita/alta que 16:9: barras em cima e embaixo (letterbox)
            Rect rect = cam.rect;

            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) / 2f;

            cam.rect = rect;
        }
        else
        {
            // Tela mais larga que 16:9: barras nas laterais (pillarbox)
            float scaleWidth = 1f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0f;

            cam.rect = rect;
        }
    }
}
