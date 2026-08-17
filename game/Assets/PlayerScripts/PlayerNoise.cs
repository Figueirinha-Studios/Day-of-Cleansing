using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    [Header("Noise Radius")]
    public float walkNoise = 10f;
    public float runNoise = 30f;
    public float jumpNoise = 50f;

    [Header("Noise Interval")]
    public float walkNoiseInterval = 0.5f;
    public float runNoiseInterval = 0.25f;

    [Header("Surface Detection")]
    public float surfaceCheckDistance = 2f;
    public float dirtNoiseMultiplier = 0.5f;

    public float currentNoise;

    private float noiseTimer;

    private bool isMoving;
    private bool isRunning;
    private bool isCrouching;

    void Update()
    {
        if (!isMoving || isCrouching)
        {
            currentNoise = 0f;
            noiseTimer = 0f;
            return;
        }

        float surfaceMultiplier = GetSurfaceMultiplier();

        if (isRunning)
        {
            currentNoise = runNoise * surfaceMultiplier;
        }
        else
        {
            currentNoise = walkNoise * surfaceMultiplier;
        }

        noiseTimer -= Time.deltaTime;

        if (noiseTimer <= 0f)
        {
            NoiseSystem.EmitNoise(
                transform.position,
                currentNoise
            );

            noiseTimer = isRunning
                ? runNoiseInterval
                : walkNoiseInterval;
        }
    }

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

        return 1f;
    }

    public void SetMovementNoise(bool moving, bool running, bool crouching)
    {
        isMoving = moving;
        isRunning = running;
        isCrouching = crouching;
    }

    public void MakeJumpNoise()
    {
        float surfaceMultiplier = GetSurfaceMultiplier();

        currentNoise = jumpNoise * surfaceMultiplier;

        NoiseSystem.EmitNoise(
            transform.position,
            currentNoise
        );
    }
}