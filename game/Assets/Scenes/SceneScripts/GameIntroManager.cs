using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class GameIntroManager : MonoBehaviour
{
    // =========================================================
    // VÍDEO
    // =========================================================

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;

    [SerializeField] private string videoName = "CutScene.mp4";

    [SerializeField] private bool PlayOrNot = true;


    // =========================================================
    // PULAR VÍDEO
    // =========================================================

    [Header("Pular Vídeo")]
    [Tooltip("Permite pular o vídeo apertando uma tecla.")]
    [SerializeField] private bool canSkipVideo = true;

    [Tooltip("Tecla usada para pular o vídeo.")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;


    // =========================================================
    // FADE
    // =========================================================

    [Header("Fade")]
    [Tooltip("Imagem preta usada para fazer o fade.")]
    [SerializeField] private Image fadeImage;

    [Tooltip("Velocidade do fade para preto.")]
    [SerializeField] private float fadeToBlackSpeed = 2f;

    [Tooltip("Velocidade do fade para revelar a imagem.")]
    [SerializeField] private float fadeFromBlackSpeed = 2f;


    // =========================================================
    // IMAGEM FINAL
    // =========================================================

    [Header("Imagem após o vídeo")]
    [Tooltip("Imagem que aparecerá depois que o vídeo terminar.")]
    [SerializeField] private Image finalImage;

    [Tooltip("Tempo que a imagem ficará na tela.")]
    [SerializeField] private float finalImageDuration = 4f;

    [Tooltip("Velocidade do fade final para preto.")]
    [SerializeField] private float finalFadeToBlackSpeed = 2f;


    // =========================================================
    // CONTROLE
    // =========================================================

    private bool videoFinished = false;
    private bool sequenceStarted = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // =====================================================
        // CONFIGURA FADE
        // =====================================================

        if (fadeImage != null)
        {
            Color color =
                fadeImage.color;

            color.a = 0f;

            fadeImage.color =
                color;

            fadeImage.raycastTarget =
                false;
        }


        // =====================================================
        // CONFIGURA IMAGEM FINAL
        // =====================================================

        if (finalImage != null)
        {
            Color color =
                finalImage.color;

            color.a = 0f;

            finalImage.color =
                color;

            finalImage.gameObject.SetActive(
                true
            );
        }


        // =====================================================
        // VÍDEO
        // =====================================================

        if (PlayOrNot)
        {
            Play();
        }
        else
        {
            SceneManager.LoadScene(
                "SampleScene"
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // =====================================================
        // PULAR VÍDEO
        // =====================================================

        if (!canSkipVideo)
            return;

        if (videoFinished)
            return;

        if (sequenceStarted)
            return;

        if (Input.GetKeyDown(skipKey))
        {
            SkipVideo();
        }
    }


    // =========================================================
    // PLAY
    // =========================================================

    public void Play()
    {
        videoPlayer.source =
            VideoSource.Url;

        videoPlayer.url =
            VideoManager.Instance.GetVideoUrl(
                videoName
            );

        videoPlayer.loopPointReached +=
            OnVideoFinished;

        videoPlayer.Play();
    }


    // =========================================================
    // PULAR VÍDEO
    // =========================================================

    private void SkipVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        StartFinalSequence();
    }


    // =========================================================
    // VÍDEO TERMINOU
    // =========================================================

    private void OnVideoFinished(
        VideoPlayer vp
    )
    {
        StartFinalSequence();
    }


    // =========================================================
    // COMEÇA SEQUÊNCIA FINAL
    // =========================================================

    private void StartFinalSequence()
    {
        if (sequenceStarted)
            return;

        sequenceStarted = true;
        videoFinished = true;

        StartCoroutine(
            FinalSequence()
        );
    }


    // =========================================================
    // SEQUÊNCIA FINAL
    // =========================================================

    private System.Collections.IEnumerator FinalSequence()
    {
        // =====================================================
        // ESCURECE PARA PRETO
        // =====================================================

        yield return StartCoroutine(
            FadeToBlack()
        );


        // =====================================================
        // GARANTE PRETO
        // =====================================================

        SetFadeAlpha(
            1f
        );


        // =====================================================
        // MOSTRA IMAGEM
        // =====================================================

        if (finalImage != null)
        {
            Color color =
                finalImage.color;

            color.a = 1f;

            finalImage.color =
                color;
        }


        // =====================================================
        // REVELA IMAGEM
        // =====================================================

        yield return StartCoroutine(
            FadeFromBlack()
        );


        // =====================================================
        // ESPERA IMAGEM
        // =====================================================

        yield return new WaitForSeconds(
            finalImageDuration
        );


        // =====================================================
        // FADE PARA PRETO NOVAMENTE
        // =====================================================

        yield return StartCoroutine(
            FadeToBlackFinal()
        );


        // =====================================================
        // CARREGA SAMPLESCENE
        // =====================================================

        SceneManager.LoadScene(
            "SampleScene"
        );
    }


    // =========================================================
    // FADE PARA PRETO
    // =========================================================

    private System.Collections.IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
            yield break;

        while (
            GetFadeAlpha() < 1f
        )
        {
            float alpha =
                Mathf.MoveTowards(
                    GetFadeAlpha(),
                    1f,
                    fadeToBlackSpeed *
                    Time.deltaTime
                );

            SetFadeAlpha(
                alpha
            );

            yield return null;
        }

        SetFadeAlpha(
            1f
        );
    }


    // =========================================================
    // REVELAR IMAGEM
    // =========================================================

    private System.Collections.IEnumerator FadeFromBlack()
    {
        if (fadeImage == null)
            yield break;

        while (
            GetFadeAlpha() > 0f
        )
        {
            float alpha =
                Mathf.MoveTowards(
                    GetFadeAlpha(),
                    0f,
                    fadeFromBlackSpeed *
                    Time.deltaTime
                );

            SetFadeAlpha(
                alpha
            );

            yield return null;
        }

        SetFadeAlpha(
            0f
        );
    }


    // =========================================================
    // FADE FINAL
    // =========================================================

    private System.Collections.IEnumerator FadeToBlackFinal()
    {
        if (fadeImage == null)
            yield break;

        while (
            GetFadeAlpha() < 1f
        )
        {
            float alpha =
                Mathf.MoveTowards(
                    GetFadeAlpha(),
                    1f,
                    finalFadeToBlackSpeed *
                    Time.deltaTime
                );

            SetFadeAlpha(
                alpha
            );

            yield return null;
        }

        SetFadeAlpha(
            1f
        );
    }


    // =========================================================
    // ALPHA DO FADE
    // =========================================================

    private float GetFadeAlpha()
    {
        if (fadeImage == null)
            return 1f;

        return fadeImage.color.a;
    }


    private void SetFadeAlpha(
        float alpha
    )
    {
        if (fadeImage == null)
            return;

        Color color =
            fadeImage.color;

        color.a =
            Mathf.Clamp01(alpha);

        fadeImage.color =
            color;
    }
}