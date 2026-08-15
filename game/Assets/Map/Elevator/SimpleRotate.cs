using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 50f, 0f);

    void Update()
    {
        // Gira o objeto continuamente a cada quadro
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
