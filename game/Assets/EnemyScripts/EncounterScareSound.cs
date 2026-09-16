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
    public float timeWithoutSeeing = 30f;

    // =========================================================
    // DISTÂNCIA
    // =========================================================

    [Header("Distância")]
    public float maxDistance = 50f;

    // =========================================================
    // DETECÇÃO
    // =========================================================

    [Header("Detecção")]
    public bool requireLineOfSight = true;

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
        // =====================================================
        // STEALTH ESTÁ BLOQUEANDO O ENCOUNTER
        // =====================================================

        if (StealthScare.IsEncounterBlocked)
        {
            timeSinceLastSeen = 0f;
            scareReady = false;
            wasVisible = false;

            return;
        }

        if (playerCamera == null)
            return;

        if (chappie == null)
            return;

        bool chappieVisible =
            IsChappieVisible();

        // =====================================================
        // VISÍVEL
        // =====================================================

        if (chappieVisible)
        {
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

            timeSinceLastSeen = 0f;
        }
        else
        {
            timeSinceLastSeen +=
                Time.deltaTime;

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

        wasVisible =
            chappieVisible;
    }

    // =========================================================
    // VISIBILIDADE
    // =========================================================

    private bool IsChappieVisible()
    {
        Vector3 viewportPosition =
            playerCamera.WorldToViewportPoint(
                chappie.position
            );

        if (viewportPosition.z <= 0f)
            return false;

        if (
            viewportPosition.x < 0f ||
            viewportPosition.x > 1f ||
            viewportPosition.y < 0f ||
            viewportPosition.y > 1f
        )
        {
            return false;
        }

        float distance =
            Vector3.Distance(
                playerCamera.transform.position,
                chappie.position
            );

        if (distance > maxDistance)
            return false;

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
    // SOM
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