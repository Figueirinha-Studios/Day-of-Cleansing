using UnityEngine;

public static class NoiseSystem
{
    public static void EmitNoise(Vector3 position, float radius)
    {
        EnemyHearing[] enemies = Object.FindObjectsByType<EnemyHearing>(
            FindObjectsSortMode.None
        );

        foreach (EnemyHearing enemy in enemies)
        {
            if (enemy.CanHearNoise(position, radius))
            {
                enemy.HearNoise(position);
            }
        }
    }
}