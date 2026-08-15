using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float joggingSpeed = 3f;
    public float crouchSpeed = 1f;

    [Header("Agachamento")]
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Smooth Movement")]
    public float acceleration = 15f;
    public float deceleration = 20f;

    [Header("Pulo e Gravidade")]
    public float jumpHeight = 0.1f;
    public float gravity = 280f;

    [Header("Câmera")]
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;

    private PlayerNoise noise;
    private PlayerFootsteps footsteps;

    public bool isCrouching { get; private set; }

    // Controle do pouso
    private bool wasGrounded;
    private bool hasJumped;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        noise = GetComponent<PlayerNoise>();
        footsteps = GetComponent<PlayerFootsteps>();

        wasGrounded = controller.isGrounded;
    }

    void Update()
    {
        // Estado do chão ANTES do movimento deste frame
        bool isGrounded = controller.isGrounded;

        MovePlayer(isGrounded);
        HandleJump(isGrounded);
        ApplyGravity();

        // Verificamos o estado DEPOIS dos movimentos
        bool nowGrounded = controller.isGrounded;

        // Acabou de aterrissar
        if (!wasGrounded && nowGrounded)
        {
            // Só toca queda se o jogador realmente pulou
            if (hasJumped)
            {
                footsteps.PlayLandingSound();
                hasJumped = false;
            }
        }

        wasGrounded = nowGrounded;
    }

    void MovePlayer(bool isGrounded)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Direção baseada na câmera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * vertical + right * horizontal;

        bool isMoving = move.magnitude > 0.1f;

        // Agachamento
        isCrouching = Input.GetKey(crouchKey);

        // Corrida
        bool isRunning =
            Input.GetKey(KeyCode.LeftShift) &&
            isMoving &&
            !isCrouching;

        // =========================
        // NOISE
        // =========================

        if (isGrounded)
        {
            noise.SetMovementNoise(
                isMoving,
                isRunning,
                isCrouching
            );
        }
        else
        {
            // No ar = nenhum barulho de movimento
            noise.SetMovementNoise(
                false,
                false,
                isCrouching
            );
        }

        // =========================
        // PASSOS
        // =========================

        // O PlayerFootsteps já sabe lidar
        // com crouching e movimento.
        footsteps.UpdateFootsteps(
            isMoving,
            isRunning,
            isCrouching,
            isGrounded
        );

        // =========================
        // VELOCIDADE
        // =========================

        float currentSpeed;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isRunning && Input.GetKey(KeyCode.W))
        {
            currentSpeed = runSpeed;
        }
        else if (isRunning &&
            (Input.GetKey(KeyCode.A) ||
             Input.GetKey(KeyCode.S) ||
             Input.GetKey(KeyCode.D)))
        {
            currentSpeed = joggingSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        Vector3 targetVelocity = move * currentSpeed;

        float smoothRate = isMoving
            ? acceleration
            : deceleration;

        currentMoveVelocity = Vector3.Lerp(
            currentMoveVelocity,
            targetVelocity,
            smoothRate * Time.deltaTime
        );

        controller.Move(currentMoveVelocity * Time.deltaTime);
    }

    void HandleJump(bool isGrounded)
    {
        // Só pode pular no chão
        if (!isGrounded)
            return;

        // Não pode pular agachado
        if (isCrouching)
            return;

        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );

            // Barulho do pulo
            noise.MakeJumpNoise();

            // Som do pulo
            footsteps.PlayJumpSound();

            // Marca que este voo começou com um pulo
            hasJumped = true;
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}