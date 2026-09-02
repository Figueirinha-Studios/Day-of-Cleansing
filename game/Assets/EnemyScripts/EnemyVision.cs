using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("References")]
    public Transform visionPoint;
    public Transform player;

    [Header("Vision")]
    public float viewDistance = 15f;

    [Range(0, 360)]
    public float viewAngle = 90f;

    [Header("Tolerância de Visão")]
    public float visionGraceTime = 0.6f;

    private float visionLostTimer = 0f;

    [Header("Layers")]
    public LayerMask obstacleMask;

    public bool CanSeePlayer()
    {
        if (player == null ||
            visionPoint == null)
        {
            return false;
        }

        Vector3 direction =
            player.position -
            visionPoint.position;

        // Distância
        if (direction.magnitude >
            viewDistance)
        {
            visionLostTimer +=
                Time.deltaTime;

            return false;
        }

        // Ângulo
        float angle =
            Vector3.Angle(
                visionPoint.forward,
                direction
            );

        if (angle >
            viewAngle / 2f)
        {
            visionLostTimer +=
                Time.deltaTime;

            return false;
        }

        // Parede / obstáculo
        if (
            Physics.Raycast(
                visionPoint.position,
                direction.normalized,
                out RaycastHit hit,
                viewDistance,
                ~0,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            // Aceita o próprio Player
            // ou qualquer collider filho dele.
            if (
                hit.transform == player ||
                hit.transform.IsChildOf(player)
            )
            {
                // Viu novamente.
                // Zera imediatamente o timer.
                visionLostTimer = 0f;

                return true;
            }
        }

        // Não conseguiu enxergar.
        visionLostTimer +=
            Time.deltaTime;

        return false;
    }

    public bool IsVisionStillActive()
    {
        return visionLostTimer <=
               visionGraceTime;
    }

    public void ResetVision()
    {
        visionLostTimer = 0f;
    }

    void OnDrawGizmosSelected()
    {
        if (visionPoint == null)
            return;

        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            visionPoint.position,
            viewDistance
        );

        Vector3 left =
            Quaternion.Euler(
                0,
                -viewAngle / 2f,
                0
            ) *
            visionPoint.forward;

        Vector3 right =
            Quaternion.Euler(
                0,
                viewAngle / 2f,
                0
            ) *
            visionPoint.forward;

        Gizmos.DrawRay(
            visionPoint.position,
            left * viewDistance
        );

        Gizmos.DrawRay(
            visionPoint.position,
            right * viewDistance
        );
    }
}