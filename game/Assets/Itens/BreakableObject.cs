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
    // DIREÇÃO DO ARREMESSO
    // =========================================================

    [Header("Direção dos cacos")]
    [Tooltip("Direção em que os cacos serão lançados.")]
    public Vector3 throwDirection = Vector3.forward;


    // =========================================================
    // CONTROLE
    // =========================================================

    private bool wasThrown = false;
    private bool hasBroken = false;


    // =========================================================
    // RECEBER DIREÇÃO
    // =========================================================

    public void SetThrowDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return;


        throwDirection =
            direction.normalized;
    }


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
        if (!wasThrown)
            return;


        if (hasBroken)
            return;


        // =====================================================
        // IGNORA O PLAYER
        // =====================================================

        PlayerPickup player =
            collision.collider
                .GetComponentInParent<PlayerPickup>();


        if (player != null)
        {
            return;
        }


        // =====================================================
        // QUEBRA
        // =====================================================

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
                rb.linearVelocity =
                    Vector3.zero;

                rb.angularVelocity =
                    Vector3.zero;

                rb.isKinematic =
                    true;

                rb.useGravity =
                    false;
            }


            breakAnimator.SetTrigger(
                breakTrigger
            );


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
            if (breakParticlePrefab == null)
            {
                Debug.LogError(
                    "BreakableObject: O campo " +
                    "'Break Particle Prefab' está vazio!",
                    gameObject
                );


                Destroy(
                    gameObject,
                    particleDestroyDelay
                );


                return;
            }


            // =================================================
            // DIREÇÃO DO ARREMESSO
            // =================================================

            Vector3 direction =
                throwDirection;


            if (direction.sqrMagnitude <= 0.001f)
            {
                direction =
                    transform.forward;
            }


            direction.Normalize();


            // =================================================
            // ROTAÇÃO DO PARTICLE SYSTEM
            // =================================================

            Quaternion particleRotation =
                Quaternion.FromToRotation(
                    Vector3.up,
                    direction
                );


            // =================================================
            // CRIA PARTÍCULAS
            // =================================================

            GameObject particles =
                Instantiate(
                    breakParticlePrefab,
                    transform.position,
                    particleRotation
                );


            // =================================================
            // INICIA PARTÍCULAS
            // =================================================

            ParticleSystem particleSystem =
                particles.GetComponent<ParticleSystem>();


            if (particleSystem != null)
            {
                particleSystem.Play();


                var main =
                    particleSystem.main;


                float particleLifetime =
                    main.duration +
                    main.startLifetime.constantMax +
                    0.5f;


                Destroy(
                    particles,
                    particleLifetime
                );
            }
            else
            {
                Debug.LogWarning(
                    "BreakableObject: O prefab configurado em " +
                    "'Break Particle Prefab' não possui " +
                    "um Particle System.",
                    particles
                );


                Destroy(
                    particles,
                    3f
                );
            }


            // =================================================
            // DESTROI OBJETO
            // =================================================

            Destroy(
                gameObject,
                particleDestroyDelay
            );
        }
    }
}