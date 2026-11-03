
using System.Collections;
using UnityEngine;

public class TrashCanEvent : MonoBehaviour
{
    [Header("Referências")]
    public Rigidbody trashCanRigidbody;
    public AudioSource audioSource;
    public AudioClip fallSound;

    [Header("Queda")]
    [Range(0.2f, 2f)]
    public float fallDuration = 1.1f;

    [Range(45f, 90f)]
    public float fallAngle = 90f;

    [Tooltip("Eixo local em que a lixeira vai tombar.")]
    public Vector3 localFallAxis = Vector3.right;

    [Header("Ajuste do chão")]
    [Tooltip("Pequeno espaço entre a lixeira e o chão.")]
    public float groundOffset = 0.03f;

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

        if (trashCanRigidbody == null)
        {
            Debug.LogWarning(
                "TrashCanEvent: Rigidbody não configurado.", this
            );
            return;
        }

        eventTriggered = true;

        StartCoroutine(FallTrashCan());
    }

    private IEnumerator FallTrashCan()
    {
        Rigidbody rb = trashCanRigidbody;
        Transform objectTransform = rb.transform;

        // Desativa a física durante a queda controlada.
        rb.isKinematic = true;
        rb.useGravity = false;

        Vector3 originalPosition = objectTransform.position;
        Quaternion originalRotation = objectTransform.rotation;

        // Descobre a posição da base do objeto.
        Collider objectCollider = rb.GetComponent<Collider>();

        Vector3 pivot = originalPosition;

        if (objectCollider != null)
        {
            Bounds bounds = objectCollider.bounds;

            pivot = new Vector3(
                bounds.center.x,
                bounds.min.y,
                bounds.center.z
            );
        }

        Vector3 axis = objectTransform.TransformDirection(
            localFallAxis.normalized
        ).normalized;

        float elapsed = 0f;

        // Faz a lixeira tombar lentamente.
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / fallDuration);

            // Começa devagar e acelera durante a queda.
            float smoothProgress = progress * progress;

            float angle = fallAngle * smoothProgress;

            Quaternion rotation = Quaternion.AngleAxis(
                angle,
                axis
            ) * originalRotation;

            objectTransform.rotation = rotation;

            // Mantém a base no ponto de apoio.
            Vector3 rotatedPivotOffset =
                rotation * Quaternion.Inverse(originalRotation) *
                (pivot - originalPosition);

            objectTransform.position =
                pivot - rotatedPivotOffset;

            yield return null;
        }

        // Garante a rotação final.
        objectTransform.rotation =
            Quaternion.AngleAxis(fallAngle, axis) * originalRotation;

        Vector3 finalPivotOffset =
            objectTransform.rotation *
            Quaternion.Inverse(originalRotation) *
            (pivot - originalPosition);

        objectTransform.position =
            pivot - finalPivotOffset;

        // Pequeno ajuste para evitar que o Collider fique enterrado.
        objectTransform.position += Vector3.up * groundOffset;

        // Finaliza a queda.
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reproduz o som.
        if (audioSource != null && fallSound != null)
        {
            audioSource.PlayOneShot(fallSound);
        }

        // Avisa o Chappie.
        NoiseSystem.EmitNoise(rb.position, noiseRadius);
    }
}
