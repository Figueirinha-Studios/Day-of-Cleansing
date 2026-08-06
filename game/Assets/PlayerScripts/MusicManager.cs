using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource ambientSource;
    public AudioSource enemySourceA;
    public AudioSource enemySourceB;

    [Header("Ambient Music")]
    public AudioClip[] ambientTracks;

    private List<AudioClip> remainingTracks = new();

    [Header("Enemy Music")]
    public AudioClip chaseMusic;
    public AudioClip searchMusic;
    private AudioSource currentEnemySource;
    private AudioSource nextEnemySource;

    private Coroutine musicCoroutine;

    private bool ambientPaused = false;

    [Header("Fade")]
    public float fadeSpeed = 2f;

    public float maxChaseVolume = 1.5f;
    public float farDistance = 20f;

    [Header("Distance Volume")]
    public Transform player;
    public Transform enemy;

    void Start()
    {
        PreparePlaylist();
        PlayNextAmbient();
        currentEnemySource = enemySourceA;
        nextEnemySource = enemySourceB;
    }

    void Update()
    {
        if (!ambientSource.isPlaying && remainingTracks.Count > 0)
        {
            PlayNextAmbient();
        }
        UpdateChaseVolume();
    }

    void PreparePlaylist()
    {
        remainingTracks.Clear();

        foreach (AudioClip clip in ambientTracks)
        {
            remainingTracks.Add(clip);
        }

        Shuffle();
    }

    void Shuffle()
    {
        for (int i = 0; i < remainingTracks.Count; i++)
        {
            int random = Random.Range(i, remainingTracks.Count);

            AudioClip temp = remainingTracks[i];
            remainingTracks[i] = remainingTracks[random];
            remainingTracks[random] = temp;
        }
    }
    void PlayNextAmbient()
    {
        if (remainingTracks.Count == 0)
            return;

        ambientSource.clip = remainingTracks[0];
        remainingTracks.RemoveAt(0);

        ambientSource.Play();
    }
    void StartEnemyMusic(AudioClip clip)
    {
        if (currentEnemySource.clip == clip &&
            currentEnemySource.isPlaying)
            return;

        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        musicCoroutine = StartCoroutine(CrossFadeEnemy(clip));
    }
    public void StartChaseMusic()
    {
        StartEnemyMusic(chaseMusic);
    }
    public void StartSearchMusic()
    {
        StartEnemyMusic(searchMusic);
    }
    IEnumerator CrossFadeEnemy(AudioClip clip)
    {
        yield return StartCoroutine(FadeOutAmbient());

        nextEnemySource.clip = clip;
        nextEnemySource.loop = true;
        nextEnemySource.volume = 0;

        nextEnemySource.Play();

        while (nextEnemySource.volume < 1)
        {
            nextEnemySource.volume += Time.deltaTime * fadeSpeed;

            currentEnemySource.volume -= Time.deltaTime * fadeSpeed;

            yield return null;
        }

        currentEnemySource.Stop();
        currentEnemySource.volume = 0;

        AudioSource temp = currentEnemySource;
        currentEnemySource = nextEnemySource;
        nextEnemySource = temp;
    }
    IEnumerator FadeOutAmbient()
    {
        if (ambientPaused)
            yield break;
        while (ambientSource.volume > 0)
        {
            ambientSource.volume -= Time.deltaTime * fadeSpeed;

            yield return null;
        }

        ambientSource.Pause();
        ambientPaused = true;
    }
    public void StopEnemyMusic()
    {
        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        musicCoroutine = StartCoroutine(FadeOutEnemy());
    }
    IEnumerator FadeOutEnemy()
    {
        while (currentEnemySource.volume > 0)
        {
            currentEnemySource.volume -= Time.deltaTime * fadeSpeed;

            yield return null;
        }

        currentEnemySource.Stop();

        yield return StartCoroutine(FadeInAmbient());
    }
    IEnumerator FadeInAmbient()
    {
        ambientSource.UnPause();

        while (ambientSource.volume < 1)
        {
            ambientSource.volume += Time.deltaTime * fadeSpeed;

            yield return null;
        }

        ambientPaused = false;
    }
    void UpdateChaseVolume()
    {
        if (!currentEnemySource.isPlaying)
            return;

        if (currentEnemySource.clip != chaseMusic)
            return;

        float distance = Vector3.Distance(
            player.position,
            enemy.position
        );

        float t = Mathf.Clamp01(distance / farDistance);

        currentEnemySource.volume = Mathf.Lerp(
            maxChaseVolume,
            1f,
            t
        );
    }
}