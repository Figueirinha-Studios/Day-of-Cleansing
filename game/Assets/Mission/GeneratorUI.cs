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


    [Header("Som do Gerador")]
    public AudioClip generatorOnSound;


    [Header("Áudio")]
    public AudioSource audioSource;


    [Header("Áudio do Gerador")]
    public AudioSource generatorAudioSource;


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


        // Espera realmente começar.

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


            // Espera 1 segundo.

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
        // SOM
        // -----------------------------------------------------

        PlayGeneratorSound();


        // -----------------------------------------------------
        // ESPERA
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
    // SOM DO GERADOR
    // =========================================================

    private void PlayGeneratorSound()
    {
        if (generatorAudioSource == null)
            return;


        if (generatorOnSound == null)
            return;


        generatorAudioSource.clip =
            generatorOnSound;


        generatorAudioSource.loop =
            true;


        generatorAudioSource.Play();
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