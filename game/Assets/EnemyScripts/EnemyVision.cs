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

    [Header("Layers")]
    public LayerMask obstacleMask;

    public bool CanSeePlayer()
    {
        Vector3 direction = player.position - visionPoint.position;

        // Distância
        if (direction.magnitude > viewDistance)
            return false;

        // Ângulo
        float angle = Vector3.Angle(visionPoint.forward, direction);

        if (angle > viewAngle / 2f)
            return false;

        // Parede?
        if (Physics.Raycast(
            visionPoint.position,
            direction.normalized,
            out RaycastHit hit,
            viewDistance))
        {
            if (hit.transform == player)
                return true;
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (visionPoint == null)
            return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            visionPoint.position,
            viewDistance
        );

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * visionPoint.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * visionPoint.forward;

        Gizmos.DrawRay(visionPoint.position, left * viewDistance);
        Gizmos.DrawRay(visionPoint.position, right * viewDistance);
    }
    void Update()
    {
        if (CanSeePlayer())
        {
            Debug.Log("PLAYER IN VISION");
        }
    }
}