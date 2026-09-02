using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    [Header("Hearing")]
    public float hearingMultiplier = 1f;

    private EnemyAI enemyAI;

    void Awake()
    {
        enemyAI =
            GetComponent<EnemyAI>();
    }

    public bool CanHearNoise(
        Vector3 noisePosition,
        float noiseRadius
    )
    {
        float distance =
            Vector3.Distance(
                transform.position,
                noisePosition
            );

        return distance <=
               noiseRadius *
               hearingMultiplier;
    }

    public void HearNoise(
        Vector3 noisePosition
    )
    {
        if (enemyAI == null)
            return;

        enemyAI.ReceiveNoise(
            noisePosition
        );
    }
}