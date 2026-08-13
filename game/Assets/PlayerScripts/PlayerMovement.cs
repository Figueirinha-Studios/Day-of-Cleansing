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
    public float groundCheckDelay = 0.1f;
    private float lastGroundedTime;

    [Header("Câmera")]
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;

    private PlayerNoise noise;
    private PlayerFootsteps footsteps;

    public bool isCrouching { get; private set; }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        noise = GetComponent<PlayerNoise>();
        footsteps = GetComponent<PlayerFootsteps>();
    }

    void Update()
    {
        MovePlayer();
        ApplyGravity();

        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    void MovePlayer()
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

        // Corrida só pode acontecer se NÃO estiver agachado
        bool isRunning =
            Input.GetKey(KeyCode.LeftShift) &&
            isMoving &&
            !isCrouching;

        // Sistemas de som
        noise.SetMovementNoise(isMoving, isRunning, isCrouching);
        footsteps.UpdateFootsteps(isMoving, isRunning, isCrouching);

        // Velocidade
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

        // Pulo
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (Time.time - lastGroundedTime <= groundCheckDelay)
            {
                if (Input.GetButtonDown("Jump") && isMoving && !isCrouching)
                {
                    velocity.y = Mathf.Sqrt(
                        jumpHeight * -2f * gravity
                    );

                    noise.MakeJumpNoise();
                }
            }
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}