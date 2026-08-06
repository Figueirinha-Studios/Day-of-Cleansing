using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("References")]
    public CharacterController playerController;
    public CameraController cameraController;

    [Header("First Person")]
    public float firstPersonDistance = 0.05f;

    [Header("Movement Tilt")]
    public float tiltAmount = 8f;
    public float tiltSpeed = 8f;


    [Header("Mouse Sway")]
    public float mouseSwayAmount = 2f;
    public float mouseSwaySpeed = 8f;


    [Header("Jump Sway")]
    public float jumpPush = 0.15f;
    public float jumpSmooth = 8f;

    [Header("Run Camera Sway")]
    public float runSwayAmount = 2f;
    public float runSwaySpeed = 8f;

    private Quaternion targetRotation;
    private Vector3 targetPosition;

    void RunSway()
    {
        if (!Input.GetKey(KeyCode.LeftShift) || !Input.GetKey(KeyCode.W))
            return;


        float time = Time.time * runSwaySpeed;


        float vertical =
            Mathf.Sin(time) * runSwayAmount;


        float horizontal =
            Mathf.Cos(time * 0.5f) * runSwayAmount;


        targetRotation *= Quaternion.Euler(
            vertical,
            horizontal,
            0
        );
    }
    void ResetEffects()
    {
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            Quaternion.identity,
            Time.deltaTime * 8f
        );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            Vector3.zero,
            Time.deltaTime * 8f
        );
    }

    void Update()
    {
        if (!cameraController.IsFirstPerson())
        {
            ResetEffects();
            return;
        }


        targetRotation = Quaternion.identity;
        targetPosition = Vector3.zero;


        MovementTilt();
        MouseSway();
        RunSway();


        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * tiltSpeed
        );


        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * 8f
        );
    }


    void MovementTilt()
    {
        float horizontal = Input.GetAxis("Horizontal");


        targetRotation *= Quaternion.Euler(
            0,
            0,
            -horizontal * tiltAmount
        );
    }


    void MouseSway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");


        targetRotation *= Quaternion.Euler(
            -mouseY * mouseSwayAmount,
            mouseX * mouseSwayAmount,
            0
        );
    }


    void JumpSway()
    {
        if (!playerController)
            return;


        if (!playerController.isGrounded)
        {
            targetPosition.z = -jumpPush;
        }
    }
}