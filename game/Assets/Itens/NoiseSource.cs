using UnityEngine;

public class NoiseSource : MonoBehaviour
{
    [Header("Noise")]
    public float noiseRadius = 10f;

    [Header("Impact Audio")]
    public AudioClip impactSound;

    [Range(0f, 1f)]
    public float volume = 1f;

    private bool canMakeNoise = false;
    private bool hasLanded = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Só produz som depois que o objeto foi arremessado.
        if (!canMakeNoise)
            return;

        // Evita vários sons caso o objeto quique.
        if (hasLanded)
            return;

        hasLanded = true;

        // Som do impacto
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(
                impactSound,
                transform.position,
                volume
            );
        }

        // Avisa o sistema de audição dos inimigos
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
    }

    // Chamado quando o jogador arremessa o objeto.
    public void EnableNoise()
    {
        canMakeNoise = true;
        hasLanded = false;
    }
}