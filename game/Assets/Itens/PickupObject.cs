using UnityEngine;

public class PickupObject : MonoBehaviour
{
    [Header("Configurações")]
    public float throwMultiplier = 1f;


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


    // =========================================================
    // VERIFICAR SE É ITEM DO GERADOR
    // =========================================================

    public bool IsGeneratorItem()
    {
        return CompareTag("Gasolina") ||
               CompareTag("Fusivel");
    }


    // =========================================================
    // VERIFICAR GASOLINA
    // =========================================================

    public bool IsGasoline()
    {
        return CompareTag("Gasolina");
    }


    // =========================================================
    // VERIFICAR FUSÍVEL
    // =========================================================

    public bool IsFuse()
    {
        return CompareTag("Fusivel");
    }
}