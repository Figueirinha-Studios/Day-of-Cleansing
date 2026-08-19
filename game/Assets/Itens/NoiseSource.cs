using UnityEngine;

public class NoiseSource : MonoBehaviour
{
    [Header("Noise - Impacto Normal")]
    public float noiseRadius = 10f;

    [Header("Impact Audio - Arremesso")]
    public AudioClip impactSound;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Noise - Objeto Solto")]
    public float dropNoiseRadius = 2f;

    [Header("Drop Audio")]
    public AudioClip dropSound;

    [Range(0f, 1f)]
    public float dropVolume = 0.25f;

    private bool canMakeNoise = false;
    private bool hasLanded = false;

    // Define qual tipo de impacto o objeto terá.
    private bool isDropped = false;

    private void OnCollisionEnter(Collision collision)
    {
        // O objeto só pode produzir som depois
        // que foi arremessado ou solto.
        if (!canMakeNoise)
            return;

        // Evita vários sons enquanto o objeto quica.
        if (hasLanded)
            return;

        hasLanded = true;

        // -----------------------------------------
        // OBJETO FOI SOLTO COM E
        // -----------------------------------------

        if (isDropped)
        {
            // Som baixo ao tocar a superfície.
            if (dropSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    dropSound,
                    transform.position,
                    dropVolume
                );
            }

            // Ruído pequeno para os inimigos.
            NoiseSystem.EmitNoise(
                transform.position,
                dropNoiseRadius
            );

            return;
        }

        // -----------------------------------------
        // OBJETO FOI ARREMESSADO
        // -----------------------------------------

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(
                impactSound,
                transform.position,
                volume
            );
        }

        // Ruído maior para os inimigos.
        NoiseSystem.EmitNoise(
            transform.position,
            noiseRadius
        );
    }

    // Chamado quando o jogador pega o objeto.
    public void ResetNoise()
    {
        canMakeNoise = false;
        hasLanded = false;
        isDropped = false;
    }

    // Chamado quando o jogador arremessa o objeto
    // com o botão esquerdo.
    public void EnableNoise()
    {
        canMakeNoise = true;
        hasLanded = false;
        isDropped = false;
    }

    // Chamado quando o jogador solta o objeto
    // com a tecla E.
    public void EnableDropNoise()
    {
        canMakeNoise = true;
        hasLanded = false;
        isDropped = true;
    }
}