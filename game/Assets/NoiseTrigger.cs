using UnityEngine;

public class NoiseTrigger : MonoBehaviour
{
    [Header("Noise")]
    public float noiseRadius = 50f;
    public KeyCode activationKey = KeyCode.E;

    private bool playerInside = false;

    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            Debug.Log("E FOI APERTADO");

            if (playerInside)
            {
                Debug.Log("PLAYER ESTA DENTRO!");

                NoiseSystem.EmitNoise(
                    transform.position,
                    noiseRadius
                );

                Debug.Log("SOM EMITIDO!");
            }
            else
            {
                Debug.Log("PLAYER NAO ESTA DENTRO DA HITBOX");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Algo entrou na hitbox: " + other.name);

        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("PLAYER ENTROU!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Algo saiu da hitbox: " + other.name);

        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("PLAYER SAIU!");
        }
    }
}