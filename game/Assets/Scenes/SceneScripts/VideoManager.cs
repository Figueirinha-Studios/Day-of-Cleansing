using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance { get; private set; }

    [Header("GitHub")]
    [SerializeField]
    private string githubBaseUrl =
        "https://github.com/viniciusam11/doc-videos/raw/refs/heads/main/videos/";

    [Header("Vídeos locais")]
    [SerializeField]
    private string localFolder = "Videos";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // =========================================================
    // URL DO VÍDEO
    // =========================================================

    public string GetVideoUrl(string videoName)
    {
        if (string.IsNullOrWhiteSpace(videoName))
        {
            Debug.LogError("VideoManager: nome do vídeo vazio.");
            return "";
        }

        // -----------------------------------------------------
        // WEBGL
        // -----------------------------------------------------

#if UNITY_WEBGL && !UNITY_EDITOR

        return githubBaseUrl + Uri.EscapeDataString(videoName);

#else

        // -----------------------------------------------------
        // WINDOWS / LINUX / OUTRAS BUILDS DESKTOP
        // -----------------------------------------------------

        string path = Path.Combine(
            Application.streamingAssetsPath,
            localFolder,
            videoName
        );

        path = Path.GetFullPath(path);

        return new Uri(path).AbsoluteUri;

#endif
    }


    // =========================================================
    // TOCAR VÍDEO
    // =========================================================

    public void Play(
        string videoName,
        VideoPlayer videoPlayer,
        RawImage videoImage = null,
        Action onFinished = null)
    {
        if (videoPlayer == null)
        {
            Debug.LogError(
                "VideoManager: VideoPlayer não configurado."
            );

            return;
        }

        StartCoroutine(
            PlayCoroutine(
                videoName,
                videoPlayer,
                videoImage,
                onFinished
            )
        );
    }


    // =========================================================
    // TOCAR E ESPERAR TERMINAR
    // =========================================================

    public IEnumerator PlayCoroutine(
        string videoName,
        VideoPlayer videoPlayer,
        RawImage videoImage = null,
        Action onFinished = null)
    {
        if (videoPlayer == null)
        {
            Debug.LogError(
                "VideoManager: VideoPlayer não configurado."
            );

            yield break;
        }

        if (string.IsNullOrWhiteSpace(videoName))
        {
            Debug.LogError(
                "VideoManager: nome do vídeo vazio."
            );

            yield break;
        }


        // -----------------------------------------------------
        // PREPARAÇÃO
        // -----------------------------------------------------

        videoPlayer.Stop();

        videoPlayer.source = VideoSource.Url;

        videoPlayer.url = GetVideoUrl(videoName);

        videoPlayer.isLooping = false;


        Debug.Log(
            "VideoManager: carregando vídeo: " +
            videoPlayer.url
        );


        // -----------------------------------------------------
        // PREPARE
        // -----------------------------------------------------

        bool prepareFinished = false;
        bool prepareFailed = false;

        VideoPlayer.ErrorEventHandler errorHandler =
            (vp, message) =>
            {
                prepareFailed = true;

                Debug.LogError(
                    "VideoManager: erro ao carregar '" +
                    videoName +
                    "': " +
                    message
                );
            };

        VideoPlayer.EventHandler prepareHandler =
            vp =>
            {
                prepareFinished = true;
            };


        videoPlayer.errorReceived += errorHandler;
        videoPlayer.prepareCompleted += prepareHandler;

        videoPlayer.Prepare();


        while (!prepareFinished && !prepareFailed)
        {
            yield return null;
        }


        videoPlayer.errorReceived -= errorHandler;
        videoPlayer.prepareCompleted -= prepareHandler;


        if (prepareFailed)
        {
            yield break;
        }


        // -----------------------------------------------------
        // MOSTRAR IMAGEM
        // -----------------------------------------------------

        if (videoImage != null)
        {
            videoImage.gameObject.SetActive(true);
        }


        // -----------------------------------------------------
        // TOCAR
        // -----------------------------------------------------

        videoPlayer.Play();


        while (!videoPlayer.isPlaying)
        {
            if (prepareFailed)
                yield break;

            yield return null;
        }


        // -----------------------------------------------------
        // ESPERAR TERMINAR
        // -----------------------------------------------------

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }


        // -----------------------------------------------------
        // ESCONDER
        // -----------------------------------------------------

        if (videoImage != null)
        {
            videoImage.gameObject.SetActive(false);
        }


        // -----------------------------------------------------
        // CALLBACK
        // -----------------------------------------------------

        onFinished?.Invoke();
    }


    // =========================================================
    // PARAR
    // =========================================================

    public void Stop(
        VideoPlayer videoPlayer,
        RawImage videoImage = null)
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.url = "";
        }

        if (videoImage != null)
        {
            videoImage.gameObject.SetActive(false);
        }
    }
}