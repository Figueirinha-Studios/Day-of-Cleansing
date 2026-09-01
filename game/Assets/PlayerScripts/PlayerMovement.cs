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


    [Header("Corrida")]
    public KeyCode runKey = KeyCode.LeftShift;


    // =========================================================
    // ESTAMINA
    // =========================================================

    [Header("Estamina")]
    public float maxStamina = 100f;

    public float staminaDrainRate = 20f;

    public float staminaRecoveryRate = 30f;


    // =========================================================
    // SMOOTH MOVEMENT
    // =========================================================

    [Header("Smooth Movement")]
    public float acceleration = 15f;

    public float deceleration = 20f;


    // =========================================================
    // PULO E GRAVIDADE
    // =========================================================

    [Header("Pulo e Gravidade")]
    public float jumpHeight = 0.1f;

    public float gravity = 280f;


    // =========================================================
    // CÂMERA
    // =========================================================

    [Header("Câmera")]
    public Transform cameraTransform;


    // =========================================================
    // VARIÁVEIS INTERNAS
    // =========================================================

    private CharacterController controller;

    private Vector3 velocity;

    private Vector3 currentMoveVelocity;

    private PlayerNoise noise;

    private PlayerFootsteps footsteps;


    // =========================================================
    // ESTADO
    // =========================================================

    public bool isCrouching { get; private set; }

    public bool isRunning { get; private set; }


    // =========================================================
    // ESTAMINA ATUAL
    // =========================================================

    [SerializeField]
    private float currentStamina;

    private bool staminaExhausted = false;


    // =========================================================
    // CONTROLE DO POUSO
    // =========================================================

    private bool wasGrounded;

    private bool hasJumped;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        controller =
            GetComponent<CharacterController>();


        noise =
            GetComponent<PlayerNoise>();


        footsteps =
            GetComponent<PlayerFootsteps>();


        wasGrounded =
            controller.isGrounded;


        // Começa com a estamina cheia.

        currentStamina =
            maxStamina;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        bool isGrounded =
            controller.isGrounded;


        MovePlayer(isGrounded);

        HandleJump(isGrounded);

        ApplyGravity();


        bool nowGrounded =
            controller.isGrounded;


        // =====================================================
        // ATERRISSAGEM
        // =====================================================

        if (!wasGrounded && nowGrounded)
        {
            if (hasJumped)
            {
                if (footsteps != null)
                {
                    footsteps.PlayLandingSound();
                }


                hasJumped = false;
            }
        }


        wasGrounded =
            nowGrounded;


        // =====================================================
        // ESTAMINA
        // =====================================================

        UpdateStamina();
    }


    // =========================================================
    // MOVIMENTO
    // =========================================================

    void MovePlayer(bool isGrounded)
    {
        float horizontal =
            Input.GetAxis("Horizontal");


        float vertical =
            Input.GetAxis("Vertical");


        // =====================================================
        // DIREÇÃO BASEADA NA CÂMERA
        // =====================================================

        Vector3 forward =
            cameraTransform.forward;


        Vector3 right =
            cameraTransform.right;


        forward.y = 0;

        right.y = 0;


        forward.Normalize();

        right.Normalize();


        Vector3 move =
            forward * vertical +
            right * horizontal;


        bool isMoving =
            move.magnitude > 0.1f;


        // =====================================================
        // AGACHAMENTO
        // =====================================================

        isCrouching =
            Input.GetKey(crouchKey);


        // =====================================================
        // CORRIDA
        // =====================================================

        // IMPORTANTE:
        //
        // O jogador precisa obrigatoriamente estar
        // pressionando W para poder correr.
        //
        // Shift sozinho não permite corrida.
        // S + Shift não permite corrida.
        // A + Shift não permite corrida.
        // D + Shift não permite corrida.
        //
        // W + Shift = corrida.

        bool wantsToRun =
            Input.GetKey(runKey) &&
            Input.GetKey(KeyCode.W) &&
            isMoving &&
            !isCrouching;


        // =====================================================
        // VERIFICA SE PODE CORRER
        // =====================================================

        if (
            staminaExhausted ||
            currentStamina <= 0f
        )
        {
            isRunning = false;
        }
        else
        {
            isRunning =
                wantsToRun;
        }


        // =====================================================
        // GARANTE BLOQUEIO NO ZERO
        // =====================================================

        if (currentStamina <= 0f)
        {
            currentStamina = 0f;

            isRunning = false;

            staminaExhausted = true;
        }


        // =====================================================
        // NOISE
        // =====================================================

        if (isGrounded)
        {
            if (noise != null)
            {
                noise.SetMovementNoise(
                    isMoving,
                    isRunning,
                    isCrouching
                );
            }
        }
        else
        {
            if (noise != null)
            {
                noise.SetMovementNoise(
                    false,
                    false,
                    isCrouching
                );
            }
        }


        // =====================================================
        // PASSOS
        // =====================================================

        if (footsteps != null)
        {
            footsteps.UpdateFootsteps(
                isMoving,
                isRunning,
                isCrouching,
                isGrounded
            );
        }


        // =====================================================
        // VELOCIDADE
        // =====================================================

        float currentSpeed;


        if (isCrouching)
        {
            currentSpeed =
                crouchSpeed;
        }
        else if (isRunning)
        {
            // Corrida só acontece quando W está pressionado.

            currentSpeed =
                runSpeed;
        }
        else
        {
            // Caminhada normal.

            currentSpeed =
                walkSpeed;
        }


        // =====================================================
        // MOVIMENTO SUAVE
        // =====================================================

        Vector3 targetVelocity =
            move * currentSpeed;


        float smoothRate =
            isMoving
                ? acceleration
                : deceleration;


        currentMoveVelocity =
            Vector3.Lerp(
                currentMoveVelocity,
                targetVelocity,
                smoothRate *
                Time.deltaTime
            );


        controller.Move(
            currentMoveVelocity *
            Time.deltaTime
        );
    }


    // =========================================================
    // ESTAMINA
    // =========================================================

    void UpdateStamina()
    {
        // =====================================================
        // CORRENDO = GASTA ESTAMINA
        // =====================================================

        if (isRunning)
        {
            currentStamina -=
                staminaDrainRate *
                Time.deltaTime;


            if (currentStamina <= 0f)
            {
                currentStamina = 0f;

                isRunning = false;

                staminaExhausted = true;
            }


            return;
        }


        // =====================================================
        // NÃO ESTÁ CORRENDO = RECUPERA
        // =====================================================

        // Recupera mesmo enquanto o jogador está andando.

        currentStamina +=
            staminaRecoveryRate *
            Time.deltaTime;


        currentStamina =
            Mathf.Clamp(
                currentStamina,
                0f,
                maxStamina
            );


        // =====================================================
        // 50% = LIBERA CORRIDA NOVAMENTE
        // =====================================================

        if (
            staminaExhausted &&
            currentStamina >=
            maxStamina * 0.5f
        )
        {
            staminaExhausted = false;
        }
    }


    // =========================================================
    // PULO
    // =========================================================

    void HandleJump(bool isGrounded)
    {
        if (!isGrounded)
            return;


        if (isCrouching)
            return;


        if (Input.GetButtonDown("Jump"))
        {
            velocity.y =
                Mathf.Sqrt(
                    jumpHeight *
                    -2f *
                    gravity
                );


            if (noise != null)
            {
                noise.MakeJumpNoise();
            }


            if (footsteps != null)
            {
                footsteps.PlayJumpSound();
            }


            hasJumped = true;
        }
    }


    // =========================================================
    // GRAVIDADE
    // =========================================================

    void ApplyGravity()
    {
        if (
            controller.isGrounded &&
            velocity.y < 0
        )
        {
            velocity.y = -2f;
        }


        velocity.y +=
            gravity *
            Time.deltaTime;


        controller.Move(
            velocity *
            Time.deltaTime
        );
    }


    // =========================================================
    // GETTERS DA ESTAMINA
    // =========================================================

    public float GetCurrentStamina()
    {
        return currentStamina;
    }


    public float GetMaxStamina()
    {
        return maxStamina;
    }


    public float GetStaminaPercentage()
    {
        if (maxStamina <= 0f)
            return 0f;


        return currentStamina /
               maxStamina;
    }


    public bool IsStaminaExhausted()
    {
        return staminaExhausted;
    }
}