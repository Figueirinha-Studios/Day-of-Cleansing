using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    public Transform player;

    [Header("Hearing")]
    public float hearingMultiplier = 1f;

    public bool HeardPlayer(float noiseRadius)
    {
        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        return distance <= noiseRadius * hearingMultiplier;
    }
}