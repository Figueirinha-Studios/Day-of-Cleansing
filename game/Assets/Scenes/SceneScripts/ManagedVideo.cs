using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ManagedVideo : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;

    [SerializeField] private string videoName;

    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;


    private void Start()
    {
        if (!playOnStart)
            return;

        Play();
    }


    public void Play()
    {
        videoPlayer.source = VideoSource.Url;

        videoPlayer.url =
            VideoManager.Instance.GetVideoUrl(
                videoName
            );

        videoPlayer.isLooping = loop;

        videoPlayer.Play();
    }
}