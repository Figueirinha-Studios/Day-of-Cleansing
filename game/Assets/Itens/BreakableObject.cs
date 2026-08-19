using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public enum BreakMode
    {
        Simple,
        Animation
    }

    [Header("Modo de quebra")]
    public BreakMode breakMode = BreakMode.Simple;

    [Header("Configuração")]
    public bool breakOnThrow = true;

    [Tooltip("Tempo antes de destruir o objeto no modo simples.")]
    public float destroyDelay = 0f;

    [Header("Animação")]
    [Tooltip("Animator do objeto que será usado para a animação.")]
    public Animator breakAnimator;

    [Tooltip("Nome do Trigger da animação de quebra.")]
    public string breakTrigger = "Break";

    [Tooltip("Tempo para destruir o objeto depois da animação.")]
    public float animationDestroyDelay = 5f;

    private bool wasThrown = false;
    private bool hasBroken = false;

    // Chamado quando o objeto é arremessado.
    public void EnableBreakOnThrow()
    {
        if (!breakOnThrow)
            return;

        wasThrown = true;
        hasBroken = false;
    }

    // Chamado quando o objeto é solto com E.
    public void DisableBreakOnThrow()
    {
        wasThrown = false;
        hasBroken = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Só quebra se tiver sido arremessado.
        if (!wasThrown)
            return;

        // Evita executar a quebra várias vezes.
        if (hasBroken)
            return;

        hasBroken = true;

        Break();
    }

    private void Break()
    {
        // =====================================
        // MODO SIMPLES
        // =====================================

        if (breakMode == BreakMode.Simple)
        {
            Destroy(gameObject, destroyDelay);

            return;
        }

        // =====================================
        // MODO ANIMAÇÃO
        // =====================================

        if (breakMode == BreakMode.Animation)
        {
            if (breakAnimator == null)
            {
                Debug.LogWarning(
                    "BreakableObject: Nenhum Animator foi configurado. " +
                    "O objeto será destruído normalmente.",
                    gameObject
                );

                Destroy(gameObject, destroyDelay);

                return;
            }

            // Para a física.
            Rigidbody rb = GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Toca a animação.
            breakAnimator.SetTrigger(breakTrigger);

            // Destrói depois do tempo configurado.
            Destroy(
                gameObject,
                animationDestroyDelay
            );
        }
    }
}