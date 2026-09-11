using UnityEngine;

public class EncounterScareSound : MonoBehaviour
{
    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("Referências")]
    [Tooltip("Câmera do jogador.")]
    public Camera playerCamera;

    [Tooltip("Chappie que será detectado.")]
    public Transform chappie;


    // =========================================================
    // ÁUDIO
    // =========================================================

    [Header("Áudio do Susto")]
    public AudioSource audioSource;

    public AudioClip scareSound;

    [Range(0f, 1f)]
    public float volume = 1f;


    // =========================================================
    // TEMPO
    // =========================================================

    [Header("Tempo sem ver o Chappie")]
    [Tooltip("Tempo que o jogador precisa ficar sem ver o Chappie antes de um novo susto.")]
    public float timeWithoutSeeing = 30f;


    // =========================================================
    // DISTÂNCIA
    // =========================================================

    [Header("Distância")]
    [Tooltip("Distância máxima para o encontro poder ativar o som.")]
    public float maxDistance = 50f;


    // =========================================================
    // DETECÇÃO
    // =========================================================

    [Header("Detecção")]
    [Tooltip("Se ativado, o Chappie também precisa estar desobstruído por paredes.")]
    public bool requireLineOfSight = true;

    [Tooltip("Layers que podem bloquear a visão do Chappie.")]
    public LayerMask obstacleMask;


    // =========================================================
    // ESTADO
    // =========================================================

    private float timeSinceLastSeen = 0f;

    private bool scareReady = false;

    private bool wasVisible = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = volume;
        }


        timeSinceLastSeen = 0f;

        scareReady = false;

        wasVisible = false;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (playerCamera == null)
            return;

        if (chappie == null)
            return;


        bool chappieVisible =
            IsChappieVisible();


        // =====================================================
        // CHAPPIE ESTÁ VISÍVEL
        // =====================================================

        if (chappieVisible)
        {
            // -------------------------------------------------
            // SE ELE ACABOU DE APARECER
            // -------------------------------------------------

            if (!wasVisible)
            {
                if (scareReady)
                {
                    PlayScareSound();

                    scareReady = false;

                    Debug.Log(
                        "ENCOUNTER SCARE: Chappie apareceu novamente! " +
                        "Som de susto tocado."
                    );
                }
            }


            // -------------------------------------------------
            // ELE ESTÁ SENDO VISTO
            // -------------------------------------------------

            timeSinceLastSeen = 0f;
        }
        else
        {
            // =================================================
            // CHAPPIE NÃO ESTÁ VISÍVEL
            // =================================================

            timeSinceLastSeen +=
                Time.deltaTime;


            // -------------------------------------------------
            // ARMA O PRÓXIMO SUSTO
            // -------------------------------------------------

            if (
                !scareReady &&
                timeSinceLastSeen >=
                timeWithoutSeeing
            )
            {
                scareReady = true;

                Debug.Log(
                    "ENCOUNTER SCARE: " +
                    "Tempo sem ver o Chappie atingido. " +
                    "Próximo encontro pode tocar o susto."
                );
            }
        }


        // =====================================================
        // SALVA ESTADO ANTERIOR
        // =====================================================

        wasVisible =
            chappieVisible;
    }


    // =========================================================
    // VERIFICA SE CHAPPIE ESTÁ NA TELA
    // =========================================================

    private bool IsChappieVisible()
    {
        Vector3 viewportPosition =
            playerCamera.WorldToViewportPoint(
                chappie.position
            );


        // -----------------------------------------------------
        // ESTÁ ATRÁS DA CÂMERA
        // -----------------------------------------------------

        if (viewportPosition.z <= 0f)
            return false;


        // -----------------------------------------------------
        // FORA DA TELA
        // -----------------------------------------------------

        if (
            viewportPosition.x < 0f ||
            viewportPosition.x > 1f ||
            viewportPosition.y < 0f ||
            viewportPosition.y > 1f
        )
        {
            return false;
        }


        // -----------------------------------------------------
        // DISTÂNCIA
        // -----------------------------------------------------

        float distance =
            Vector3.Distance(
                playerCamera.transform.position,
                chappie.position
            );


        if (distance > maxDistance)
            return false;


        // -----------------------------------------------------
        // LINHA DE VISÃO
        // -----------------------------------------------------

        if (requireLineOfSight)
        {
            Vector3 direction =
                chappie.position -
                playerCamera.transform.position;


            float distanceToChappie =
                direction.magnitude;


            if (
                Physics.Raycast(
                    playerCamera.transform.position,
                    direction.normalized,
                    out RaycastHit hit,
                    distanceToChappie,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                return false;
            }
        }


        return true;
    }


    // =========================================================
    // TOCA O SOM
    // =========================================================

    private void PlayScareSound()
    {
        if (audioSource == null)
        {
            Debug.LogWarning(
                "EncounterScareSound: " +
                "Audio Source não configurado!"
            );

            return;
        }


        if (scareSound == null)
        {
            Debug.LogWarning(
                "EncounterScareSound: " +
                "Scare Sound não configurado!"
            );

            return;
        }


        audioSource.PlayOneShot(
            scareSound,
            volume
        );
    }
}