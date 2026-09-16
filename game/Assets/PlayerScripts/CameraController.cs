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
    public float targetDistance = 0f;
    public float currentDistance = 0f;

    private float yaw;
    private float pitch;

    // =========================================================
    // STEALTH
    // =========================================================

    private bool automaticLook = false;

    // =========================================================
    // DISTÂNCIA
    // =========================================================

    public float GetDistance()
    {
        return currentDistance;
    }

    public bool IsFirstPerson()
    {
        return currentDistance <= 0.05f;
    }

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        yaw =
            transform.eulerAngles.y;

        pitch = 15f;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (!automaticLook)
        {
            HandleMouse();
        }

        HandleZoom();
        UpdateCamera();
    }

    // =========================================================
    // MOUSE
    // =========================================================

    void HandleMouse()
    {
        bool firstPerson =
            currentDistance <= 0.05f;

        if (firstPerson)
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible =
                false;
        }
        else
        {
            if (Input.GetMouseButton(1))
            {
                Cursor.lockState =
                    CursorLockMode.Locked;

                Cursor.visible =
                    false;
            }
            else
            {
                Cursor.lockState =
                    CursorLockMode.None;

                Cursor.visible =
                    true;

                return;
            }
        }

        yaw +=
            Input.GetAxis("Mouse X") *
            mouseSensitivity;

        pitch -=
            Input.GetAxis("Mouse Y") *
            mouseSensitivity;

        pitch =
            Mathf.Clamp(
                pitch,
                -80f,
                80f
            );

        transform.rotation =
            Quaternion.Euler(
                0,
                yaw,
                0
            );

        cameraPivot.localRotation =
            Quaternion.Euler(
                pitch,
                0,
                0
            );
    }

    // =========================================================
    // ZOOM
    // =========================================================

    void HandleZoom()
    {
        if (!allowThirdPerson)
        {
            targetDistance = 0f;
            return;
        }

        float scroll =
            Input.GetAxis(
                "Mouse ScrollWheel"
            );

        targetDistance -=
            scroll * zoomSpeed;

        targetDistance =
            Mathf.Clamp(
                targetDistance,
                minDistance,
                maxDistance
            );

        currentDistance =
            Mathf.Lerp(
                currentDistance,
                targetDistance,
                Time.deltaTime *
                zoomSmoothness
            );
    }

    // =========================================================
    // CÂMERA
    // =========================================================

    void UpdateCamera()
    {
        playerCamera.transform.localPosition =
            new Vector3(
                0,
                0,
                -currentDistance
            );

        playerCamera.transform.localRotation =
            Quaternion.identity;
    }

    // =========================================================
    // STEALTH - INICIAR CONTROLE
    // =========================================================

    public void BeginAutomaticLook()
    {
        automaticLook = true;

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible =
            false;
    }

    // =========================================================
    // STEALTH - OLHAR PARA ALVO
    // =========================================================

    public bool RotateAutomaticallyTowards(
        Vector3 targetPosition,
        float rotationSpeed
    )
    {
        if (!automaticLook)
            return true;

        Vector3 direction =
            targetPosition -
            transform.position;

        if (direction.sqrMagnitude <=
            0.001f)
        {
            return true;
        }

        // -----------------------------------------------------
        // ROTAÇÃO HORIZONTAL
        // -----------------------------------------------------

        Vector3 horizontalDirection =
            new Vector3(
                direction.x,
                0f,
                direction.z
            );

        if (horizontalDirection.sqrMagnitude >
            0.001f)
        {
            Quaternion targetYaw =
                Quaternion.LookRotation(
                    horizontalDirection
                );

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetYaw,
                    rotationSpeed *
                    Time.deltaTime
                );

            yaw =
                transform.eulerAngles.y;
        }

        // -----------------------------------------------------
        // ROTAÇÃO VERTICAL
        // -----------------------------------------------------

        Vector3 localDirection =
            transform.InverseTransformDirection(
                direction
            );

        float targetPitch =
            -Mathf.Atan2(
                localDirection.y,
                new Vector2(
                    localDirection.x,
                    localDirection.z
                ).magnitude
            ) *
            Mathf.Rad2Deg;

        targetPitch =
            Mathf.Clamp(
                targetPitch,
                -80f,
                80f
            );

        pitch =
            Mathf.MoveTowards(
                pitch,
                targetPitch,
                rotationSpeed *
                Time.deltaTime
            );

        cameraPivot.localRotation =
            Quaternion.Euler(
                pitch,
                0f,
                0f
            );

        return
            Mathf.Abs(
                Mathf.DeltaAngle(
                    transform.eulerAngles.y,
                    targetYawAngle(
                        horizontalDirection
                    )
                )
            ) < 1f &&
            Mathf.Abs(
                pitch - targetPitch
            ) < 1f;
    }

    private float targetYawAngle(
        Vector3 direction
    )
    {
        if (direction.sqrMagnitude <=
            0.001f)
        {
            return transform.eulerAngles.y;
        }

        return Quaternion.LookRotation(
            direction
        ).eulerAngles.y;
    }

    // =========================================================
    // STEALTH - DEVOLVER CONTROLE
    // =========================================================

    public void EndAutomaticLook()
    {
        yaw =
            transform.eulerAngles.y;

        pitch =
            cameraPivot.localEulerAngles.x;

        if (pitch > 180f)
        {
            pitch -= 360f;
        }

        pitch =
            Mathf.Clamp(
                pitch,
                -80f,
                80f
            );

        automaticLook = false;

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible =
            false;
    }
}