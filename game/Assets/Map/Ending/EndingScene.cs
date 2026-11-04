using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingSequence : MonoBehaviour
{
    [Header("Movimento do jogador")]
    public Behaviour[] playerMovementScripts;

    [Header("Metralhadoras - WeaponMain")]
    public Transform[] weaponMains;

    [Header("Animação")]
    public float animationDuration = 1f;
    public float startY = -3.453f;
    public float targetY = 0f;

    [Header("Som da animação")]
    public AudioSource animationAudioSource;
    public AudioClip animationSound;

    [Header("Finalização")]
    public float sceneChangeDelay = 2f;
    public string finalScene = "FinalScene";

    private bool sequenceStarted = false;

    private Vector3[] initialPositions;

    private MusicManager musicManager;

    private PlayerMovement playerMovement;
    private CameraEffects cameraEffects;

    private void Start()
    {
        initialPositions = new Vector3[weaponMains.Length];

        for (int i = 0; i < weaponMains.Length; i++)
        {
            if (weaponMains[i] != null)
            {
                initialPositions[i] = weaponMains[i].localPosition;

                Vector3 position = weaponMains[i].localPosition;
                position.y = startY;
                weaponMains[i].localPosition = position;
            }
        }

        musicManager = FindFirstObjectByType<MusicManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sequenceStarted) return;

        if (other.CompareTag("Player"))
        {
            playerMovement =
                other.GetComponentInParent<PlayerMovement>();

            if (playerMovement != null)
            {
                cameraEffects =
                    playerMovement.GetComponentInChildren<CameraEffects>(true);
            }

            StartCoroutine(EndingSequenceRoutine());
        }
    }

    private IEnumerator EndingSequenceRoutine()
    {
        sequenceStarted = true;

        // =====================================================
        // PARA A MÚSICA DO MUSIC MANAGER
        // =====================================================

        if (musicManager != null)
        {
            musicManager.StopAllCoroutines();

            if (musicManager.ambientSource != null)
            {
                musicManager.ambientSource.Stop();
            }

            if (musicManager.enemySourceA != null)
            {
                musicManager.enemySourceA.Stop();
            }

            if (musicManager.enemySourceB != null)
            {
                musicManager.enemySourceB.Stop();
            }

            musicManager.enabled = false;
        }

        // =====================================================
        // PREPARA O JOGADOR PARA A FINALIZAÇÃO
        // =====================================================

        if (playerMovement != null)
        {
            playerMovement.BeginEndingSequence();
        }

        // Desativa os controles, mantendo a gravidade
        // e os efeitos da câmera funcionando temporariamente.

        foreach (Behaviour script in playerMovementScripts)
        {
            if (script != null &&
                script != playerMovement &&
                script != cameraEffects)
            {
                script.enabled = false;
            }
        }

        // =====================================================
        // ESPERA O JOGADOR TOCAR O CHÃO
        // =====================================================

        if (playerMovement != null)
        {
            while (!playerMovement.IsGroundedForEnding)
            {
                yield return null;
            }
        }

        // =====================================================
        // ESPERA A CÂMERA VOLTAR À POSIÇÃO NORMAL
        // =====================================================

        if (cameraEffects != null && cameraEffects.enabled)
        {
            while (
                cameraEffects.transform.localPosition.sqrMagnitude >
                0.0001f
            )
            {
                yield return null;
            }

            cameraEffects.enabled = false;
        }

        // =====================================================
        // DESATIVA DEFINITIVAMENTE OS CONTROLES
        // =====================================================

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        foreach (Behaviour script in playerMovementScripts)
        {
            if (script != null)
            {
                script.enabled = false;
            }
        }

        // =====================================================
        // REPRODUZ O SOM NO INÍCIO DA ANIMAÇÃO
        // =====================================================

        if (animationAudioSource != null && animationSound != null)
        {
            animationAudioSource.PlayOneShot(animationSound);
        }

        // =====================================================
        // ANIMA AS TRÊS METRALHADORAS SIMULTANEAMENTE
        // =====================================================

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / animationDuration);

            // Suaviza o movimento
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < weaponMains.Length; i++)
            {
                if (weaponMains[i] == null) continue;

                Vector3 position = weaponMains[i].localPosition;

                position.y = Mathf.Lerp(
                    startY,
                    targetY,
                    smoothProgress
                );

                weaponMains[i].localPosition = position;
            }

            yield return null;
        }

        // =====================================================
        // GARANTE QUE TODAS TERMINEM EXATAMENTE NA POSIÇÃO FINAL
        // =====================================================

        for (int i = 0; i < weaponMains.Length; i++)
        {
            if (weaponMains[i] == null) continue;

            Vector3 position = weaponMains[i].localPosition;
            position.y = targetY;
            weaponMains[i].localPosition = position;
        }

        // =====================================================
        // AGUARDA ANTES DE CARREGAR A CENA FINAL
        // =====================================================

        yield return new WaitForSeconds(sceneChangeDelay);

        SceneManager.LoadScene(finalScene);
    }
}