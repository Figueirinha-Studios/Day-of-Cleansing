
using UnityEngine;

public class ItemFallRespawn : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Transform initialParent;
    private Rigidbody rb;

    private void Awake()
    {
        // Salvar a posição inicial do item.
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialParent = transform.parent;

        rb = GetComponent<Rigidbody>();
    }

    public void ReturnToStart()
    {
        // Remover o item de qualquer posição de segurá-lo.
        transform.SetParent(initialParent);

        // Retornar à posição e rotação iniciais.
        transform.SetPositionAndRotation(
            initialPosition,
            initialRotation
        );

        // Reiniciar a física do objeto.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
