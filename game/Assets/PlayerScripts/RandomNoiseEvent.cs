using UnityEngine;
using System.Collections;

public class RandomNoiseEvent : MonoBehaviour
{
    [Header("Sorteio")]
    public float interval = 60f;

    [Range(0f, 100f)]
    public float probability = 15f;

    [Header("Evento")]
    public RemoteNoisePoint playerNoisePoint;

    [Header("Som Contínuo")]
    public float noiseInterval = 1f;
    public float noiseDuration = 15f;

    private float timer;
    private bool eventRunning;

    void Start()
    {
        timer = interval;

        Debug.Log(
            "RANDOM NOISE EVENT INICIADO!"
        );
    }

    void Update()
    {
        if (eventRunning)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = interval;

            TryTriggerEvent();
        }
    }

    void TryTriggerEvent()
    {
        float chance = Random.Range(0f, 100f);

        Debug.Log(
            "RANDOM NOISE: sorteio = " +
            chance.ToString("F2") +
            "% | necessário <= " +
            probability +
            "%"
        );

        if (chance <= probability)
        {
            Debug.Log(
                "RANDOM NOISE: EVENTO ATIVADO!"
            );

            if (playerNoisePoint == null)
            {
                Debug.LogError(
                    "RANDOM NOISE: playerNoisePoint NÃO ESTÁ CONFIGURADO!"
                );

                return;
            }

            StartCoroutine(NoiseSequence());
        }
        else
        {
            Debug.Log(
                "RANDOM NOISE: nada aconteceu."
            );
        }
    }

    IEnumerator NoiseSequence()
    {
        eventRunning = true;

        float elapsed = 0f;

        Debug.Log(
            "RANDOM NOISE: iniciando sequência de " +
            noiseDuration +
            " segundos."
        );

        while (elapsed < noiseDuration)
        {
            Debug.Log(
                "RANDOM NOISE: Player emitindo som!"
            );

            playerNoisePoint.EmitNoise();

            yield return new WaitForSeconds(noiseInterval);

            elapsed += noiseInterval;
        }

        Debug.Log(
            "RANDOM NOISE: sequência terminou."
        );

        eventRunning = false;
    }
}