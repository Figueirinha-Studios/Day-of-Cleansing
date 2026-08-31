using UnityEngine;

public class NoiseSource : MonoBehaviour
{
    [Header("Noise - Impacto Normal")]
    public float noiseRadius = 10f;


    [Header("Impact Audio - Arremesso")]
    public AudioClip impactSound;

    public float volume = 1f;


    [Header("Noise - Objeto Solto")]
    public float dropNoiseRadius = 2f;


    [Header("Drop Audio")]
    public AudioClip dropSound;

    public float dropVolume = 0.25f;


    private bool canMakeNoise = false;
    private bool hasLanded = false;

    // Define se o objeto foi apenas solto.
    private bool isDropped = false;


    // =========================================================
    // COLISÃO
    // =========================================================

    private void OnCollisionEnter(Collision collision)
    {
        // =====================================================
        // NÃO PODE FAZER SOM
        // =====================================================

        if (!canMakeNoise)
            return;


        // =====================================================
        // IGNORA COLISÃO COM O PLAYER
        // =====================================================

        PlayerPickup player =
            collision.collider
                .GetComponentInParent<PlayerPickup>();


        if (player != null)
        {
            return;
        }


        // =====================================================
        // EVITA VÁRIOS SONS
        // =====================================================

        if (hasLanded)
            return;


        hasLanded = true;


        // =====================================================
        // OBJETO FOI SOLTO
        // =====================================================

        if (isDropped)
        {
            if (dropSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    dropSound,
                    transform.position,
                    dropVolume
                );
            }


            NoiseSystem.EmitNoise(
                transform.position,
                dropNoiseRadius
            );


            return;
        }


        // =====================================================
        // OBJETO FOI ARREMESSADO
        // =====================================================

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(
                impactSound,
                transform.position,
                volume
            );
        }


        NoiseSystem.EmitNoise(
            transform.position,
            noiseRadius
        );
    }


    // =========================================================
    // PEGOU O OBJETO
    // =========================================================

    public void ResetNoise()
    {
        canMakeNoise = false;

        hasLanded = false;

        isDropped = false;
    }


    // =========================================================
    // ARREMESSOU
    // =========================================================

    public void EnableNoise()
    {
        canMakeNoise = true;

        hasLanded = false;

        isDropped = false;
    }


    // =========================================================
    // SOLTOU
    // =========================================================

    public void EnableDropNoise()
    {
        canMakeNoise = true;

        hasLanded = false;

        isDropped = true;
    }
}