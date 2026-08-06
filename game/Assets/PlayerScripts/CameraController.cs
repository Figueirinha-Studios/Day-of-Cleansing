using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Referências")]
    public Transform cameraPivot;
    public Camera playerCamera;

    [Header("Rotação")]
    public float mouseSensitivity = 3f;

    [Header("Zoom")]
    public bool allowThirdPerson = true;
    public float minDistance = 0f;
    public float maxDistance = 8f;
    public float zoomSpeed = 8f;
    public float zoomSmoothness = 10f;

    public float GetDistance()
    {
        return currentDistance;
    }
    public bool IsFirstPerson()
    {
        return currentDistance <= 0.05f;
    }

    float yaw;
    float pitch;

    float targetDistance = 4f;
    float currentDistance = 4f;

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = 15f;
    }

    void Update()
    {
        HandleMouse();
        HandleZoom();
        UpdateCamera();
    }

    void HandleMouse()
    {
        bool firstPerson = currentDistance <= 0.05f;

        if (firstPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (Input.GetMouseButton(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }
        }

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(0, yaw, 0);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    void HandleZoom()
    {
        if (!allowThirdPerson)
        {
            targetDistance = 0f;
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        targetDistance -= scroll * zoomSpeed;

        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        currentDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            Time.deltaTime * zoomSmoothness);
    }

    void UpdateCamera()
    {
        playerCamera.transform.localPosition =
            new Vector3(0, 0, -currentDistance);

        playerCamera.transform.localRotation = Quaternion.identity;
    }
}