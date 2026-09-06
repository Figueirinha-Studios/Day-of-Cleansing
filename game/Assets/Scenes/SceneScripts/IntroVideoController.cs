using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [SerializeField] private RawImage videoImage;

    [Header("Nome do vídeo")]
    [SerializeField] private string videoName = "intro.mp4";

    [Header("Cena do Menu")]
    [SerializeField] private string menuSceneName = "Menu";


    private void Start()
    {
        if (VideoManager.Instance == null)
        {
            Debug.LogError(
                "IntroVideoController: VideoManager não existe."
            );

            return;
        }

        VideoManager.Instance.Play(
            videoName,
            videoPlayer,
            videoImage,
            OnVideoFinished
        );
    }


    private void OnVideoFinished()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}