
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemFallRespawnZone : MonoBehaviour
{
    [Header("Imagem do aviso")]
    public RawImage warningImage;

    [Header("Som do aviso")]
    public AudioSource warningAudioSource;
    public AudioClip warningSound;

    [Header("Configurações")]
    public float warningDuration = 3f;

    private Coroutine warningCoroutine;

    private void Awake()
    {
        if (warningImage != null)
        {
            warningImage.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar se o objeto é um item configurado para retornar.
        ItemFallRespawn item =
            other.GetComponentInParent<ItemFallRespawn>();

        if (item == null)
        {
            return;
        }

        // Retornar o item ao local inicial.
        item.ReturnToStart();

        // Mostrar a imagem e tocar o som.
        ShowWarning();
    }

    private void ShowWarning()
    {
        if (warningImage == null)
        {
            return;
        }

        // Reiniciar o tempo do aviso caso outro item caia.
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }

        warningCoroutine = StartCoroutine(ShowWarningCoroutine());
    }

    private IEnumerator ShowWarningCoroutine()
    {
        // Mostrar a Raw Image.
        warningImage.gameObject.SetActive(true);

        // Reproduzir o som do aviso.
        if (warningAudioSource != null && warningSound != null)
        {
            warningAudioSource.PlayOneShot(warningSound);
        }

        // Aguardar o tempo configurado.
        yield return new WaitForSeconds(warningDuration);

        // Esconder a imagem.
        warningImage.gameObject.SetActive(false);

        warningCoroutine = null;
    }
}
