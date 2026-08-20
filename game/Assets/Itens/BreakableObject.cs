using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public enum BreakMode
    {
        Simple,
        Animation,
        Particles
    }

    [Header("Modo de quebra")]
    public BreakMode breakMode = BreakMode.Simple;

    [Header("Configuração")]
    public bool breakOnThrow = true;

    [Tooltip("Tempo antes de destruir no modo simples.")]
    public float destroyDelay = 0f;

    // =========================================================
    // ANIMAÇÃO
    // =========================================================

    [Header("Animação")]
    public Animator breakAnimator;

    [Tooltip("Nome do Trigger da animação.")]
    public string breakTrigger = "Break";

    [Tooltip("Tempo para destruir depois da animação.")]
    public float animationDestroyDelay = 5f;

    // =========================================================
    // PARTÍCULAS
    // =========================================================

    [Header("Partículas")]
    [Tooltip("Prefab do Particle System usado na quebra.")]
    public GameObject breakParticlePrefab;

    [Tooltip("Tempo até destruir o objeto depois de gerar as partículas.")]
    public float particleDestroyDelay = 0f;

    // =========================================================
    // CONTROLE
    // =========================================================

    private bool wasThrown = false;

    private bool hasBroken = false;

    // =========================================================
    // ATIVAR QUEBRA
    // =========================================================

    public void EnableBreakOnThrow()
    {
        if (!breakOnThrow)
            return;

        wasThrown = true;

        hasBroken = false;
    }

    // =========================================================
    // DESATIVAR QUEBRA
    // =========================================================

    public void DisableBreakOnThrow()
    {
        wasThrown = false;

        hasBroken = false;
    }

    // =========================================================
    // COLISÃO
    // =========================================================

    private void OnCollisionEnter(Collision collision)
    {
        // Só quebra se foi arremessado.
        if (!wasThrown)
            return;

        // Evita quebrar várias vezes.
        if (hasBroken)
            return;

        hasBroken = true;

        Break();
    }

    // =========================================================
    // QUEBRAR
    // =========================================================

    private void Break()
    {
        // =====================================================
        // SIMPLE
        // =====================================================

        if (breakMode == BreakMode.Simple)
        {
            Destroy(
                gameObject,
                destroyDelay
            );

            return;
        }

        // =====================================================
        // ANIMATION
        // =====================================================

        if (breakMode == BreakMode.Animation)
        {
            if (breakAnimator == null)
            {
                Debug.LogWarning(
                    "BreakableObject: Nenhum Animator foi configurado. " +
                    "O objeto será destruído normalmente.",
                    gameObject
                );

                Destroy(
                    gameObject,
                    destroyDelay
                );

                return;
            }

            Rigidbody rb =
                GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;

                rb.angularVelocity = Vector3.zero;

                rb.isKinematic = true;

                rb.useGravity = false;
            }

            // Toca a animação.
            breakAnimator.SetTrigger(
                breakTrigger
            );

            // Destrói depois da animação.
            Destroy(
                gameObject,
                animationDestroyDelay
            );

            return;
        }

        // =====================================================
        // PARTICLES
        // =====================================================

        if (breakMode == BreakMode.Particles)
        {
            if (breakParticlePrefab != null)
            {
                GameObject particles =
                    Instantiate(
                        breakParticlePrefab,
                        transform.position,
                        Quaternion.identity
                    );

                ParticleSystem particleSystem =
                    particles.GetComponent<ParticleSystem>();

                if (particleSystem != null)
                {
                    float particleLifetime =
                        particleSystem.main.duration +
                        particleSystem.main.startLifetime.constantMax +
                        0.5f;

                    Destroy(
                        particles,
                        particleLifetime
                    );
                }
                else
                {
                    Destroy(
                        particles,
                        3f
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    "BreakableObject: Nenhum Particle Prefab foi configurado.",
                    gameObject
                );
            }

            // Remove a garrafa.
            Destroy(
                gameObject,
                particleDestroyDelay
            );
        }
    }
}