
using UnityEngine;

public class SodaCanEvent : MonoBehaviour
{
    [Header("Referências")]
    public Rigidbody canRigidbody;
    public AudioSource audioSource;
    public AudioClip rollSound;

    [Header("Movimento")]
    [Tooltip("Direção horizontal em que a latinha vai rolar.")]
    public Vector3 rollDirection = Vector3.forward;

    [Min(0f)]
    [Tooltip("Velocidade inicial da latinha.")]
    public float rollSpeed = 3f;

    [Min(0f)]
    [Tooltip("Velocidade inicial de rotação da latinha.")]
    public float rollSpin = 12f;

    [Header("Som e alcance")]
    [Min(0f)]
    public float noiseRadius = 200f;

    [Header("Estado")]
    [SerializeField]
    private bool eventTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (eventTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (canRigidbody == null)
        {
            Debug.LogWarning(
                "SodaCanEvent: Rigidbody da latinha não configurado.",
                this
            );
            return;
        }

        eventTriggered = true;

        StartRolling();
    }

    private void StartRolling()
    {
        Rigidbody rb = canRigidbody;

        // Ativa a física da latinha.
        rb.isKinematic = false;
        rb.useGravity = true;

        // Limpa qualquer movimento anterior.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Mantém a direção horizontal.
        Vector3 direction = rollDirection;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.forward;
            direction.y = 0f;
        }

        direction.Normalize();

        // Dá velocidade para a latinha seguir pelo chão.
        rb.linearVelocity = direction * rollSpeed;

        // Aplica rotação no eixo perpendicular à direção do movimento.
        Vector3 spinAxis = Vector3.Cross(Vector3.up, direction).normalized;
        rb.angularVelocity = spinAxis * rollSpin;

        // Toca o som da latinha rolando.
        if (audioSource != null && rollSound != null)
        {
            audioSource.PlayOneShot(rollSound);
        }

        // Avisa o Chappie sobre o barulho.
        NoiseSystem.EmitNoise(rb.position, noiseRadius);
    }
}
