using System.Collections;
using UnityEngine;

public class SomPeriodico : MonoBehaviour
{
    [Header("Áudio")]
    public AudioSource audioSource;
    public AudioClip som;

    [Header("Intervalo Aleatório")]
    public float intervaloMinimo = 30f;
    public float intervaloMaximo = 120f;

    private Coroutine scareCoroutine;

    private void Start()
    {
        scareCoroutine = StartCoroutine(ScareLoop());
    }

    private IEnumerator ScareLoop()
    {
        while (true)
        {
            // -------------------------------------------------
            // Escolhe um intervalo aleatório entre 30 e 120s.
            // -------------------------------------------------

            float espera =
                Random.Range(
                    intervaloMinimo,
                    intervaloMaximo
                );

            Debug.Log(
                "SCARE SOUND: Próximo som em " +
                espera.ToString("F1") +
                " segundos."
            );

            yield return new WaitForSeconds(espera);


            // -------------------------------------------------
            // Verificações de segurança
            // -------------------------------------------------

            if (audioSource == null)
                continue;

            if (som == null)
                continue;


            // -------------------------------------------------
            // Toca o som.
            // -------------------------------------------------

            audioSource.PlayOneShot(som);

            Debug.Log(
                "SCARE SOUND: Som tocado!"
            );


            // -------------------------------------------------
            // Espera o som terminar antes de iniciar uma nova
            // contagem.
            // -------------------------------------------------

            yield return new WaitForSeconds(
                som.length
            );
        }
    }

    private void OnDisable()
    {
        if (scareCoroutine != null)
        {
            StopCoroutine(
                scareCoroutine
            );

            scareCoroutine = null;
        }
    }
}