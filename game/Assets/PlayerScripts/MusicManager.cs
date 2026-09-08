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


    // ---------------------------------------------------------
    // Diz se uma música de inimigo deve estar ativa.
    // ---------------------------------------------------------

    private bool enemyMusicActive = false;


    [Header("Fade")]
    public float fadeSpeed = 2f;

    public float maxChaseVolume = 1.5f;

    public float farDistance = 20f;


    [Header("Distance Volume")]
    public Transform player;
    public Transform enemy;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        PreparePlaylist();

        PlayNextAmbient();

        currentEnemySource =
            enemySourceA;

        nextEnemySource =
            enemySourceB;


        // -----------------------------------------------------
        // Garante que os dois canais de inimigo começam parados.
        // -----------------------------------------------------

        if (enemySourceA != null)
        {
            enemySourceA.Stop();
            enemySourceA.volume = 0f;
        }

        if (enemySourceB != null)
        {
            enemySourceB.Stop();
            enemySourceB.volume = 0f;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // -----------------------------------------------------
        // MÚSICA AMBIENTE
        // -----------------------------------------------------

        if (ambientSource != null &&
            !ambientSource.isPlaying &&
            !ambientPaused)
        {
            PlayNextAmbient();
        }


        // -----------------------------------------------------
        // MÚSICA DO INIMIGO
        // -----------------------------------------------------

        UpdateChaseVolume();
    }


    // =========================================================
    // PREPARAR PLAYLIST
    // =========================================================

    private void PreparePlaylist()
    {
        remainingTracks.Clear();

        foreach (AudioClip clip in ambientTracks)
        {
            if (clip != null)
            {
                remainingTracks.Add(clip);
            }
        }

        Shuffle();
    }


    // =========================================================
    // EMBARALHAR PLAYLIST
    // =========================================================

    private void Shuffle()
    {
        for (int i = 0;
             i < remainingTracks.Count;
             i++)
        {
            int random =
                Random.Range(
                    i,
                    remainingTracks.Count
                );


            AudioClip temp =
                remainingTracks[i];

            remainingTracks[i] =
                remainingTracks[random];

            remainingTracks[random] =
                temp;
        }
    }


    // =========================================================
    // TOCAR PRÓXIMA MÚSICA AMBIENTE
    // =========================================================

    private void PlayNextAmbient()
    {
        if (ambientSource == null)
            return;


        // -----------------------------------------------------
        // Se acabaram as músicas,
        // cria a playlist novamente.
        // -----------------------------------------------------

        if (remainingTracks.Count == 0)
        {
            PreparePlaylist();
        }


        if (remainingTracks.Count == 0)
            return;


        // -----------------------------------------------------
        // Pega a primeira música da lista.
        // -----------------------------------------------------

        AudioClip nextTrack =
            remainingTracks[0];

        remainingTracks.RemoveAt(0);


        ambientSource.clip =
            nextTrack;

        ambientSource.loop =
            false;

        ambientSource.Play();
    }


    // =========================================================
    // INICIAR MÚSICA DO INIMIGO
    // =========================================================

    private void StartEnemyMusic(
        AudioClip clip
    )
    {
        if (clip == null)
            return;

        if (currentEnemySource != null &&
            currentEnemySource.clip == clip &&
            currentEnemySource.isPlaying &&
            enemyMusicActive)
        {
            return;
        }


        // -----------------------------------------------------
        // Marca imediatamente como ativo.
        // -----------------------------------------------------

        enemyMusicActive = true;


        if (musicCoroutine != null)
        {
            StopCoroutine(
                musicCoroutine
            );

            musicCoroutine = null;
        }


        musicCoroutine =
            StartCoroutine(
                CrossFadeEnemy(clip)
            );
    }


    // =========================================================
    // CHASE
    // =========================================================

    public void StartChaseMusic()
    {
        StartEnemyMusic(
            chaseMusic
        );
    }


    // =========================================================
    // SEARCH
    // =========================================================

    public void StartSearchMusic()
    {
        StartEnemyMusic(
            searchMusic
        );
    }


    // =========================================================
    // CROSSFADE
    // =========================================================

    private IEnumerator CrossFadeEnemy(
        AudioClip clip
    )
    {
        yield return StartCoroutine(
            FadeOutAmbient()
        );


        if (nextEnemySource == null)
        {
            musicCoroutine = null;
            yield break;
        }


        // -----------------------------------------------------
        // Configura novo canal
        // -----------------------------------------------------

        nextEnemySource.Stop();

        nextEnemySource.clip =
            clip;

        nextEnemySource.loop =
            true;

        nextEnemySource.volume =
            0f;


        nextEnemySource.Play();


        // -----------------------------------------------------
        // Fade in
        // -----------------------------------------------------

        while (nextEnemySource.volume < 1f)
        {
            // Se a música foi cancelada enquanto fazia fade,
            // para imediatamente.
            if (!enemyMusicActive)
            {
                nextEnemySource.Stop();
                nextEnemySource.volume = 0f;

                musicCoroutine = null;

                yield break;
            }


            nextEnemySource.volume +=
                Time.deltaTime *
                fadeSpeed;


            if (currentEnemySource != null)
            {
                currentEnemySource.volume -=
                    Time.deltaTime *
                    fadeSpeed;
            }


            yield return null;
        }


        nextEnemySource.volume = 1f;


        // -----------------------------------------------------
        // Para canal antigo
        // -----------------------------------------------------

        if (currentEnemySource != null)
        {
            currentEnemySource.Stop();

            currentEnemySource.volume = 0f;
        }


        AudioSource temp =
            currentEnemySource;

        currentEnemySource =
            nextEnemySource;

        nextEnemySource =
            temp;


        musicCoroutine = null;
    }


    // =========================================================
    // FADE OUT AMBIENTE
    // =========================================================

    private IEnumerator FadeOutAmbient()
    {
        if (ambientSource == null)
            yield break;


        if (ambientPaused)
            yield break;


        while (ambientSource.volume > 0f)
        {
            ambientSource.volume -=
                Time.deltaTime *
                fadeSpeed;

            yield return null;
        }


        ambientSource.volume = 0f;

        ambientSource.Pause();

        ambientPaused = true;
    }


    // =========================================================
    // PARAR MÚSICA DO INIMIGO
    // =========================================================

    public void StopEnemyMusic()
    {
        // -----------------------------------------------------
        // Desliga primeiro o controle de volume por distância.
        // -----------------------------------------------------

        enemyMusicActive = false;


        if (musicCoroutine != null)
        {
            StopCoroutine(
                musicCoroutine
            );

            musicCoroutine = null;
        }


        musicCoroutine =
            StartCoroutine(
                FadeOutEnemy()
            );
    }


    // =========================================================
    // FADE OUT INIMIGO
    // =========================================================

    private IEnumerator FadeOutEnemy()
    {
        if (currentEnemySource != null)
        {
            while (currentEnemySource.volume > 0f)
            {
                currentEnemySource.volume -=
                    Time.deltaTime *
                    fadeSpeed;

                yield return null;
            }


            currentEnemySource.volume =
                0f;

            currentEnemySource.Stop();

            currentEnemySource.clip =
                null;
        }


        // -----------------------------------------------------
        // Também garante que o outro canal não fique tocando.
        // -----------------------------------------------------

        if (nextEnemySource != null)
        {
            nextEnemySource.Stop();

            nextEnemySource.volume =
                0f;

            nextEnemySource.clip =
                null;
        }


        yield return StartCoroutine(
            FadeInAmbient()
        );


        musicCoroutine = null;
    }


    // =========================================================
    // FADE IN AMBIENTE
    // =========================================================

    private IEnumerator FadeInAmbient()
    {
        if (ambientSource == null)
            yield break;


        ambientSource.UnPause();


        while (ambientSource.volume < 1f)
        {
            ambientSource.volume +=
                Time.deltaTime *
                fadeSpeed;

            yield return null;
        }


        ambientSource.volume =
            1f;


        ambientPaused = false;
    }


    // =========================================================
    // VOLUME DO CHASE POR DISTÂNCIA
    // =========================================================

    private void UpdateChaseVolume()
    {
        // -----------------------------------------------------
        // NÃO mexe no volume durante FadeOutEnemy().
        // -----------------------------------------------------

        if (!enemyMusicActive)
            return;


        if (currentEnemySource == null)
            return;


        if (!currentEnemySource.isPlaying)
            return;


        if (currentEnemySource.clip != chaseMusic)
            return;


        if (player == null ||
            enemy == null)
            return;


        float distance =
            Vector3.Distance(
                player.position,
                enemy.position
            );


        float t =
            Mathf.Clamp01(
                distance /
                farDistance
            );


        currentEnemySource.volume =
            Mathf.Lerp(
                maxChaseVolume,
                1f,
                t
            );
    }
}