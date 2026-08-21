using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MapController : MonoBehaviour
{
    [Header("MAPA")]
    public GameObject mapCanvas;
    public GameObject mapImage;
    public GameObject mapVideo;

    [Header("TELA PRETA")]
    public Image blackScreen;

    [Header("VIDEO")]
    public VideoPlayer videoPlayer;

    [Header("AUDIO DO VIDEO")]
    public AudioSource mapVideoAudio;

    [Header("SONS DE TRANSICAO")]
    public AudioSource openStartSound;
    public AudioSource openEndSound;
    public AudioSource closeStartSound;
    public AudioSource closeEndSound;

    [Header("PLAYER")]
    public PlayerMovement playerMovement;
    public CameraController cameraController;
    public PlayerPickup playerPickup;

    [Header("ELEVADOR")]
    public Transform elevator;

    [Header("CONFIGURACOES")]
    public float transitionTime = 1f;

    [Header("VOLUME DO JOGO NO MAPA")]
    [Range(0f, 1f)]
    public float mapVolume = 0.20f;

    [Header("VOLUME DO JOGO DURANTE A TRANSICAO")]
    [Range(0f, 1f)]
    public float transitionVolume = 0f;


    // =========================================================
    // VARIAVEIS INTERNAS
    // =========================================================

    private bool mapOpen = false;
    private bool transitionRunning = false;

    private Dictionary<AudioSource, float> originalVolumes =
        new Dictionary<AudioSource, float>();

    private Transform originalPlayerParent;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Mapa começa fechado
        if (mapCanvas != null)
            mapCanvas.SetActive(false);

        if (mapImage != null)
            mapImage.SetActive(false);

        if (mapVideo != null)
            mapVideo.SetActive(false);

        // Tela preta invisível
        SetBlackScreen(false);

        // Encontra os áudios do jogo
        RegisterAllAudioSources();

        // Jogo começa com áudio normal
        SetGameAudioNormal();

        // Vídeo começa parado
        if (videoPlayer != null)
            videoPlayer.Stop();

        // Áudio do vídeo começa parado
        if (mapVideoAudio != null)
            mapVideoAudio.Stop();

        // Garante que o jogador começa liberado
        SetPlayerControl(true);
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Procura AudioSources novos
        RegisterNewAudioSources();

        // Aperta M
        if (Input.GetKeyDown(KeyCode.M) && !transitionRunning)
        {
            if (!mapOpen)
            {
                StartCoroutine(OpenMap());
            }
            else
            {
                StartCoroutine(CloseMap());
            }
        }
    }


    // =========================================================
    // ABRIR MAPA
    // =========================================================

    private IEnumerator OpenMap()
    {
        transitionRunning = true;

        // -----------------------------------------------------
        // MOMENTO 1
        // -----------------------------------------------------

        // Ativa Canvas
        if (mapCanvas != null)
            mapCanvas.SetActive(true);

        // Tela preta imediatamente
        SetBlackScreen(true);

        // Silencia o jogo imediatamente
        SetGameAudioSilent();

        // Prende o jogador ao elevador
        AttachPlayerToElevator();

        // Bloqueia jogador
        SetPlayerControl(false);

        // Som da transição
        PlayTransitionSound(openStartSound);


        // -----------------------------------------------------
        // ESPERA
        // -----------------------------------------------------

        yield return new WaitForSecondsRealtime(transitionTime);


        // -----------------------------------------------------
        // MOMENTO 2
        // -----------------------------------------------------

        // Mostra vídeo
        if (mapVideo != null)
        {
            mapVideo.SetActive(true);

            if (videoPlayer != null)
            {
                videoPlayer.Play();
            }
        }

        // Esconde imagem
        if (mapImage != null)
            mapImage.SetActive(false);

        // Remove tela preta
        SetBlackScreen(false);

        // Som do mapa aparecendo
        PlayTransitionSound(openEndSound);

        // Jogo volta com volume reduzido
        SetGameAudioMuffled();

        mapOpen = true;

        transitionRunning = false;
    }


    // =========================================================
    // FECHAR MAPA
    // =========================================================

    private IEnumerator CloseMap()
    {
        transitionRunning = true;

        // -----------------------------------------------------
        // MOMENTO 1
        // -----------------------------------------------------

        // Tela preta imediatamente
        SetBlackScreen(true);

        // Silencia jogo imediatamente
        SetGameAudioSilent();

        // Som de fechamento
        PlayTransitionSound(closeStartSound);


        // -----------------------------------------------------
        // ESPERA
        // -----------------------------------------------------

        yield return new WaitForSecondsRealtime(transitionTime);


        // -----------------------------------------------------
        // MOMENTO 2
        // -----------------------------------------------------

        // Para vídeo
        if (videoPlayer != null)
            videoPlayer.Stop();

        // Para áudio do vídeo
        if (mapVideoAudio != null)
            mapVideoAudio.Stop();

        // Esconde vídeo
        if (mapVideo != null)
            mapVideo.SetActive(false);

        // Esconde imagem
        if (mapImage != null)
            mapImage.SetActive(false);

        // Desativa Canvas
        if (mapCanvas != null)
            mapCanvas.SetActive(false);

        // Som de retorno
        PlayTransitionSound(closeEndSound);

        // Áudio normal
        SetGameAudioNormal();

        // Remove jogador do elevador
        DetachPlayerFromElevator();

        // Libera jogador
        SetPlayerControl(true);

        mapOpen = false;

        transitionRunning = false;
    }


    // =========================================================
    // PRENDER PLAYER AO ELEVADOR
    // =========================================================

    private void AttachPlayerToElevator()
    {
        if (elevator == null)
        {
            Debug.LogWarning(
                "MapController: Nenhum elevador foi configurado no Inspector."
            );

            return;
        }

        if (playerMovement == null)
        {
            Debug.LogWarning(
                "MapController: PlayerMovement não foi configurado."
            );

            return;
        }

        Transform playerTransform =
            playerMovement.transform;

        // Guarda o Parent original
        originalPlayerParent =
            playerTransform.parent;

        // Torna o Player filho do elevador
        // Mantém a posição atual no mundo
        playerTransform.SetParent(
            elevator,
            true
        );
    }


    // =========================================================
    // SOLTAR PLAYER DO ELEVADOR
    // =========================================================

    private void DetachPlayerFromElevator()
    {
        if (playerMovement == null)
            return;

        Transform playerTransform =
            playerMovement.transform;

        // Volta para o Parent original
        playerTransform.SetParent(
            originalPlayerParent,
            true
        );
    }


    // =========================================================
    // TOCAR SONS DE TRANSICAO
    // =========================================================

    private void PlayTransitionSound(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            Debug.LogWarning(
                "MapController: AudioSource de transicao nao foi configurado."
            );

            return;
        }

        if (audioSource.clip == null)
        {
            Debug.LogWarning(
                "MapController: O AudioSource '" +
                audioSource.name +
                "' nao possui AudioClip."
            );

            return;
        }

        audioSource.PlayOneShot(
            audioSource.clip
        );
    }


    // =========================================================
    // SILENCIAR JOGO
    // =========================================================

    private void SetGameAudioSilent()
    {
        RegisterNewAudioSources();

        foreach (KeyValuePair<AudioSource, float> pair
                 in originalVolumes)
        {
            AudioSource audio = pair.Key;

            if (audio == null)
                continue;

            // Ignora sons do mapa
            if (IsMapAudio(audio))
                continue;

            audio.volume =
                transitionVolume;
        }
    }


    // =========================================================
    // SOM ABAFADO
    // =========================================================

    private void SetGameAudioMuffled()
    {
        RegisterNewAudioSources();

        foreach (KeyValuePair<AudioSource, float> pair
                 in originalVolumes)
        {
            AudioSource audio = pair.Key;

            if (audio == null)
                continue;

            // Ignora sons do mapa
            if (IsMapAudio(audio))
                continue;

            float originalVolume =
                pair.Value;

            audio.volume =
                originalVolume *
                mapVolume;
        }
    }


    // =========================================================
    // SOM NORMAL
    // =========================================================

    private void SetGameAudioNormal()
    {
        RegisterNewAudioSources();

        foreach (KeyValuePair<AudioSource, float> pair
                 in originalVolumes)
        {
            AudioSource audio = pair.Key;

            if (audio == null)
                continue;

            // Ignora sons do mapa
            if (IsMapAudio(audio))
                continue;

            audio.volume =
                pair.Value;
        }
    }


    // =========================================================
    // ENCONTRAR TODOS OS AUDIOS
    // =========================================================

    private void RegisterAllAudioSources()
    {
        AudioSource[] audios =
            FindObjectsByType<AudioSource>(
                FindObjectsSortMode.None
            );

        foreach (AudioSource audio in audios)
        {
            RegisterAudioSource(audio);
        }
    }


    // =========================================================
    // ENCONTRAR NOVOS AUDIOS
    // =========================================================

    private void RegisterNewAudioSources()
    {
        AudioSource[] audios =
            FindObjectsByType<AudioSource>(
                FindObjectsSortMode.None
            );

        foreach (AudioSource audio in audios)
        {
            if (!originalVolumes.ContainsKey(audio))
            {
                RegisterAudioSource(audio);

                // Se o mapa já estiver aberto,
                // aplica o volume reduzido
                if (mapOpen && !IsMapAudio(audio))
                {
                    audio.volume *= mapVolume;
                }
            }
        }
    }


    // =========================================================
    // REGISTRAR AUDIO
    // =========================================================

    private void RegisterAudioSource(
        AudioSource audio)
    {
        if (audio == null)
            return;

        if (originalVolumes.ContainsKey(audio))
            return;

        // Não registra sons do mapa
        if (IsMapAudio(audio))
            return;

        // Guarda volume original
        originalVolumes.Add(
            audio,
            audio.volume
        );
    }


    // =========================================================
    // IDENTIFICAR AUDIO DO MAPA
    // =========================================================

    private bool IsMapAudio(
        AudioSource audio)
    {
        if (audio == null)
            return true;

        if (audio == openStartSound)
            return true;

        if (audio == openEndSound)
            return true;

        if (audio == closeStartSound)
            return true;

        if (audio == closeEndSound)
            return true;

        if (audio == mapVideoAudio)
            return true;

        // Qualquer AudioSource dentro
        // do MapSystem
        if (audio.transform.IsChildOf(transform))
            return true;

        return false;
    }


    // =========================================================
    // TELA PRETA
    // =========================================================

    private void SetBlackScreen(
        bool visible)
    {
        if (blackScreen == null)
            return;

        Color color =
            blackScreen.color;

        if (visible)
            color.a = 1f;
        else
            color.a = 0f;

        blackScreen.color =
            color;
    }


    // =========================================================
    // CONTROLE DO PLAYER
    // =========================================================

    private void SetPlayerControl(
        bool enabled)
    {
        if (playerMovement != null)
            playerMovement.enabled = enabled;

        if (cameraController != null)
            cameraController.enabled = enabled;

        if (playerPickup != null)
            playerPickup.enabled = enabled;
    }
}