using System.Collections;
using UnityEngine;

public class EyeOpening : MonoBehaviour
{
    // =========================================================
    // FAIXAS DOS OLHOS
    // =========================================================

    [Header("Faixas dos olhos")]
    public RectTransform topEyelid;
    public RectTransform bottomEyelid;


    // =========================================================
    // CONFIGURAÇÃO
    // =========================================================

    [Header("Abertura")]
    public float openSpeed = 1500f;

    [Tooltip("Pequena espera após carregar a cena antes de abrir os olhos.")]
    public float delayBeforeOpening = 0.1f;


    // =========================================================
    // POSIÇÕES
    // =========================================================

    private Vector2 topOpenPosition;
    private Vector2 bottomOpenPosition;


    // =========================================================
    // ESTADO
    // =========================================================

    private bool opening = false;
    private bool opened = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // =====================================================
        // FAIXA SUPERIOR
        // =====================================================

        if (topEyelid != null)
        {
            topOpenPosition =
                topEyelid.anchoredPosition +
                Vector2.up * 600f;
        }


        // =====================================================
        // FAIXA INFERIOR
        // =====================================================

        if (bottomEyelid != null)
        {
            bottomOpenPosition =
                bottomEyelid.anchoredPosition +
                Vector2.down * 600f;
        }


        // =====================================================
        // ESPERA A CENA CARREGAR
        // =====================================================

        StartCoroutine(
            StartOpening()
        );
    }


    // =========================================================
    // INICIAR ABERTURA
    // =========================================================

    private IEnumerator StartOpening()
    {
        // Espera a SampleScene terminar
        // de ser renderizada.

        yield return null;

        yield return new WaitForEndOfFrame();


        // Pequena espera adicional.

        if (delayBeforeOpening > 0f)
        {
            yield return new WaitForSeconds(
                delayBeforeOpening
            );
        }


        // Agora começa a abrir.

        opening = true;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!opening || opened)
            return;


        bool topFinished = true;
        bool bottomFinished = true;


        // =====================================================
        // FAIXA DE CIMA
        // =====================================================

        if (topEyelid != null)
        {
            topEyelid.anchoredPosition =
                Vector2.MoveTowards(
                    topEyelid.anchoredPosition,
                    topOpenPosition,
                    openSpeed * Time.deltaTime
                );


            topFinished =
                Vector2.Distance(
                    topEyelid.anchoredPosition,
                    topOpenPosition
                ) < 0.01f;
        }


        // =====================================================
        // FAIXA DE BAIXO
        // =====================================================

        if (bottomEyelid != null)
        {
            bottomEyelid.anchoredPosition =
                Vector2.MoveTowards(
                    bottomEyelid.anchoredPosition,
                    bottomOpenPosition,
                    openSpeed * Time.deltaTime
                );


            bottomFinished =
                Vector2.Distance(
                    bottomEyelid.anchoredPosition,
                    bottomOpenPosition
                ) < 0.01f;
        }


        // =====================================================
        // TERMINOU
        // =====================================================

        if (topFinished && bottomFinished)
        {
            opened = true;
            opening = false;

            Debug.Log(
                "EYES: Olhos abertos!"
            );
        }
    }
}