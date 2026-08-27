using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GeneratorUI : MonoBehaviour
{
    // =========================================================
    // VÍDEOS DOS ITENS
    // =========================================================

    [Header("Vídeos dos Itens")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage;

    public VideoClip gasolineOneVideo;
    public VideoClip gasolineTwoVideo;
    public VideoClip fuseVideo;


    // =========================================================
    // GERADOR LIGADO
    // =========================================================

    [Header("Vídeo - Gerador Ligado")]
    public VideoPlayer generatorOnVideoPlayer;
    public RawImage generatorOnVideoImage;

    public VideoClip generatorOnVideo;


    // =========================================================
    // OBJETIVO DO GERADOR
    // =========================================================

    [Header("Vídeo - Objetivo do Gerador")]
    public VideoPlayer objectiveVideoPlayer;
    public RawImage objectiveVideoImage;

    public VideoClip generatorTutorialVideo;


    // =========================================================
    // SONS DE COLOCAR
    // =========================================================

    [Header("Sons de Colocar")]
    public AudioClip gasolineInsertSound;
    public AudioClip fuseInsertSound;


    // =========================================================
    // SOM DO GERADOR
    // =========================================================

    [Header("Som do Gerador Ligado")]
    public AudioClip generatorOnSound;


    // =========================================================
    // ÁUDIO NORMAL
    // =========================================================

    [Header("Áudio")]
    public AudioSource audioSource;


    // =========================================================
    // ÁUDIO DO GERADOR
    // =========================================================

    [Header("Áudio do Gerador")]
    public AudioSource generatorAudioSource;


    // =========================================================
    // TEMPO DOS VÍDEOS DOS ITENS
    // =========================================================

    [Header("Tempo entre Som e Vídeo")]
    public float delayBeforeVideo = 1f;

    public float videoDisplayTime = 3f;


    // =========================================================
    // SEQUÊNCIA FINAL
    // =========================================================

    [Header("Sequência Final")]
    public float delayBeforeGeneratorSound = 2f;

    public float delayBeforeGeneratorOnVideo = 3f;

    public float generatorOnVideoDisplayTime = 4f;


    // =========================================================
    // CONTROLE
    // =========================================================

    private Coroutine currentSequence;

    private bool generatorIsPlaying = false;

    private bool itemVideoPlaying = false;


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
    // OBJETIVO DO GERADOR
    // =========================================================

    public void ShowGeneratorTutorial()
    {
        if (generatorTutorialVideo == null)
        {
            Debug.LogWarning(
                "Vídeo do objetivo do gerador não configurado."
            );

            return;
        }

        if (objectiveVideoPlayer == null)
        {
            Debug.LogWarning(
                "Objective Video Player não configurado."
            );

            return;
        }

        if (objectiveVideoImage == null)
        {
            Debug.LogWarning(
                "Objective Video Image não configurado."
            );

            return;
        }

        StartCoroutine(
            GeneratorTutorialSequence()
        );
    }


    // =========================================================
    // SEQUÊNCIA DO OBJETIVO
    // =========================================================

    private IEnumerator GeneratorTutorialSequence()
    {
        HideObjectiveVideo();

        objectiveVideoPlayer.Stop();

        objectiveVideoPlayer.clip =
            generatorTutorialVideo;

        objectiveVideoPlayer.isLooping =
            false;

        objectiveVideoPlayer.Prepare();

        while (!objectiveVideoPlayer.isPrepared)
        {
            yield return null;
        }

        objectiveVideoImage.gameObject.SetActive(
            true
        );

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
        int gasolineCount
    )
    {
        VideoClip video = null;

        if (gasolineCount == 1)
        {
            video =
                gasolineOneVideo;
        }
        else if (gasolineCount == 2)
        {
            video =
                gasolineTwoVideo;
        }

        if (video == null)
        {
            Debug.LogWarning(
                "Vídeo da gasolina " +
                gasolineCount +
                "/2 não configurado."
            );

            return;
        }

        StartVideoSequence(
            video,
            gasolineInsertSound
        );
    }


    // =========================================================
    // FUSÍVEL
    // =========================================================

    public void ShowFuseInserted()
    {
        if (fuseVideo == null)
        {
            Debug.LogWarning(
                "Vídeo do fusível não configurado."
            );

            return;
        }

        StartVideoSequence(
            fuseVideo,
            fuseInsertSound
        );
    }


    // =========================================================
    // COMEÇAR VÍDEO DOS ITENS
    // =========================================================

    private void StartVideoSequence(
        VideoClip video,
        AudioClip insertSound
    )
    {
        if (currentSequence != null)
        {
            StopCoroutine(
                currentSequence
            );
        }

        currentSequence =
            StartCoroutine(
                PlayInsertSequence(
                    video,
                    insertSound
                )
            );
    }


    // =========================================================
    // SEQUÊNCIA DOS ITENS
    // =========================================================

    private IEnumerator PlayInsertSequence(
        VideoClip video,
        AudioClip insertSound
    )
    {
        itemVideoPlaying = true;

        HideItemVideo();


        // =====================================================
        // SOM DE COLOCAR
        // =====================================================

        PlaySound(
            insertSound
        );


        // =====================================================
        // ESPERA O SOM TERMINAR
        // =====================================================

        if (insertSound != null)
        {
            yield return new WaitForSeconds(
                insertSound.length
            );
        }


        // =====================================================
        // ESPERA MAIS 1 SEGUNDO
        // =====================================================

        yield return new WaitForSeconds(
            delayBeforeVideo
        );


        if (videoPlayer == null)
        {
            Debug.LogWarning(
                "Video Player não configurado."
            );

            itemVideoPlaying = false;

            yield break;
        }


        if (videoImage == null)
        {
            Debug.LogWarning(
                "Video Image não configurado."
            );

            itemVideoPlaying = false;

            yield break;
        }


        // =====================================================
        // CONFIGURA VÍDEO
        // =====================================================

        videoPlayer.Stop();

        videoPlayer.clip =
            video;

        videoPlayer.isLooping =
            false;


        // =====================================================
        // PREPARA
        // =====================================================

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }


        // =====================================================
        // MOSTRA
        // =====================================================

        videoImage.gameObject.SetActive(
            true
        );


        // =====================================================
        // TOCA
        // =====================================================

        videoPlayer.Play();


        // =====================================================
        // FICA 3 SEGUNDOS
        // =====================================================

        yield return new WaitForSeconds(
            videoDisplayTime
        );


        // =====================================================
        // ESCONDE
        // =====================================================

        HideItemVideo();

        itemVideoPlaying = false;

        currentSequence = null;
    }


    // =========================================================
    // GERADOR LIGADO
    // =========================================================

    public void StartGeneratorOnSequence()
    {
        if (generatorIsPlaying)
            return;


        StartCoroutine(
            WaitForItemVideoThenStartGenerator()
        );
    }


    // =========================================================
    // ESPERA VÍDEO DO ÚLTIMO ITEM
    // =========================================================

    private IEnumerator WaitForItemVideoThenStartGenerator()
    {
        // Espera o vídeo do último item terminar.

        while (itemVideoPlaying)
        {
            yield return null;
        }


        // Agora sim começa a sequência do gerador.

        if (generatorIsPlaying)
            yield break;


        currentSequence =
            StartCoroutine(
                GeneratorOnSequence()
            );
    }


    // =========================================================
    // SEQUÊNCIA DO GERADOR LIGADO
    // =========================================================

    private IEnumerator GeneratorOnSequence()
    {
        generatorIsPlaying = true;


        // =====================================================
        // ESCONDE OS VÍDEOS
        // =====================================================

        HideItemVideo();
        HideGeneratorOnVideo();
        HideObjectiveVideo();


        // =====================================================
        // ESPERA 2 SEGUNDOS
        // =====================================================

        yield return new WaitForSeconds(
            delayBeforeGeneratorSound
        );


        // =====================================================
        // COMEÇA SOM DO GERADOR
        // =====================================================

        PlayGeneratorSound();


        // =====================================================
        // ESPERA 3 SEGUNDOS
        // =====================================================

        yield return new WaitForSeconds(
            delayBeforeGeneratorOnVideo
        );


        // =====================================================
        // VÍDEO GERADOR LIGADO
        // =====================================================

        if (generatorOnVideo != null &&
            generatorOnVideoPlayer != null &&
            generatorOnVideoImage != null)
        {
            generatorOnVideoPlayer.Stop();

            generatorOnVideoPlayer.clip =
                generatorOnVideo;

            generatorOnVideoPlayer.isLooping =
                false;


            // =================================================
            // PREPARA
            // =================================================

            generatorOnVideoPlayer.Prepare();

            while (!generatorOnVideoPlayer.isPrepared)
            {
                yield return null;
            }


            // =================================================
            // MOSTRA
            // =================================================

            generatorOnVideoImage.gameObject.SetActive(
                true
            );


            // =================================================
            // TOCA
            // =================================================

            generatorOnVideoPlayer.Play();


            // =================================================
            // FICA 4 SEGUNDOS
            // =================================================

            yield return new WaitForSeconds(
                generatorOnVideoDisplayTime
            );


            // =================================================
            // ESCONDE
            // =================================================

            HideGeneratorOnVideo();
        }


        currentSequence = null;
    }


    // =========================================================
    // SOM DO GERADOR
    // =========================================================

    private void PlayGeneratorSound()
    {
        if (generatorAudioSource == null)
        {
            Debug.LogWarning(
                "Generator Audio Source não configurado."
            );

            return;
        }


        if (generatorOnSound == null)
        {
            Debug.LogWarning(
                "Generator On Sound não configurado."
            );

            return;
        }


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
    // ESCONDER VÍDEO DOS ITENS
    // =========================================================

    private void HideItemVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (videoImage != null)
        {
            videoImage.gameObject.SetActive(
                false
            );
        }
    }


    // =========================================================
    // ESCONDER VÍDEO DO GERADOR ON
    // =========================================================

    private void HideGeneratorOnVideo()
    {
        if (generatorOnVideoPlayer != null)
        {
            generatorOnVideoPlayer.Stop();
        }

        if (generatorOnVideoImage != null)
        {
            generatorOnVideoImage.gameObject.SetActive(
                false
            );
        }
    }


    // =========================================================
    // ESCONDER VÍDEO DO OBJETIVO
    // =========================================================

    private void HideObjectiveVideo()
    {
        if (objectiveVideoPlayer != null)
        {
            objectiveVideoPlayer.Stop();
        }

        if (objectiveVideoImage != null)
        {
            objectiveVideoImage.gameObject.SetActive(
                false
            );
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