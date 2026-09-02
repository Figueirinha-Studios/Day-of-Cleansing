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

    void Start()
    {
        controller = GetComponent<CharacterController>();

        noise = GetComponent<PlayerNoise>();
        footsteps = GetComponent<PlayerFootsteps>();

        wasGrounded = controller.isGrounded;

        currentStamina = maxStamina;

        SetupBreathingAudio();
    }

    void Update()
    {
        bool isGrounded = controller.isGrounded;

        MovePlayer(isGrounded);
        HandleJump(isGrounded);
        ApplyGravity();

        bool nowGrounded = controller.isGrounded;

        if (!wasGrounded && nowGrounded)
        {
            if (hasJumped)
            {
                if (footsteps != null)
                    footsteps.PlayLandingSound();

                hasJumped = false;
            }
        }

        wasGrounded = nowGrounded;

        UpdateStamina();
        UpdateBreathing();
    }

    void MovePlayer(bool isGrounded)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 move =
            forward * vertical +
            right * horizontal;

        bool isMoving = move.magnitude > 0.1f;

        isCrouching = Input.GetKey(crouchKey);

        bool wantsToRun =
            Input.GetKey(runKey) &&
            Input.GetKey(KeyCode.W) &&
            isMoving &&
            !isCrouching;

        // Não permite correr enquanto estiver exausto
        if (staminaExhausted || currentStamina <= 0f)
        {
            isRunning = false;
        }
        else
        {
            isRunning = wantsToRun;
        }

        // Garante que a estamina nunca fique negativa
        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            isRunning = false;
            staminaExhausted = true;
        }

        // Ruído
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

        // Passos
        if (footsteps != null)
        {
            footsteps.UpdateFootsteps(
                isMoving,
                isRunning,
                isCrouching,
                isGrounded
            );
        }

        float currentSpeed;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isRunning)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

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
                smoothRate * Time.deltaTime
            );

        controller.Move(
            currentMoveVelocity *
            Time.deltaTime
        );
    }

    void UpdateStamina()
    {
        // =========================
        // GASTANDO ESTAMINA
        // =========================

        if (isRunning)
        {
            currentStamina -=
                staminaDrainRate *
                Time.deltaTime;

            // Chegou a zero
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;

                isRunning = false;

                // Só executa quando realmente
                // entra no estado de exaustão
                if (!staminaExhausted)
                {
                    staminaExhausted = true;

                    StartTiredBreathing();
                }
            }

            return;
        }

        // =========================
        // RECUPERANDO ESTAMINA
        // =========================

        currentStamina +=
            staminaRecoveryRate *
            Time.deltaTime;

        currentStamina =
            Mathf.Clamp(
                currentStamina,
                0f,
                maxStamina
            );

        // Precisa chegar a 50%
        // para poder correr novamente
        if (
            staminaExhausted &&
            currentStamina >=
            maxStamina * 0.5f
        )
        {
            staminaExhausted = false;
        }
    }

    void UpdateBreathing()
    {
        if (breathingAudioSource == null)
            return;

        if (!breathingAudioSource.isPlaying)
            return;

        // Enquanto estiver abaixo de 50%,
        // mantém a respiração tocando
        if (staminaExhausted)
            return;

        // Chegou a 50%, então faz fade-out
        FadeOutBreathing();
    }

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

        // Cancela qualquer fade que esteja acontecendo
        if (breathingFadeCoroutine != null)
        {
            StopCoroutine(
                breathingFadeCoroutine
            );

            breathingFadeCoroutine = null;
        }

        // Configura o áudio
        breathingAudioSource.clip =
            tiredBreathingSound;

        breathingAudioSource.loop = true;

        breathingAudioSource.playOnAwake = false;

        // Respiração do próprio jogador:
        // 2D para ficar sempre audível
        breathingAudioSource.spatialBlend = 0f;

        breathingAudioSource.volume =
            breathingVolume;

        // Sempre inicia o áudio novamente
        breathingAudioSource.Stop();
        breathingAudioSource.Play();

        Debug.Log(
            "PlayerMovement: Respiração cansada iniciada!"
        );
    }

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

    IEnumerator FadeBreathingCoroutine()
    {
        float targetVolume = 0f;

        while (
            breathingAudioSource != null &&
            breathingAudioSource.volume > targetVolume
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

    void SetupBreathingAudio()
    {
        if (breathingAudioSource == null)
            return;

        breathingAudioSource.playOnAwake = false;
        breathingAudioSource.loop = true;

        // 2D
        breathingAudioSource.spatialBlend = 0f;

        breathingAudioSource.volume =
            breathingVolume;

        breathingAudioSource.Stop();
    }

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
                noise.MakeJumpNoise();

            if (footsteps != null)
                footsteps.PlayJumpSound();

            hasJumped = true;
        }
    }

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