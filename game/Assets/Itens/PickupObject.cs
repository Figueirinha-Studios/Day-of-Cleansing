using UnityEngine;

public class PickupObject : MonoBehaviour
{
    [Header("Configurações")]
    public float throwMultiplier = 1f;

    [Header("Configurações de arremesso")]
    [Tooltip("Define se este objeto pode ser arremessado com o botão esquerdo.")]
    public bool canThrow = true;

    [Header("Posição na mão")]
    [Tooltip("Posição do objeto em relação ao HoldPoint.")]
    public Vector3 holdPosition;

    [Header("Rotação na mão")]
    [Tooltip("Rotação do objeto em relação ao HoldPoint.")]
    public Vector3 holdRotation;

    [HideInInspector]
    public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
}