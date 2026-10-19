using UnityEngine;
using UnityEngine.Audio;
using System.Collections; // Necessário para usar IEnumerator

public class HistoryElevator : MonoBehaviour
{
    [Header("Áudios")]
    public AudioSource audioSource;
    public AudioClip intro;

    // Mudamos o tipo de retorno de 'void' para 'IEnumerator'
    IEnumerator Start()
    {
        // Agora o 'yield return' diz para o Unity esperar 5 segundos reais
        yield return new WaitForSeconds(5f);

        audioSource.clip = intro;
        audioSource.Play();
    }
}