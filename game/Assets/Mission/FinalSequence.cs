using System.Collections;
using UnityEngine;

public class FinalSequence : MonoBehaviour
{
    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("UI")]
    public CanvasGroup fadeCanvasGroup;

    public CanvasGroup finalImageCanvasGroup;

    public GameObject menuButton;


    // =========================================================
    // CONFIGURAÇÃO
    // =========================================================

    [Header("Fade para Preto")]
    [Tooltip("Velocidade com que a tela fica preta.")]
    public float fadeToBlackSpeed = 2f;


    [Header("Fade da Imagem")]
    [Tooltip("Tempo que a imagem fica preparada antes de remover o preto.")]
    public float imageDisplayDelay = 0.5f;


    [Header("Fade para Revelar Imagem")]
    [Tooltip("Velocidade com que o preto desaparece.")]
    public float fadeFromBlackSpeed = 2f;


    // =========================================================
    // ESTADO
    // =========================================================

    private bool sequenceStarted = false;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // =====================================================
        // IMPORTANTE:
        // A cena começa PRETA imediatamente.
        // =====================================================

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;

            fadeCanvasGroup.interactable = false;

            fadeCanvasGroup.blocksRaycasts = true;
        }


        // =====================================================
        // IMAGEM FINAL
        // =====================================================

        if (finalImageCanvasGroup != null)
        {
            // A imagem já fica preparada atrás do preto.

            finalImageCanvasGroup.alpha = 1f;

            finalImageCanvasGroup.interactable = false;

            finalImageCanvasGroup.blocksRaycasts = false;
        }


        // =====================================================
        // BOTÃO
        // =====================================================

        if (menuButton != null)
        {
            menuButton.SetActive(false);
        }


        // =====================================================
        // CURSOR
        // =====================================================

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        StartCoroutine(
            FinalRoutine()
        );
    }


    // =========================================================
    // SEQUÊNCIA FINAL
    // =========================================================

    private IEnumerator FinalRoutine()
    {
        if (sequenceStarted)
            yield break;


        sequenceStarted = true;


        // =====================================================
        // GARANTE QUE A CENA COMEÇOU PRETA
        // =====================================================

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;

            fadeCanvasGroup.blocksRaycasts = true;
        }


        // =====================================================
        // PEQUENA PAUSA NO PRETO
        // =====================================================

        yield return new WaitForSeconds(
            imageDisplayDelay
        );


        // =====================================================
        // REVELA A IMAGEM
        // =====================================================

        if (fadeCanvasGroup != null)
        {
            while (
                fadeCanvasGroup.alpha > 0f
            )
            {
                fadeCanvasGroup.alpha =
                    Mathf.MoveTowards(
                        fadeCanvasGroup.alpha,
                        0f,
                        fadeFromBlackSpeed *
                        Time.deltaTime
                    );


                yield return null;
            }


            fadeCanvasGroup.alpha = 0f;

            fadeCanvasGroup.blocksRaycasts = false;
        }


        // =====================================================
        // MOSTRA BOTÃO
        // =====================================================

        if (menuButton != null)
        {
            menuButton.SetActive(true);
        }


        // =====================================================
        // LIBERA UI
        // =====================================================

        if (finalImageCanvasGroup != null)
        {
            finalImageCanvasGroup.interactable = true;

            finalImageCanvasGroup.blocksRaycasts = true;
        }


        // =====================================================
        // CURSOR
        // =====================================================

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }
}