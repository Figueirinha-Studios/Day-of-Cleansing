using UnityEngine;

public class RemoteNoisePoint : MonoBehaviour
{
    [Header("Remote Event")]
    public string eventName;

    [Header("Virtual Noise")]
    public float noiseRadius = 30f;

    public void EmitNoise()
    {
        NoiseSystem.EmitNoise(
            transform.position,
            noiseRadius
        );

        Debug.Log("SOM VIRTUAL EMITIDO: " + eventName);
    }
}