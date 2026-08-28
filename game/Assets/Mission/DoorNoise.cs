using System.Collections;
using UnityEngine;

public class DoorNoise : MonoBehaviour
{
    [Header("Gerador")]
    public Generator generator;


    [Header("Sirene")]
    public AudioSource audioSource;

    public AudioClip sirenSound;

    [Range(0f, 10f)]
    public float sirenVolume = 5f;


    [Header("Alcance da Sirene")]
    public float sirenMinDistance = 5f;

    public float sirenMaxDistance = 200f;


    [Header("Ruído para os Inimigos")]
    public float noiseRadius = 200f;


    [Header("Tempo")]
    public float delayBeforeSiren = 1f;

    public float delayAfterSiren = 1f;


    private bool sequenceStarted = false;

    private bool doorCanOpen = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (audioSource != null)
        {
            audioSource.Stop();

            audioSource.playOnAwake = false;

            audioSource.loop = false;


            // =================================================
            // VOLUME
            // =================================================

            audioSource.volume =
                sirenVolume;


            // =================================================
            // ÁUDIO 3D
            // =================================================

            audioSource.spatialBlend =
                1f;


            // =================================================
            // DISTÂNCIA
            // =================================================

            audioSource.minDistance =
                sirenMinDistance;

            audioSource.maxDistance =
                sirenMaxDistance;


            // =================================================
            // ROLLOFF
            // =================================================

            audioSource.rolloffMode =
                AudioRolloffMode.Linear;


            // =================================================
            // PRIORIDADE
            // =================================================

            audioSource.priority =
                0;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (sequenceStarted)
            return;


        if (generator == null)
            return;


        if (!generator.IsGeneratorOn())
            return;


        sequenceStarted = true;


        Debug.Log(
            "DOORNOISE: Gerador detectado como ON!"
        );


        StartCoroutine(
            DoorSequence()
        );
    }


    // =========================================================
    // SEQUÊNCIA
    // =========================================================

    private IEnumerator DoorSequence()
    {
        Debug.Log(
            "DOORNOISE: Esperando " +
            delayBeforeSiren +
            " segundos para a sirene."
        );


        yield return new WaitForSeconds(
            delayBeforeSiren
        );


        // =====================================================
        // SIRENE
        // =====================================================

        PlaySiren();


        // =====================================================
        // RUÍDO PARA O INIMIGO
        // =====================================================

        NoiseSystem.EmitNoise(
            transform.position,
            noiseRadius
        );


        Debug.Log(
            "DOORNOISE: Ruído enviado ao NoiseSystem."
        );


        // =====================================================
        // ESPERA A SIRENE
        // =====================================================

        if (sirenSound != null)
        {
            yield return new WaitForSeconds(
                sirenSound.length
            );
        }


        // =====================================================
        // ESPERA DEPOIS DA SIRENE
        // =====================================================

        Debug.Log(
            "DOORNOISE: Sirene terminou. " +
            "Esperando " +
            delayAfterSiren +
            " segundos."
        );


        yield return new WaitForSeconds(
            delayAfterSiren
        );


        // =====================================================
        // LIBERA A PORTA
        // =====================================================

        doorCanOpen = true;


        Debug.Log(
            "DOORNOISE: PORTA LIBERADA PARA ABRIR!"
        );
    }


    // =========================================================
    // TOCAR SIRENE
    // =========================================================

    private void PlaySiren()
    {
        if (audioSource == null)
        {
            Debug.LogError(
                "DOORNOISE ERRO: Audio Source não configurado!"
            );

            return;
        }


        if (sirenSound == null)
        {
            Debug.LogError(
                "DOORNOISE ERRO: Siren Sound não configurado!"
            );

            return;
        }


        audioSource.Stop();


        audioSource.clip =
            sirenSound;


        audioSource.volume =
            sirenVolume;


        audioSource.loop =
            false;


        // =====================================================
        // ÁUDIO 3D
        // =====================================================

        audioSource.spatialBlend =
            1f;


        audioSource.minDistance =
            sirenMinDistance;


        audioSource.maxDistance =
            sirenMaxDistance;


        audioSource.rolloffMode =
            AudioRolloffMode.Linear;


        audioSource.priority =
            0;


        audioSource.Play();


        Debug.Log(
            "DOORNOISE: SIRENE TOCANDO! " +
            "Volume: " +
            audioSource.volume
        );
    }


    // =========================================================
    // PODE ABRIR
    // =========================================================

    public bool CanDoorOpen()
    {
        return doorCanOpen;
    }
}