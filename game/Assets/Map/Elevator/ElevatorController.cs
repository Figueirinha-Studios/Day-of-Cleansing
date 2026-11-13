
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

    [Header("Emergency Door")]
    public GameObject forceOpen;
    public float forceOpenDelay = 2f;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.down * distance;

        StartElevator();
    }

    public void StartElevator()
    {
        if (isMoving)
            return;

        isMoving = true;

 

        StartCoroutine(MoveElevator());
    }

    private IEnumerator MoveElevator()
    {
        // Tempo de espera antes de iniciar o movimento
        yield return new WaitForSeconds(25f);

        if (movementAudio != null)
            movementAudio.Play();

        float totalDistance = Vector3.Distance(startPosition, targetPosition);

        float accelerationDistance = (maxSpeed * accelerationTime) / 2f;
        float decelerationDistance = (maxSpeed * decelerationTime) / 2f;

        // Ajusta a velocidade caso a distância seja pequena
        if (accelerationDistance + decelerationDistance > totalDistance)
        {
            float factor = totalDistance /
                (accelerationDistance + decelerationDistance);

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

        // Garante a posição final
        transform.position = targetPosition;

        isMoving = false;

        // Áudio de movimento
        if (movementAudio != null)
            movementAudio.Stop();

        // Áudio de chegada
        if (arrivalAudio != null)
            arrivalAudio.Play();

        Debug.Log("ELEVADOR CHEGOU!");

        // Verifica o controlador das portas
        if (doorController == null)
        {
            Debug.LogError(
                "ElevatorController: doorController não foi atribuído no Inspector!"
            );

            yield break;
        }

        // Abre as portas
        Debug.Log("Solicitando abertura das portas...");

        doorController.OpenDoors();

        Debug.Log("OpenDoors() foi chamado!");

        // Verifica se as portas realmente abriram
        StartCoroutine(CheckIfDoorsOpened());
    }

    private IEnumerator CheckIfDoorsOpened()
    {
        yield return new WaitForSeconds(forceOpenDelay);

        if (doorController == null)
        {
            Debug.LogError(
                "ElevatorController: doorController ficou nulo durante a verificação!"
            );

            yield break;
        }

        if (!doorController.AreDoorsOpen())
        {
            Debug.LogWarning(
                "ELEVADOR EMPERROU! Ativando abertura emergencial."
            );

            if (forceOpen != null)
            {
                forceOpen.SetActive(true);
            }
            else
            {
                Debug.LogError(
                    "ElevatorController: forceOpen não foi atribuído no Inspector!"
                );
            }
        }
        else
        {
            Debug.Log("Portas abertas com sucesso!");
        }
    }
}
