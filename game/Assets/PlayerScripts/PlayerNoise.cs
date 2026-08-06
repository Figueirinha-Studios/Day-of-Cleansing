using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    [Header("Noise Radius")]
    public float walkNoise = 5f;
    public float runNoise = 15f;
    public float jumpNoise = 25f;

    public float currentNoise;

    public void SetMovementNoise(bool moving, bool running)
    {
        if (!moving)
        {
            currentNoise = 0;
            return;
        }

        if (running)
        {
            currentNoise = runNoise;
        }
        else
        {
            currentNoise = walkNoise;
        }
    }

    public void MakeJumpNoise()
    {
        currentNoise = jumpNoise;
    }
}