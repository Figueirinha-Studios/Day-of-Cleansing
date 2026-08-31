using UnityEngine;

public class Door : MonoBehaviour
{
    // =========================================================
    // SISTEMA
    // =========================================================

    [Header("Sistema")]
    public DoorNoise doorNoise;


    // =========================================================
    // PORTAS
    // =========================================================

    [Header("Folhas da Porta")]
    public Transform leftDoor;

    public Transform rightDoor;


    // =========================================================
    // CONFIGURAÇÃO
    // =========================================================

    [Header("Abertura")]
    public float openDistance = 2f;

    public float openSpeed = 0.5f;


    // =========================================================
    // SOM DA PORTA
    // =========================================================

    [Header("Som - Porta Abrindo")]
    public AudioSource doorAudioSource;

    public AudioClip doorOpeningSound;

    [Range(0f, 10f)]
    public float doorOpeningVolume = 1f;


    // =========================================================
    // POSIÇÕES
    // =========================================================

    private Vector3 leftOpenPosition;

    private Vector3 rightOpenPosition;


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
        if (leftDoor != null)
        {
            Vector3 startPosition =
                leftDoor.localPosition;


            // Porta esquerda vai para frente.

            leftOpenPosition =
                startPosition +
                Vector3.forward *
                openDistance;
        }


        if (rightDoor != null)
        {
            Vector3 startPosition =
                rightDoor.localPosition;


            // Porta direita vai para trás.

            rightOpenPosition =
                startPosition +
                Vector3.back *
                openDistance;
        }


        // =====================================================
        // CONFIGURA AUDIO SOURCE
        // =====================================================

        if (doorAudioSource != null)
        {
            doorAudioSource.Stop();

            doorAudioSource.playOnAwake = false;

            doorAudioSource.loop = true;

            doorAudioSource.volume =
                doorOpeningVolume;


            // Áudio 3D.

            doorAudioSource.spatialBlend = 1f;


            // Alcance do som.

            doorAudioSource.minDistance = 5f;

            doorAudioSource.maxDistance = 50f;


            // Rolloff.

            doorAudioSource.rolloffMode =
                AudioRolloffMode.Linear;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (opened)
            return;


        // =====================================================
        // ESPERA A SIRENE TERMINAR
        // =====================================================

        if (!opening)
        {
            if (doorNoise != null &&
                doorNoise.CanDoorOpen())
            {
                opening = true;


                // Começa o som da porta.

                PlayDoorOpeningSound();
            }
        }


        // =====================================================
        // ABRE
        // =====================================================

        if (opening)
        {
            OpenDoor();
        }
    }


    // =========================================================
    // SOM DA PORTA
    // =========================================================

    private void PlayDoorOpeningSound()
    {
        if (doorAudioSource == null)
        {
            Debug.LogWarning(
                "DOOR: Audio Source da porta não configurado!"
            );

            return;
        }


        if (doorOpeningSound == null)
        {
            Debug.LogWarning(
                "DOOR: Som de abertura não configurado!"
            );

            return;
        }


        doorAudioSource.Stop();


        doorAudioSource.clip =
            doorOpeningSound;


        doorAudioSource.volume =
            doorOpeningVolume;


        doorAudioSource.loop =
            true;


        // =====================================================
        // ÁUDIO 3D
        // =====================================================

        doorAudioSource.spatialBlend =
            1f;


        doorAudioSource.Play();


        Debug.Log(
            "DOOR: Som de porta abrindo começou."
        );
    }


    // =========================================================
    // PARAR SOM DA PORTA
    // =========================================================

    private void StopDoorOpeningSound()
    {
        if (doorAudioSource == null)
            return;


        doorAudioSource.Stop();


        Debug.Log(
            "DOOR: Som de porta abrindo terminou."
        );
    }


    // =========================================================
    // ABRIR PORTA
    // =========================================================

    private void OpenDoor()
    {
        bool leftFinished = true;

        bool rightFinished = true;


        // =====================================================
        // PORTA ESQUERDA
        // =====================================================

        if (leftDoor != null)
        {
            leftDoor.localPosition =
                Vector3.MoveTowards(
                    leftDoor.localPosition,
                    leftOpenPosition,
                    openSpeed *
                    Time.deltaTime
                );


            leftFinished =
                Vector3.Distance(
                    leftDoor.localPosition,
                    leftOpenPosition
                ) < 0.01f;
        }


        // =====================================================
        // PORTA DIREITA
        // =====================================================

        if (rightDoor != null)
        {
            rightDoor.localPosition =
                Vector3.MoveTowards(
                    rightDoor.localPosition,
                    rightOpenPosition,
                    openSpeed *
                    Time.deltaTime
                );


            rightFinished =
                Vector3.Distance(
                    rightDoor.localPosition,
                    rightOpenPosition
                ) < 0.01f;
        }


        // =====================================================
        // TERMINOU
        // =====================================================

        if (leftFinished &&
            rightFinished)
        {
            opened = true;


            // Para o som.

            StopDoorOpeningSound();


            Debug.Log(
                "PORTA ABERTA!"
            );
        }
    }
}