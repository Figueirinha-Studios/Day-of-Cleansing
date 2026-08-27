using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GeneratorUI : MonoBehaviour
{
    [Header("========================================")]
    [Header("VÍDEOS DOS ITENS")]
    [Header("========================================")]

    public VideoPlayer videoPlayer;
    public RawImage videoImage;


    [Header("Vídeos")]
    public VideoClip gasolineOneVideo;
    public VideoClip gasolineTwoVideo;
    public VideoClip fuseVideo;


    [Header("========================================")]
    [Header("VÍDEO - GERADOR LIGADO")]
    [Header("========================================")]

    public VideoPlayer generatorOnVideoPlayer;
    public RawImage generatorOnVideoImage;

    public VideoClip generatorOnVideo;


    [Header("========================================")]
    [Header("SONS DE COLOCAR")]
    [Header("========================================")]

    public AudioSource audioSource;

    public AudioClip gasolineInsertSound;
    public AudioClip fuseInsertSound;


    [Header("========================================")]
    [Header("SOM DO GERADOR")]
    [Header("========================================")]

    public AudioSource generatorAudioSource;
    public AudioClip generatorOnSound;


    [Header("========================================")]
    [Header("TEMPO DOS VÍDEOS")]
    [Header("========================================")]

    [Tooltip("Tempo de espera depois que o som de inserção terminar.")]
    public float delayAfterInsertSound = 1f;

    [Tooltip("Tempo que o vídeo do item fica aparecendo.")]
    public float videoDisplayTime = 3f;


    [Header("========================================")]
    [Header("SEQUÊNCIA FINAL")]
    [Header("========================================")]

    [Tooltip("Depois do último vídeo, espera esse tempo antes de ligar o som do gerador.")]
    public float delayBeforeGeneratorSound = 2f;

    [Tooltip("Depois que o som do gerador começa, espera esse tempo antes do vídeo.")]
    public float delayBeforeGeneratorOnVideo = 3f;

    [Tooltip("Tempo que o vídeo 'Gerador Ligado' fica na tela.")]
    public float generatorOnVideoDisplayTime = 4f;


    [Header("========================================")]
    [Header("ESTADO")]
    [Header("========================================")]

    private Coroutine currentSequence;

    private bool generatorIsPlaying = false;

    private bool itemVideoFinished = true;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        HideAllVideos();


        if (generatorAudioSource != null)
        {
            generatorAudioSource.Stop();
            generatorAudioSource.loop = true;
        }
    }


    // =========================================================
    // GASOLINA
    // =========================================================

    public void ShowGasolineInserted(int gasolineCount)
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
        {
            Debug.LogWarning(
                "GeneratorUI: Vídeo da gasolina " +
                gasolineCount +
                "/2 não está configurado."
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
                "GeneratorUI: Vídeo do fusível não está configurado."
            );

            return;
        }


        StartVideoSequence(
            fuseVideo,
            fuseInsertSound
        );
    }


    // =========================================================
    // INICIAR SEQUÊNCIA DO ITEM
    // =========================================================

    private void StartVideoSequence(
        VideoClip video,
        AudioClip insertSound
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
                    insertSound
                )
            );
    }


    // =========================================================
    // SEQUÊNCIA DO ITEM
    // =========================================================

    private IEnumerator PlayInsertSequence(
        VideoClip video,
        AudioClip insertSound
    )
    {
        itemVideoFinished = false;


        HideItemVideo();


        // =====================================================
        // SOM DE INSERÇÃO
        // =====================================================

        if (insertSound != null &&
            audioSource != null)
        {
            audioSource.PlayOneShot(
                insertSound
            );
        }
        else
        {
            if (insertSound == null)
            {
                Debug.LogWarning(
                    "GeneratorUI: AudioClip de inserção não configurado."
                );
            }

            if (audioSource == null)
            {
                Debug.LogWarning(
                    "GeneratorUI: Audio Source não configurado."
                );
            }
        }


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
        // ESPERA 1 SEGUNDO APÓS O SOM
        // =====================================================

        yield return new WaitForSeconds(
            delayAfterInsertSound
        );


        // =====================================================
        // VERIFICA VIDEO PLAYER
        // =====================================================

        if (videoPlayer == null)
        {
            Debug.LogError(
                "GeneratorUI: Video Player dos itens não foi configurado!"
            );

            itemVideoFinished = true;
            currentSequence = null;

            yield break;
        }


        // =====================================================
        // VERIFICA RAW IMAGE
        // =====================================================

        if (videoImage == null)
        {
            Debug.LogError(
                "GeneratorUI: Raw Image dos itens não foi configurado!"
            );

            itemVideoFinished = true;
            currentSequence = null;

            yield break;
        }


        // =====================================================
        // VERIFICA VIDEO
        // =====================================================

        if (video == null)
        {
            Debug.LogError(
                "GeneratorUI: VideoClip não configurado!"
            );

            itemVideoFinished = true;
            currentSequence = null;

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
        // PREPARA VÍDEO
        // =====================================================

        videoPlayer.Prepare();


        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }


        // =====================================================
        // MOSTRA VÍDEO
        // =====================================================

        videoImage.gameObject.SetActive(
            true
        );


        // =====================================================
        // COMEÇA VÍDEO
        // =====================================================

        videoPlayer.Play();


        // =====================================================
        // OS 3 SEGUNDOS COMEÇAM JUNTO COM O VÍDEO
        // =====================================================

        yield return new WaitForSeconds(
            videoDisplayTime
        );


        // =====================================================
        // ESCONDE VÍDEO
        // =====================================================

        HideItemVideo();


        // =====================================================
        // AVISA QUE O VÍDEO TERMINOU
        // =====================================================

        itemVideoFinished = true;

        currentSequence = null;
    }


    // =========================================================
    // VERIFICAR SE VÍDEO DO ITEM TERMINOU
    // =========================================================

    public bool IsItemVideoFinished()
    {
        return itemVideoFinished;
    }


    // =========================================================
    // SEQUÊNCIA FINAL DO GERADOR
    // =========================================================

    public void StartGeneratorOnSequence()
    {
        if (generatorIsPlaying)
        {
            return;
        }


        if (currentSequence != null)
        {
            StopCoroutine(currentSequence);
        }


        currentSequence =
            StartCoroutine(
                GeneratorOnSequence()
            );
    }


    // =========================================================
    // GERADOR LIGADO
    // =========================================================

    private IEnumerator GeneratorOnSequence()
    {
        generatorIsPlaying = true;


        // =====================================================
        // ESCONDE VÍDEOS
        // =====================================================

        HideAllVideos();


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
        // VÍDEO "GERADOR LIGADO"
        // =====================================================

        yield return StartCoroutine(
            PlayGeneratorOnVideo()
        );


        // =====================================================
        // FINAL
        // =====================================================

        currentSequence = null;
    }


    // =========================================================
    // VÍDEO GERADOR LIGADO
    // =========================================================

    private IEnumerator PlayGeneratorOnVideo()
    {
        if (generatorOnVideo == null)
        {
            Debug.LogWarning(
                "GeneratorUI: Vídeo 'Gerador Ligado' não configurado."
            );

            yield break;
        }


        if (generatorOnVideoPlayer == null)
        {
            Debug.LogError(
                "GeneratorUI: Video Player do 'Gerador Ligado' não configurado!"
            );

            yield break;
        }


        if (generatorOnVideoImage == null)
        {
            Debug.LogError(
                "GeneratorUI: Raw Image do 'Gerador Ligado' não configurado!"
            );

            yield break;
        }


        // =====================================================
        // CONFIGURA VÍDEO
        // =====================================================

        generatorOnVideoPlayer.Stop();

        generatorOnVideoPlayer.clip =
            generatorOnVideo;

        generatorOnVideoPlayer.isLooping =
            false;


        // =====================================================
        // PREPARA
        // =====================================================

        generatorOnVideoPlayer.Prepare();


        while (!generatorOnVideoPlayer.isPrepared)
        {
            yield return null;
        }


        // =====================================================
        // MOSTRA
        // =====================================================

        generatorOnVideoImage.gameObject.SetActive(
            true
        );


        // =====================================================
        // TOCA
        // =====================================================

        generatorOnVideoPlayer.Play();


        // =====================================================
        // FICA 4 SEGUNDOS
        // =====================================================

        yield return new WaitForSeconds(
            generatorOnVideoDisplayTime
        );


        // =====================================================
        // ESCONDE
        // =====================================================

        generatorOnVideoPlayer.Stop();

        generatorOnVideoImage.gameObject.SetActive(
            false
        );
    }


    // =========================================================
    // SOM DO GERADOR
    // =========================================================

    private void PlayGeneratorSound()
    {
        if (generatorAudioSource == null)
        {
            Debug.LogError(
                "GeneratorUI: Generator Audio Source não configurado!"
            );

            return;
        }


        if (generatorOnSound == null)
        {
            Debug.LogError(
                "GeneratorUI: Generator On Sound não configurado!"
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
    // ESCONDER TODOS OS VÍDEOS
    // =========================================================

    private void HideAllVideos()
    {
        HideItemVideo();


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
    // ESTADO
    // =========================================================

    public bool IsGeneratorPlaying()
    {
        return generatorIsPlaying;
    }
}