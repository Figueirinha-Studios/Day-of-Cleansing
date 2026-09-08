using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class GameIntroManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;

    [SerializeField] private string videoName = "CutScene.mp4";

    [SerializeField] private bool PlayOrNot = true;

    private void Start()
    {
        if (PlayOrNot)
        {
            Play();
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    public void Play()
    {
        videoPlayer.source = VideoSource.Url;

        videoPlayer.url = VideoManager.Instance.GetVideoUrl(videoName);

        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene("SampleScene");
    }
}