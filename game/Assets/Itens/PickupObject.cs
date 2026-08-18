using UnityEngine;

public class PickupObject : MonoBehaviour
{
    [Header("Configurações")]
    public float throwMultiplier = 1f;

    [HideInInspector]
    public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
}