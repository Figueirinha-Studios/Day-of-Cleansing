using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GeneratorUI : MonoBehaviour
{
    [Header("Vídeos dos Itens")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage;

    public VideoClip gasolineOneVideo;
    public VideoClip gasolineTwoVideo;
    public VideoClip fuseVideo;


    [Header("Vídeo - Gerador Ligado")]
    public VideoPlayer generatorOnVideoPlayer;
    public RawImage generatorOnVideoImage;

    public VideoClip generatorOnVideo;


    [Header("Vídeo - Objetivo")]
    public VideoPlayer objectiveVideoPlayer;
    public RawImage objectiveVideoImage;

    public VideoClip generatorTutorialVideo;


    [Header("Sons de Colocar")]
    public AudioClip gasolineInsertSound;
    public AudioClip fuseInsertSound;


    // =========================================================
    // SONS DO GERADOR
    // =========================================================

    [Header("Som do Gerador - Ligando")]
    public AudioClip generatorStartSound;


    [Header("Som do Gerador - Loop")]
    public AudioClip generatorLoopSound;


    [Header("Áudio")]
    public AudioSource audioSource;


    [Header("Áudio do Gerador")]
    public AudioSource generatorAudioSource;


    [Header("Configuração do Som do Gerador")]
    [Range(0f, 10f)]
    public float generatorSoundVolume = 1f;

    public float generatorMinDistance = 5f;

    public float generatorMaxDistance = 50f;


    [Header("Tempo entre Som e Vídeo")]
    public float delayBeforeVideo = 1f;


    [Header("Tempo depois do último vídeo")]
    public float delayAfterLastItemVideo = 1f;


    [Header("Sequência GeneratorON")]
    public float delayBeforeGeneratorSound = 2f;

    public float delayBeforeGeneratorOnVideo = 3f;


    private Coroutine currentSequence;

    private bool generatorIsPlaying = false;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        HideAllVideos();


        if (generatorAudioSource != null)
        {
            generatorAudioSource.Stop();

            generatorAudioSource.playOnAwake = false;

            generatorAudioSource.loop = false;


            // =================================================
            // ÁUDIO 3D
            // =================================================

            generatorAudioSource.spatialBlend = 1f;


            generatorAudioSource.minDistance =
                generatorMinDistance;


            generatorAudioSource.maxDistance =
                generatorMaxDistance;


            generatorAudioSource.rolloffMode =
                AudioRolloffMode.Linear;


            generatorAudioSource.volume =
                generatorSoundVolume;
        }
    }


    // =========================================================
    // OBJETIVO
    // =========================================================

    public void ShowGeneratorTutorial()
    {
        if (generatorTutorialVideo == null)
            return;

        if (objectiveVideoPlayer == null)
            return;

        if (objectiveVideoImage == null)
            return;


        StartCoroutine(
            GeneratorTutorialSequence()
        );
    }


    private IEnumerator GeneratorTutorialSequence()
    {
        HideObjectiveVideo();


        objectiveVideoPlayer.clip =
            generatorTutorialVideo;


        objectiveVideoPlayer.isLooping =
            false;


        objectiveVideoPlayer.Prepare();


        while (!objectiveVideoPlayer.isPrepared)
        {
            yield return null;
        }


        objectiveVideoImage.gameObject.SetActive(true);


        objectiveVideoPlayer.Play();


        while (objectiveVideoPlayer.isPlaying)
        {
            yield return null;
        }


        HideObjectiveVideo();
    }


    // =========================================================
    // GASOLINA
    // =========================================================

    public void ShowGasolineInserted(
        int gasolineCount,
        bool isLastItem
    )
    {
        VideoClip video = null;


        if (gasolineCount == 1)
        {
            video = gasolineOneVideo;
        }
        else if (gasolineCount == 2)
        {
            video = gasolineTwoVideo;
        }


        if (video == null)
            return;


        StartVideoSequence(
            video,
            gasolineInsertSound,
            isLastItem
        );
    }


    // =========================================================
    // FUSÍVEL
    // =========================================================

    public void ShowFuseInserted(
        bool isLastItem
    )
    {
        if (fuseVideo == null)
            return;


        StartVideoSequence(
            fuseVideo,
            fuseInsertSound,
            isLastItem
        );
    }


    // =========================================================
    // COMEÇAR VÍDEO
    // =========================================================

    private void StartVideoSequence(
        VideoClip video,
        AudioClip insertSound,
        bool isLastItem
    )
    {
        if (currentSequence != null)
        {
            StopCoroutine(currentSequence);
        }


        currentSequence =
            StartCoroutine(
                PlayInsertSequence(
                    video,
                    insertSound,
                    isLastItem
                )
            );
    }


    // =========================================================
    // VÍDEO DO ITEM
    // =========================================================

    private IEnumerator PlayInsertSequence(
        VideoClip video,
        AudioClip insertSound,
        bool isLastItem
    )
    {
        HideItemVideo();


        // -----------------------------------------------------
        // SOM
        // -----------------------------------------------------

        PlaySound(insertSound);


        if (insertSound != null)
        {
            yield return new WaitForSeconds(
                insertSound.length
            );
        }


        // -----------------------------------------------------
        // DELAY
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            delayBeforeVideo
        );


        if (videoPlayer == null ||
            videoImage == null)
        {
            yield break;
        }


        // -----------------------------------------------------
        // CONFIGURA
        // -----------------------------------------------------

        videoPlayer.Stop();

        videoPlayer.clip =
            video;

        videoPlayer.isLooping =
            false;


        videoPlayer.Prepare();


        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }


        // -----------------------------------------------------
        // MOSTRA
        // -----------------------------------------------------

        videoImage.gameObject.SetActive(true);


        // -----------------------------------------------------
        // TOCA
        // -----------------------------------------------------

        videoPlayer.Play();


        while (!videoPlayer.isPlaying)
        {
            yield return null;
        }


        // -----------------------------------------------------
        // ESPERA TERMINAR
        // -----------------------------------------------------

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }


        // -----------------------------------------------------
        // ESCONDE
        // -----------------------------------------------------

        HideItemVideo();


        currentSequence = null;


        // =====================================================
        // ÚLTIMO ITEM
        // =====================================================

        if (isLastItem)
        {
            Debug.Log(
                "Último vídeo terminou!"
            );


            yield return new WaitForSeconds(
                delayAfterLastItemVideo
            );


            Generator generator =
                FindFirstObjectByType<Generator>();


            if (generator != null)
            {
                generator.LastItemVideoFinished();
            }
        }
    }


    // =========================================================
    // GENERATOR ON
    // =========================================================

    public void StartGeneratorOnSequence()
    {
        if (generatorIsPlaying)
            return;


        if (currentSequence != null)
        {
            StopCoroutine(currentSequence);
        }


        currentSequence =
            StartCoroutine(
                GeneratorOnSequence()
            );
    }


    private IEnumerator GeneratorOnSequence()
    {
        generatorIsPlaying = true;


        HideItemVideo();

        HideGeneratorOnVideo();

        HideObjectiveVideo();


        // -----------------------------------------------------
        // ESPERA
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            delayBeforeGeneratorSound
        );


        // -----------------------------------------------------
        // PREPARA ÁUDIO
        // -----------------------------------------------------

        SetupGeneratorAudio();


        // -----------------------------------------------------
        // SOM DE LIGAÇÃO
        // -----------------------------------------------------

        if (generatorStartSound != null)
        {
            generatorAudioSource.loop = false;

            generatorAudioSource.clip =
                generatorStartSound;

            generatorAudioSource.Play();


            // Espera o som terminar.

            yield return new WaitForSeconds(
                generatorStartSound.length
            );
        }


        // =====================================================
        // SOM DE LOOP
        // =====================================================

        if (generatorLoopSound != null)
        {
            generatorAudioSource.clip =
                generatorLoopSound;

            generatorAudioSource.loop =
                true;

            generatorAudioSource.Play();
        }


        // -----------------------------------------------------
        // ESPERA PARA O VÍDEO
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            delayBeforeGeneratorOnVideo
        );


        // -----------------------------------------------------
        // VÍDEO
        // -----------------------------------------------------

        if (generatorOnVideo != null &&
            generatorOnVideoPlayer != null &&
            generatorOnVideoImage != null)
        {
            generatorOnVideoPlayer.Stop();


            generatorOnVideoPlayer.clip =
                generatorOnVideo;


            generatorOnVideoPlayer.isLooping =
                false;


            generatorOnVideoPlayer.Prepare();


            while (!generatorOnVideoPlayer.isPrepared)
            {
                yield return null;
            }


            generatorOnVideoImage.gameObject.SetActive(
                true
            );


            generatorOnVideoPlayer.Play();


            while (!generatorOnVideoPlayer.isPlaying)
            {
                yield return null;
            }


            // Espera o vídeo terminar.

            while (generatorOnVideoPlayer.isPlaying)
            {
                yield return null;
            }


            HideGeneratorOnVideo();
        }


        // =====================================================
        // AGORA SIM: GENERATOR ON
        // =====================================================

        Generator generator =
            FindFirstObjectByType<Generator>();


        if (generator != null)
        {
            generator.CompleteGeneratorOn();
        }


        currentSequence = null;

        generatorIsPlaying = false;
    }


    // =========================================================
    // CONFIGURAR ÁUDIO DO GERADOR
    // =========================================================

    private void SetupGeneratorAudio()
    {
        if (generatorAudioSource == null)
            return;


        generatorAudioSource.Stop();


        generatorAudioSource.playOnAwake =
            false;


        generatorAudioSource.volume =
            generatorSoundVolume;


        // =====================================================
        // 3D
        // =====================================================

        generatorAudioSource.spatialBlend =
            1f;


        generatorAudioSource.minDistance =
            generatorMinDistance;


        generatorAudioSource.maxDistance =
            generatorMaxDistance;


        generatorAudioSource.rolloffMode =
            AudioRolloffMode.Linear;
    }


    // =========================================================
    // SOM NORMAL
    // =========================================================

    private void PlaySound(
        AudioClip clip
    )
    {
        if (audioSource == null)
            return;


        if (clip == null)
            return;


        audioSource.PlayOneShot(
            clip
        );
    }


    // =========================================================
    // ESCONDER ITEM
    // =========================================================

    private void HideItemVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();

            videoPlayer.clip = null;
        }


        if (videoImage != null)
        {
            videoImage.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // ESCONDER GENERATOR ON
    // =========================================================

    private void HideGeneratorOnVideo()
    {
        if (generatorOnVideoPlayer != null)
        {
            generatorOnVideoPlayer.Stop();

            generatorOnVideoPlayer.clip = null;
        }


        if (generatorOnVideoImage != null)
        {
            generatorOnVideoImage.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // ESCONDER OBJETIVO
    // =========================================================

    private void HideObjectiveVideo()
    {
        if (objectiveVideoPlayer != null)
        {
            objectiveVideoPlayer.Stop();

            objectiveVideoPlayer.clip = null;
        }


        if (objectiveVideoImage != null)
        {
            objectiveVideoImage.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // ESCONDER TODOS
    // =========================================================

    private void HideAllVideos()
    {
        HideItemVideo();

        HideGeneratorOnVideo();

        HideObjectiveVideo();
    }


    // =========================================================
    // ESTADO
    // =========================================================

    public bool IsGeneratorPlaying()
    {
        return generatorIsPlaying;
    }
}
