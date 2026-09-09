using UnityEngine;
using System.Collections;

public class AudioCodeManager : MonoBehaviour
{
    public static AudioCodeManager Instance;

    [Header("Áudios")]
    public AudioSource audioSource;
    public AudioClip intro;
    public AudioClip[] numeros = new AudioClip[10];

    [Header("Código gerado")]
    public string codigo;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GerarCodigo();
        StartCoroutine(TocarSequencia());
    }

    void GerarCodigo()
    {
        int centenas = Random.Range(0, 10);
        int dezenas = Random.Range(0, 10);
        int unidades = Random.Range(0, 10);

        codigo = $"{centenas}{dezenas}{unidades}";

        Debug.Log("Código gerado: " + codigo);
    }

    IEnumerator TocarSequencia()
    {
        // Espera 5 segundos antes de começar
        yield return new WaitForSeconds(5f);

        // Toca a introdução
        audioSource.clip = intro;
        audioSource.Play();

        yield return new WaitForSeconds(intro.length);

        // Toca os três dígitos
        yield return StartCoroutine(TocarNumero(codigo[0] - '0'));
        yield return StartCoroutine(TocarNumero(codigo[1] - '0'));
        yield return StartCoroutine(TocarNumero(codigo[2] - '0'));
    }

    IEnumerator TocarNumero(int numero)
    {
        audioSource.clip = numeros[numero];
        audioSource.Play();

        yield return new WaitForSeconds(audioSource.clip.length);
    }
}