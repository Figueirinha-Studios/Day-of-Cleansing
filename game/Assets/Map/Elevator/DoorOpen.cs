using UnityEngine;
using System.Collections;

public class ElevatorDoorController : MonoBehaviour
{
    [Header("Doors")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Movement")]
    public float openDistance = 1f;
    public float openTime = 1f;

    [Header("Audio")]
    public AudioSource doorAudio;

    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;

    private Vector3 leftOpenPosition;
    private Vector3 rightOpenPosition;

    private Coroutine doorCoroutine;

    private void Awake()
    {
        leftClosedPosition = leftDoor.localPosition;
        rightClosedPosition = rightDoor.localPosition;

        // Seu eixo correto é Z
        leftOpenPosition = leftClosedPosition + Vector3.forward * openDistance;
        rightOpenPosition = rightClosedPosition + Vector3.back * openDistance;
    }

    public void OpenDoors()
    {
        doorAudio.Play();
        StartDoorMovement(leftOpenPosition, rightOpenPosition);
    }

    public void CloseDoors()
    {
        StartDoorMovement(leftClosedPosition, rightClosedPosition);
    }

    private void StartDoorMovement(Vector3 leftTarget, Vector3 rightTarget)
    {
        if (doorCoroutine != null)
        {
            StopCoroutine(doorCoroutine);
        }

        doorCoroutine = StartCoroutine(
            MoveDoors(leftTarget, rightTarget)
        );
    }

    private IEnumerator MoveDoors(Vector3 leftTarget, Vector3 rightTarget)
    {
        Vector3 leftStart = leftDoor.localPosition;
        Vector3 rightStart = rightDoor.localPosition;

        float elapsed = 0f;

        while (elapsed < openTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / openTime);

            t = Mathf.SmoothStep(0f, 1f, t);

            leftDoor.localPosition = Vector3.Lerp(
                leftStart,
                leftTarget,
                t
            );

            rightDoor.localPosition = Vector3.Lerp(
                rightStart,
                rightTarget,
                t
            );

            yield return null;
        }

        leftDoor.localPosition = leftTarget;
        rightDoor.localPosition = rightTarget;

        doorCoroutine = null;
    }
}