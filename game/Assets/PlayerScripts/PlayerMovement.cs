using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float joggingSpeed = 3f;

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

        // Ignora inclinação da câmera
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * vertical + right * horizontal;

        bool isMoving = move.magnitude > 0.1f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving;

        // Informa o barulho para o sistema do robô
        noise.SetMovementNoise(isMoving, isRunning);

        // Informa o movimento para o sistema de passos
        footsteps.UpdateFootsteps(isMoving, isRunning);

        // Velocidade
        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            Debug.Log("correndo");
            currentSpeed = runSpeed;
        }

        if (Input.GetKey(KeyCode.LeftShift) &&
            (Input.GetKey(KeyCode.A) ||
             Input.GetKey(KeyCode.S) ||
             Input.GetKey(KeyCode.D)))
        {
            Debug.Log("trotando");
            currentSpeed = joggingSpeed;
        }

        Vector3 targetVelocity = move * currentSpeed;

        float smoothRate = move.magnitude > 0.1f
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
                if (Input.GetButtonDown("Jump") && isMoving)
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
