
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [Header("Configuração do cronômetro")]
    [Min(1)]
    public int durationMinutes = 10;

    [Header("Avisos a cada 2 minutos")]
    [Tooltip("AudioSource para o aviso 3D do cronômetro.")]
    public AudioSource warningAudioSource;

    public AudioClip warningSound;

    [Header("Últimos 10 segundos")]
    [Tooltip("AudioSource para os bipes finais.")]
    public AudioSource finalBeepAudioSource;

    public AudioClip finalBeepSound;

    [Header("Bipes rápidos ao chegar em zero")]
    public AudioClip rapidBeepSound;

    [Min(0.05f)]
    public float rapidBeepInterval = 0.15f;

    [Min(0.1f)]
    public float rapidBeepDuration = 3f;

    [Header("Sequência final")]
    public AudioSource sirenAudioSource;
    public AudioClip sirenSound;

    [Min(0.1f)]
    public float sirenDuration = 10f;

    public AudioSource explosionAudioSource;
    public AudioClip explosionSound;

    [Tooltip("Imagem preta que cobre a tela inteira.")]
    public Image blackScreen;

    [Min(0f)]
    public float blackScreenDelay = 2f;

    [Header("Cena de Game Over")]
    [Tooltip("Nome exato da cena de Game Over.")]
    public string gameOverSceneName = "GameOver";

    [Header("Estado atual")]
    [SerializeField]
    private int remainingSeconds;

    public int RemainingSeconds => remainingSeconds;

    public bool TimerFinished { get; private set; }

    private float countdown;
    private int previousDisplayedSecond;
    private bool sequenceStarted;

    private void Awake()
    {
        remainingSeconds = Mathf.Max(1, durationMinutes) * 60;
        countdown = remainingSeconds;
        previousDisplayedSecond = remainingSeconds;

        if (blackScreen != null)
        {
            Color color = blackScreen.color;
            color.a = 0f;
            blackScreen.color = color;
            blackScreen.raycastTarget = false;
        }

        // O cronômetro não chama NoiseSystem nem avisa o Chappie.
    }

    private void Start()
    {
        UpdateRemainingSeconds();
    }

    private void Update()
    {
        if (TimerFinished)
            return;

        countdown -= Time.unscaledDeltaTime;

        UpdateRemainingSeconds();

        if (countdown <= 0f)
        {
            countdown = 0f;
            remainingSeconds = 0;
            TimerFinished = true;

            if (!sequenceStarted)
            {
                sequenceStarted = true;
                StartCoroutine(FinalSequence());
            }
        }
    }

    private void UpdateRemainingSeconds()
    {
        int newSecond = Mathf.Max(0, Mathf.CeilToInt(countdown));

        if (newSecond == remainingSeconds &&
            newSecond == previousDisplayedSecond)
        {
            return;
        }

        remainingSeconds = newSecond;

        // Avisos aos 8, 6, 4 e 2 minutos restantes.
        if (remainingSeconds > 0 &&
            remainingSeconds < durationMinutes * 60 &&
            remainingSeconds % 120 == 0)
        {
            PlayWarning();
        }

        // Um bipe por segundo nos últimos 10 segundos.
        if (remainingSeconds >= 1 &&
            remainingSeconds <= 10 &&
            remainingSeconds != previousDisplayedSecond)
        {
            PlayFinalBeep();
        }

        previousDisplayedSecond = remainingSeconds;
    }

    private void PlayWarning()
    {
        if (warningAudioSource != null && warningSound != null)
        {
            warningAudioSource.PlayOneShot(warningSound);
        }
    }

    private void PlayFinalBeep()
    {
        if (finalBeepAudioSource != null && finalBeepSound != null)
        {
            finalBeepAudioSource.PlayOneShot(finalBeepSound);
        }
    }

    private IEnumerator FinalSequence()
    {
        remainingSeconds = 0;

        // Interrompe os avisos normais.
        if (warningAudioSource != null)
            warningAudioSource.Stop();

        if (finalBeepAudioSource != null)
            finalBeepAudioSource.Stop();

        // 1. Bipes rápidos durante 3 segundos.
        float elapsed = 0f;

        while (elapsed < rapidBeepDuration)
        {
            if (finalBeepAudioSource != null &&
                rapidBeepSound != null)
            {
                finalBeepAudioSource.PlayOneShot(rapidBeepSound);
            }

            float waitTime = Mathf.Min(
                rapidBeepInterval,
                rapidBeepDuration - elapsed
            );

            yield return new WaitForSecondsRealtime(waitTime);
            elapsed += waitTime;
        }

        // Para os bipes antes da sirene.
        if (finalBeepAudioSource != null)
            finalBeepAudioSource.Stop();

        // 2. Sirene durante 10 segundos.
        if (sirenAudioSource != null && sirenSound != null)
        {
            sirenAudioSource.Stop();
            sirenAudioSource.clip = sirenSound;
            sirenAudioSource.loop = false;
            sirenAudioSource.Play();

            Debug.Log("GameTimer: sirene iniciada.");

            yield return new WaitForSecondsRealtime(sirenDuration);

            sirenAudioSource.Stop();
        }
        else
        {
            Debug.LogError(
                "GameTimer: configure Siren Audio Source e Siren Sound."
            );
        }

        // 3. Inicia a explosão e ativa a tela preta imediatamente.
        if (explosionAudioSource != null && explosionSound != null)
        {
            explosionAudioSource.Stop();
            explosionAudioSource.clip = explosionSound;
            explosionAudioSource.loop = false;
            explosionAudioSource.Play();

            Debug.Log("GameTimer: explosão iniciada.");

            if (blackScreen != null)
            {
                blackScreen.gameObject.SetActive(true);

                Color color = blackScreen.color;
                color.a = 1f;
                blackScreen.color = color;
                blackScreen.raycastTarget = true;
            }
            else
            {
                Debug.LogError(
                    "GameTimer: Black Screen não foi configurada."
                );
            }

            // Aguarda o término real do áudio da explosão.
            while (explosionAudioSource != null &&
                   explosionAudioSource.isPlaying)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogError(
                "GameTimer: configure Explosion Audio Source e Explosion Sound."
            );

            // Mantém a tela preta mesmo se o áudio não estiver configurado.
            if (blackScreen != null)
            {
                blackScreen.gameObject.SetActive(true);

                Color color = blackScreen.color;
                color.a = 1f;
                blackScreen.color = color;
                blackScreen.raycastTarget = true;
            }
        }

        // 4. Mantém a tela preta por mais 2 segundos.
        yield return new WaitForSecondsRealtime(blackScreenDelay);

        // 5. Carrega a cena de Game Over.
        if (string.IsNullOrWhiteSpace(gameOverSceneName))
        {
            Debug.LogError(
                "GameTimer: informe o nome da cena de Game Over."
            );
            yield break;
        }

        SceneManager.LoadScene(gameOverSceneName);
    }

    public string GetFormattedTime()
    {
        int minutes = remainingSeconds / 60;
        int seconds = remainingSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }
}
