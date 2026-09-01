using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("References")]
    public CharacterController playerController;

    public CameraController cameraController;


    // =========================================================
    // FIRST PERSON
    // =========================================================

    [Header("First Person")]
    public float firstPersonDistance = 0.05f;


    // =========================================================
    // MOVEMENT TILT
    // =========================================================

    [Header("Movement Tilt")]
    public float tiltAmount = 8f;

    public float tiltSpeed = 8f;


    // =========================================================
    // MOUSE SWAY
    // =========================================================

    [Header("Mouse Sway")]
    public float mouseSwayAmount = 2f;

    public float mouseSwaySpeed = 8f;


    // =========================================================
    // JUMP SWAY
    // =========================================================

    [Header("Jump Sway")]
    public float jumpPush = 0.15f;

    public float jumpSmooth = 8f;


    // =========================================================
    // RUN CAMERA SWAY
    // =========================================================

    [Header("Run Camera Sway")]
    public float runSwayAmount = 2f;

    public float runSwaySpeed = 8f;


    // =========================================================
    // CROUCH
    // =========================================================

    [Header("Crouch")]
    public float crouchCameraHeight = -0.5f;

    public float crouchSmooth = 8f;


    // =========================================================
    // PLAYER MOVEMENT
    // =========================================================

    [Header("Player Movement")]
    public PlayerMovement playerMovement;


    // =========================================================
    // VARIÁVEIS INTERNAS
    // =========================================================

    private Quaternion targetRotation;

    private Vector3 targetPosition;


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // =====================================================
        // PRIMEIRA PESSOA
        // =====================================================

        if (!cameraController.IsFirstPerson())
        {
            ResetEffects();
            return;
        }


        // =====================================================
        // RESET DOS ALVOS
        // =====================================================

        targetRotation =
            Quaternion.identity;

        targetPosition =
            Vector3.zero;


        // =====================================================
        // EFEITOS
        // =====================================================

        MovementTilt();

        MouseSway();

        RunSway();

        JumpSway();

        Crouch();


        // =====================================================
        // APLICA ROTAÇÃO
        // =====================================================

        transform.localRotation =
            Quaternion.Lerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * tiltSpeed
            );


        // =====================================================
        // APLICA POSIÇÃO
        // =====================================================

        transform.localPosition =
            Vector3.Lerp(
                transform.localPosition,
                targetPosition,
                Time.deltaTime * 8f
            );
    }


    // =========================================================
    // BALANÇO DE CORRIDA
    // =========================================================

    void RunSway()
    {
        if (playerMovement == null)
            return;


        // Só balança se o PlayerMovement
        // disser que está realmente correndo.

        if (!playerMovement.isRunning)
            return;


        float time =
            Time.time *
            runSwaySpeed;


        float vertical =
            Mathf.Sin(time) *
            runSwayAmount;


        float horizontal =
            Mathf.Cos(time * 0.5f) *
            runSwayAmount;


        targetRotation *=
            Quaternion.Euler(
                vertical,
                horizontal,
                0
            );
    }


    // =========================================================
    // MOVEMENT TILT
    // =========================================================

    void MovementTilt()
    {
        float horizontal =
            Input.GetAxis("Horizontal");


        targetRotation *=
            Quaternion.Euler(
                0,
                0,
                -horizontal *
                tiltAmount
            );
    }


    // =========================================================
    // MOUSE SWAY
    // =========================================================

    void MouseSway()
    {
        float mouseX =
            Input.GetAxis("Mouse X");


        float mouseY =
            Input.GetAxis("Mouse Y");


        targetRotation *=
            Quaternion.Euler(
                -mouseY *
                mouseSwayAmount,

                mouseX *
                mouseSwayAmount,

                0
            );
    }


    // =========================================================
    // JUMP SWAY
    // =========================================================

    void JumpSway()
    {
        if (!playerController)
            return;


        if (!playerController.isGrounded)
        {
            targetPosition.z =
                -jumpPush;
        }
    }


    // =========================================================
    // CROUCH
    // =========================================================

    void Crouch()
    {
        if (
            playerMovement != null &&
            playerMovement.isCrouching
        )
        {
            targetPosition.y =
                crouchCameraHeight;
        }
    }


    // =========================================================
    // RESET DOS EFEITOS
    // =========================================================

    void ResetEffects()
    {
        transform.localRotation =
            Quaternion.Lerp(
                transform.localRotation,
                Quaternion.identity,
                Time.deltaTime * 8f
            );


        transform.localPosition =
            Vector3.Lerp(
                transform.localPosition,
                Vector3.zero,
                Time.deltaTime * 8f
            );
    }
}