using System.Collections;
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

    [Header("Estamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 10f;
    public float staminaRecoveryRate = 10f;

    [Header("Smooth Movement")]
    public float acceleration = 15f;
    public float deceleration = 20f;

    [Header("Pulo e Gravidade")]
    public float jumpHeight = 0.1f;
    public float gravity = 280f;

    [Header("Câmera")]
    public Transform cameraTransform;

    [Header("Respiração Cansada")]
    public AudioSource breathingAudioSource;
    public AudioClip tiredBreathingSound;

    [Range(0f, 1f)]
    public float breathingVolume = 1f;

    [Tooltip("Velocidade do fade quando a respiração termina.")]
    public float breathingFadeOutSpeed = 5f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;

    private PlayerNoise noise;
    private PlayerFootsteps footsteps;

    public bool isCrouching { get; private set; }
    public bool isRunning { get; private set; }

    [SerializeField]
    private float currentStamina;

    private bool staminaExhausted = false;
    private bool wasGrounded;
    private bool hasJumped;

    private Coroutine breathingFadeCoroutine;


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

        currentStamina =
            maxStamina;

        SetupBreathingAudio();
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


        // -----------------------------------------------------
        // LANDING
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // ESTAMINA
        // -----------------------------------------------------

        UpdateStamina();


        // -----------------------------------------------------
        // RESPIRAÇÃO
        // -----------------------------------------------------

        UpdateBreathing();
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


        // -----------------------------------------------------
        // DIREÇÃO DA CÂMERA
        // -----------------------------------------------------

        Vector3 forward =
            cameraTransform.forward;

        Vector3 right =
            cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();


        // -----------------------------------------------------
        // MOVIMENTO
        // -----------------------------------------------------

        Vector3 move =
            forward * vertical +
            right * horizontal;


        // -----------------------------------------------------
        // CORREÇÃO DA VELOCIDADE DIAGONAL
        // -----------------------------------------------------

        move =
            Vector3.ClampMagnitude(
                move,
                1f
            );


        bool isMoving =
            move.magnitude > 0.1f;


        // -----------------------------------------------------
        // AGACHAMENTO
        // -----------------------------------------------------

        isCrouching =
            Input.GetKey(crouchKey);


        // -----------------------------------------------------
        // CORRIDA
        // -----------------------------------------------------
        //
        // O jogador só pode correr se:
        //
        // 1. Estiver segurando Shift
        // 2. Estiver segurando W
        // 3. Estiver realmente se movendo para frente
        // 4. Não estiver agachado
        //
        // Dessa forma:
        //
        // W + Shift       = CORRE
        // W + A + Shift   = CORRE
        // W + D + Shift   = CORRE
        // S + Shift       = NÃO CORRE
        // S + A + Shift   = NÃO CORRE
        // S + D + Shift   = NÃO CORRE
        //

        bool movingForward =
            vertical > 0.1f;


        bool wantsToRun =
            Input.GetKey(runKey) &&
            movingForward &&
            isMoving &&
            !isCrouching;


        // -----------------------------------------------------
        // CONTROLE DA CORRIDA
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // ESTAMINA ESGOTADA
        // -----------------------------------------------------

        if (currentStamina <= 0f)
        {
            currentStamina = 0f;

            isRunning = false;

            staminaExhausted = true;
        }


        // -----------------------------------------------------
        // PLAYER NOISE
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // PASSOS
        // -----------------------------------------------------

        if (footsteps != null)
        {
            footsteps.UpdateFootsteps(
                isMoving,
                isRunning,
                isCrouching,
                isGrounded
            );
        }


        // -----------------------------------------------------
        // VELOCIDADE
        // -----------------------------------------------------

        float currentSpeed;


        if (isCrouching)
        {
            currentSpeed =
                crouchSpeed;
        }
        else if (isRunning)
        {
            currentSpeed =
                runSpeed;
        }
        else
        {
            currentSpeed =
                walkSpeed;
        }


        // -----------------------------------------------------
        // VELOCIDADE ALVO
        // -----------------------------------------------------

        Vector3 targetVelocity =
            move *
            currentSpeed;


        // -----------------------------------------------------
        // ACELERAÇÃO / DESACELERAÇÃO
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // MOVE CHARACTER CONTROLLER
        // -----------------------------------------------------

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
        if (isRunning)
        {
            currentStamina -=
                staminaDrainRate *
                Time.deltaTime;


            if (currentStamina <= 0f)
            {
                currentStamina = 0f;

                isRunning = false;


                if (!staminaExhausted)
                {
                    staminaExhausted = true;

                    StartTiredBreathing();
                }
            }

            return;
        }


        // -----------------------------------------------------
        // RECUPERAÇÃO
        // -----------------------------------------------------

        currentStamina +=
            staminaRecoveryRate *
            Time.deltaTime;


        currentStamina =
            Mathf.Clamp(
                currentStamina,
                0f,
                maxStamina
            );


        // -----------------------------------------------------
        // SAI DO ESTADO DE EXAUSTÃO EM 50%
        // -----------------------------------------------------

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
    // RESPIRAÇÃO
    // =========================================================

    void UpdateBreathing()
    {
        if (breathingAudioSource == null)
            return;

        if (!breathingAudioSource.isPlaying)
            return;

        if (staminaExhausted)
            return;

        FadeOutBreathing();
    }


    // =========================================================
    // INICIA RESPIRAÇÃO CANSADA
    // =========================================================

    void StartTiredBreathing()
    {
        if (breathingAudioSource == null)
        {
            Debug.LogWarning(
                "PlayerMovement: Breathing Audio Source não configurado!"
            );

            return;
        }


        if (tiredBreathingSound == null)
        {
            Debug.LogWarning(
                "PlayerMovement: Tired Breathing Sound não configurado!"
            );

            return;
        }


        if (breathingFadeCoroutine != null)
        {
            StopCoroutine(
                breathingFadeCoroutine
            );

            breathingFadeCoroutine = null;
        }


        breathingAudioSource.clip =
            tiredBreathingSound;

        breathingAudioSource.loop =
            true;

        breathingAudioSource.playOnAwake =
            false;

        breathingAudioSource.spatialBlend =
            0f;

        breathingAudioSource.volume =
            breathingVolume;


        breathingAudioSource.Stop();

        breathingAudioSource.Play();


        Debug.Log(
            "PlayerMovement: Respiração cansada iniciada!"
        );
    }


    // =========================================================
    // FADE DA RESPIRAÇÃO
    // =========================================================

    void FadeOutBreathing()
    {
        if (breathingAudioSource == null)
            return;


        if (breathingFadeCoroutine != null)
            return;


        breathingFadeCoroutine =
            StartCoroutine(
                FadeBreathingCoroutine()
            );
    }


    // =========================================================
    // COROUTINE FADE
    // =========================================================

    IEnumerator FadeBreathingCoroutine()
    {
        float targetVolume = 0f;


        while (
            breathingAudioSource != null &&
            breathingAudioSource.volume >
            targetVolume
        )
        {
            breathingAudioSource.volume =
                Mathf.MoveTowards(
                    breathingAudioSource.volume,
                    targetVolume,
                    breathingFadeOutSpeed *
                    Time.deltaTime
                );


            yield return null;
        }


        if (breathingAudioSource != null)
        {
            breathingAudioSource.Stop();

            breathingAudioSource.volume =
                breathingVolume;
        }


        breathingFadeCoroutine = null;
    }


    // =========================================================
    // CONFIGURAÇÃO DA RESPIRAÇÃO
    // =========================================================

    void SetupBreathingAudio()
    {
        if (breathingAudioSource == null)
            return;


        breathingAudioSource.playOnAwake =
            false;

        breathingAudioSource.loop =
            true;

        breathingAudioSource.spatialBlend =
            0f;

        breathingAudioSource.volume =
            breathingVolume;


        breathingAudioSource.Stop();
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