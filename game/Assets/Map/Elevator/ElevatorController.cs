using UnityEngine;
using System.Collections;

public class ElevatorController : MonoBehaviour
{
    [Header("Movement")]
    public float distance = 20f;
    public float accelerationTime = 2f;
    public float maxSpeed = 5f;
    public float decelerationTime = 3f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private bool isMoving = false;

    [Header("Doors")]
    public ElevatorDoorController doorController;

    [Header("Audio")]
    public AudioSource movementAudio;
    public AudioSource arrivalAudio;
    public AudioSource doorAudio;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.down * distance;

        StartElevator();
    }

    public void StartElevator()
    {
        if (!isMoving)
        {
            movementAudio.Play();
            StartCoroutine(MoveElevator());
        }
    }

    private IEnumerator MoveElevator()
    {
        isMoving = true;

        float totalDistance = Vector3.Distance(startPosition, targetPosition);

        float accelerationDistance = (maxSpeed * accelerationTime) / 2f;
        float decelerationDistance = (maxSpeed * decelerationTime) / 2f;

        // Caso a distância seja pequena demais para atingir a velocidade máxima
        if (accelerationDistance + decelerationDistance > totalDistance)
        {
            float factor = totalDistance / (accelerationDistance + decelerationDistance);

            accelerationDistance *= factor;
            decelerationDistance *= factor;
        }

        float constantDistance =
            totalDistance - accelerationDistance - decelerationDistance;

        float traveledDistance = 0f;
        float currentSpeed = 0f;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            float deltaTime = Time.deltaTime;

            // ACELERAÇÃO
            if (traveledDistance < accelerationDistance)
            {
                currentSpeed = Mathf.MoveTowards(
                    currentSpeed,
                    maxSpeed,
                    maxSpeed / accelerationTime * deltaTime
                );
            }

            // DESACELERAÇÃO
            else if (traveledDistance > accelerationDistance + constantDistance)
            {
                currentSpeed = Mathf.MoveTowards(
                    currentSpeed,
                    0f,
                    maxSpeed / decelerationTime * deltaTime
                );
            }

            // VELOCIDADE CONSTANTE
            else
            {
                currentSpeed = maxSpeed;
            }

            float movement = currentSpeed * deltaTime;

            traveledDistance += movement;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                movement
            );

            yield return null;
        }

        // Força a posição final
        transform.position = targetPosition;
        isMoving = false;

        movementAudio.Stop();
        arrivalAudio.Play();

        Debug.Log("ELEVADOR CHEGOU!");
        doorController.OpenDoors();
    }
}