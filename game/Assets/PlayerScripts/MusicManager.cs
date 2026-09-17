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

    private List<AudioClip> remainingTracks =
        new List<AudioClip>();


    [Header("Enemy Music")]
    public AudioClip chaseMusic;
    public AudioClip searchMusic;

    private AudioSource currentEnemySource;
    private AudioSource nextEnemySource;


    private Coroutine musicCoroutine;


    private bool ambientPaused = false;


    // =========================================================
    // CONTROLE DA MÚSICA DO INIMIGO
    // =========================================================

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
        // Garante que os dois canais começam parados.
        // -----------------------------------------------------

        if (enemySourceA != null)
        {
            enemySourceA.Stop();
            enemySourceA.volume = 0f;
            enemySourceA.clip = null;
        }


        if (enemySourceB != null)
        {
            enemySourceB.Stop();
            enemySourceB.volume = 0f;
            enemySourceB.clip = null;
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


        if (remainingTracks.Count == 0)
        {
            PreparePlaylist();
        }


        if (remainingTracks.Count == 0)
            return;


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
        // ATIVA IMEDIATAMENTE.
        // -----------------------------------------------------

        enemyMusicActive = true;


        // -----------------------------------------------------
        // CANCELA QUALQUER FADE ANTERIOR.
        // -----------------------------------------------------

        if (musicCoroutine != null)
        {
            StopCoroutine(
                musicCoroutine
            );

            musicCoroutine = null;
        }


        // -----------------------------------------------------
        // GARANTE QUE O AMBIENTE ESTÁ PARADO.
        // -----------------------------------------------------

        if (ambientSource != null)
        {
            ambientSource.volume = 0f;

            if (ambientSource.isPlaying)
            {
                ambientSource.Pause();
            }

            ambientPaused = true;
        }


        // -----------------------------------------------------
        // INICIA CROSSFADE.
        // -----------------------------------------------------

        musicCoroutine =
            StartCoroutine(
                CrossFadeEnemy(
                    clip
                )
            );
    }


    // =========================================================
    // CHASE
    // =========================================================

    public void StartChaseMusic()
    {
        Debug.Log(
            "MUSIC MANAGER: Iniciando música de CHASE."
        );

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
        if (clip == null)
        {
            musicCoroutine = null;
            yield break;
        }


        // -----------------------------------------------------
        // Garante que o ambiente está parado.
        // -----------------------------------------------------

        if (ambientSource != null)
        {
            ambientSource.volume = 0f;

            if (ambientSource.isPlaying)
            {
                ambientSource.Pause();
            }

            ambientPaused = true;
        }


        // -----------------------------------------------------
        // VERIFICA CANAL.
        // -----------------------------------------------------

        if (nextEnemySource == null)
        {
            musicCoroutine = null;
            yield break;
        }


        // -----------------------------------------------------
        // CONFIGURA NOVO CANAL.
        // -----------------------------------------------------

        nextEnemySource.Stop();

        nextEnemySource.clip =
            clip;

        nextEnemySource.loop =
            true;

        nextEnemySource.volume =
            0f;


        // -----------------------------------------------------
        // COMEÇA MÚSICA.
        // -----------------------------------------------------

        nextEnemySource.Play();


        // -----------------------------------------------------
        // FADE IN.
        // -----------------------------------------------------

        while (
            nextEnemySource != null &&
            nextEnemySource.volume < 1f
        )
        {
            // -------------------------------------------------
            // Se foi cancelado.
            // -------------------------------------------------

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


            // -------------------------------------------------
            // Diminui canal antigo.
            // -------------------------------------------------

            if (currentEnemySource != null &&
                currentEnemySource != nextEnemySource)
            {
                currentEnemySource.volume -=
                    Time.deltaTime *
                    fadeSpeed;

                if (currentEnemySource.volume < 0f)
                {
                    currentEnemySource.volume =
                        0f;
                }
            }


            yield return null;
        }


        // -----------------------------------------------------
        // Garante volume inicial.
        // -----------------------------------------------------

        if (nextEnemySource != null)
        {
            nextEnemySource.volume =
                1f;
        }


        // -----------------------------------------------------
        // PARA CANAL ANTIGO.
        // -----------------------------------------------------

        if (currentEnemySource != null &&
            currentEnemySource != nextEnemySource)
        {
            currentEnemySource.Stop();

            currentEnemySource.volume =
                0f;
        }


        // -----------------------------------------------------
        // TROCA OS CANAIS.
        // -----------------------------------------------------

        AudioSource temp =
            currentEnemySource;

        currentEnemySource =
            nextEnemySource;

        nextEnemySource =
            temp;


        musicCoroutine = null;
    }


    // =========================================================
    // PARAR MÚSICA DO INIMIGO
    // =========================================================

    public void StopEnemyMusic()
    {
        Debug.Log(
            "MUSIC MANAGER: Parando música do inimigo."
        );


        // -----------------------------------------------------
        // DESATIVA PRIMEIRO.
        // -----------------------------------------------------

        enemyMusicActive =
            false;


        // -----------------------------------------------------
        // CANCELA QUALQUER CORROTINA ANTERIOR.
        // -----------------------------------------------------

        if (musicCoroutine != null)
        {
            StopCoroutine(
                musicCoroutine
            );

            musicCoroutine = null;
        }


        // -----------------------------------------------------
        // INICIA FADE OUT.
        // -----------------------------------------------------

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
        // -----------------------------------------------------
        // FADE DO CANAL ATUAL.
        // -----------------------------------------------------

        if (currentEnemySource != null)
        {
            while (
                currentEnemySource != null &&
                currentEnemySource.volume > 0f
            )
            {
                currentEnemySource.volume -=
                    Time.deltaTime *
                    fadeSpeed;

                if (currentEnemySource.volume < 0f)
                {
                    currentEnemySource.volume =
                        0f;
                }

                yield return null;
            }


            if (currentEnemySource != null)
            {
                currentEnemySource.volume =
                    0f;

                currentEnemySource.Stop();

                currentEnemySource.clip =
                    null;
            }
        }


        // -----------------------------------------------------
        // GARANTE QUE O OUTRO CANAL TAMBÉM PAROU.
        // -----------------------------------------------------

        if (nextEnemySource != null)
        {
            nextEnemySource.Stop();

            nextEnemySource.volume =
                0f;

            nextEnemySource.clip =
                null;
        }


        // -----------------------------------------------------
        // AMBIENTE.
        // -----------------------------------------------------

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


        while (
            ambientSource.volume < 1f
        )
        {
            // -------------------------------------------------
            // Se uma música de inimigo começou,
            // cancela o fade do ambiente.
            // -------------------------------------------------

            if (enemyMusicActive)
            {
                yield break;
            }


            ambientSource.volume +=
                Time.deltaTime *
                fadeSpeed;


            if (ambientSource.volume > 1f)
            {
                ambientSource.volume =
                    1f;
            }


            yield return null;
        }


        ambientSource.volume =
            1f;

        ambientPaused =
            false;
    }


    // =========================================================
    // VOLUME DO CHASE POR DISTÂNCIA
    // =========================================================

    private void UpdateChaseVolume()
    {
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