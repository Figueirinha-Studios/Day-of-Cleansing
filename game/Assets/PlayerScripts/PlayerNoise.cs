using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    [Header("Noise Radius")]
    public float walkNoise = 5f;
    public float runNoise = 15f;
    public float jumpNoise = 25f;

    [Header("Surface Detection")]
    public float surfaceCheckDistance = 2f;
    public float dirtNoiseMultiplier = 0.5f;

    public float currentNoise;

    private float GetSurfaceMultiplier()
    {
        RaycastHit hit;

        if (Physics.Raycast(
            transform.position,
            Vector3.down,
            out hit,
            surfaceCheckDistance))
        {
            if (hit.collider.CompareTag("Terra"))
            {
                return dirtNoiseMultiplier;
            }
        }

        // Se não for Terra, considera concreto
        return 1f;
    }

    public void SetMovementNoise(bool moving, bool running, bool crouching)
    {
        if (crouching)
        {
            currentNoise = 0;
            return;
        }

        if (!moving)
        {
            currentNoise = 0;
            return;
        }

        float surfaceMultiplier = GetSurfaceMultiplier();

        if (running)
        {
            currentNoise = runNoise * surfaceMultiplier;
        }
        else
        {
            currentNoise = walkNoise * surfaceMultiplier;
        }
    }

    public void MakeJumpNoise()
    {
        float surfaceMultiplier = GetSurfaceMultiplier();

        currentNoise = jumpNoise * surfaceMultiplier;
    }
}