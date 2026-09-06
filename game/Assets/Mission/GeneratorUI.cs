using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GeneratorUI : MonoBehaviour
{
    [Header("Nomes dos vídeos")]
    public string gasolineOneVideo = "1-1.mp4";
    public string gasolineTwoVideo = "1-2.mp4";
    public string fuseVideo = "2-2.mp4";
    public string generatorOnVideo = "GeneratorON.mp4";
    public string generatorTutorialVideo = "Objective.mp4";

    [Header("Vídeos dos Itens")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage;

    [Header("Vídeo - Gerador Ligado")]
    public VideoPlayer generatorOnVideoPlayer;
    public RawImage generatorOnVideoImage;

    [Header("Vídeo - Objetivo")]
    public VideoPlayer objectiveVideoPlayer;
    public RawImage objectiveVideoImage;

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
        if (string.IsNullOrEmpty(generatorTutorialVideo))
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


        objectiveVideoPlayer.source = VideoSource.Url;
        objectiveVideoPlayer.url = VideoManager.Instance.GetVideoUrl(generatorTutorialVideo);

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
        string video = null;

        if (gasolineCount == 1)
        {
            video = gasolineOneVideo;
        }
        else if (gasolineCount == 2)
        {
            video = gasolineTwoVideo;
        }

        if (string.IsNullOrEmpty(video))
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
        if (string.IsNullOrEmpty(fuseVideo))
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
        string video,
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
        string video,
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
            /*
             * Se este era o último item,
             * ainda tenta iniciar o Generator ON.
             */
            currentSequence = null;

            if (isLastItem)
            {
                Generator generator =
                    FindFirstObjectByType<Generator>();

                if (generator != null)
                {
                    generator.LastItemVideoFinished();
                }
            }

            yield break;
        }


        // -----------------------------------------------------
        // CONFIGURA
        // -----------------------------------------------------

        videoPlayer.Stop();

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = VideoManager.Instance.GetVideoUrl(video);

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

            currentSequence = null;
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
            generatorAudioSource.loop =
                false;

            generatorAudioSource.clip =
                generatorStartSound;

            generatorAudioSource.Play();


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

        if (!string.IsNullOrEmpty(generatorOnVideo) &&
            generatorOnVideoPlayer != null &&
            generatorOnVideoImage != null)
        {
            generatorOnVideoPlayer.Stop();


            generatorOnVideoPlayer.source = VideoSource.Url;
            generatorOnVideoPlayer.url = VideoManager.Instance.GetVideoUrl(generatorOnVideo);

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