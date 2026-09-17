using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StealthScare : MonoBehaviour
{
    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("Referências")]
    public EnemyAI enemyAI;

    public EnemyVision enemyVision;

    public EnemyAudio enemyAudio;

    public Generator generator;

    public CameraController cameraController;

    public Camera playerCamera;

    public Transform player;

    public Transform chappie;

    public NavMeshAgent agent;


    // =========================================================
    // TEMPO PARA ATIVAR
    // =========================================================

    [Header("Ativação")]
    [Tooltip("Tempo que o jogador precisa ficar sem ver o Chappie.")]
    public float timeWithoutSeeing = 120f;

    [Tooltip("Chance de ativar o Stealth depois do tempo.")]
    [Range(0f, 100f)]
    public float stealthChance = 35f;

    [Tooltip("Depois de cada tentativa, começa uma nova janela de tempo.")]
    public bool resetTimerAfterAttempt = true;


    // =========================================================
    // COOLDOWN
    // =========================================================

    [Header("Cooldown")]
    [Tooltip("Tempo mínimo entre Stealth Scares.")]
    public float stealthCooldown = 60f;

    private float cooldownTimer = 0f;


    // =========================================================
    // GERADOR
    // =========================================================

    [Header("Gerador")]
    [Tooltip("Quando 2 ou mais itens já foram colocados, o Stealth é desativado.")]
    public int minimumGeneratorItemsToDisable = 2;


    // =========================================================
    // DISTÂNCIA
    // =========================================================

    [Header("Aproximação")]
    [Tooltip("Distância em que o Chappie para antes do grito.")]
    public float stopDistance = 4f;

    [Tooltip("Velocidade usada pelo Chappie durante a aproximação.")]
    public float stealthSpeed = 8f;

    [Tooltip("Velocidade com que a câmera vira para o Chappie.")]
    public float cameraLookSpeed = 180f;


    // =========================================================
    // ÁUDIO
    // =========================================================

    [Header("Áudio")]
    public AudioSource audioSource;

    [Tooltip("Som quando o Chappie percebe que foi descoberto.")]
    public AudioClip spottedSound;

    [Tooltip("Grito do Chappie.")]
    public AudioClip screamSound;

    [Range(0f, 1f)]
    public float spottedVolume = 1f;

    [Range(0f, 1f)]
    public float screamVolume = 1f;


    // =========================================================
    // PASSOS DURANTE STEALTH
    // =========================================================

    [Header("Passos durante Stealth")]
    [Tooltip("Volume dos passos enquanto Chappie corre durante o Stealth.")]
    [Range(0f, 1f)]
    public float stealthFootstepVolume = 0.05f;


    // =========================================================
    // TEMPOS
    // =========================================================

    [Header("Tempos")]
    [Tooltip("Pausa depois que o jogador percebe o Chappie.")]
    public float spottedPause = 1f;

    [Tooltip("Tempo do grito.")]
    public float screamDuration = 2f;

    [Tooltip("Tempo que o Chappie fica parado depois do grito.")]
    public float pauseAfterScream = 3f;

    [Tooltip("Tempo que o Encounter fica bloqueado após o Stealth.")]
    public float encounterBlockDuration = 15f;


    // =========================================================
    // ESTADO
    // =========================================================

    private float unseenTimer = 0f;

    private bool stealthActive = false;

    private bool hasSeenPlayerDuringApproach = false;

    private Coroutine stealthCoroutine;

    private float originalStoppingDistance;


    // =========================================================
    // BLOQUEIO GLOBAL DO ENCOUNTER
    // =========================================================

    private static float encounterBlockedUntil = 0f;

    public static bool IsEncounterBlocked
    {
        get
        {
            return Time.time <
                   encounterBlockedUntil;
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (enemyAI == null)
        {
            enemyAI =
                GetComponent<EnemyAI>();
        }

        if (enemyVision == null)
        {
            enemyVision =
                GetComponent<EnemyVision>();
        }

        if (enemyAudio == null)
        {
            enemyAudio =
                GetComponent<EnemyAudio>();
        }

        if (agent == null)
        {
            agent =
                GetComponent<NavMeshAgent>();
        }

        if (chappie == null)
        {
            chappie =
                transform;
        }

        if (player == null &&
            enemyAI != null)
        {
            player =
                enemyAI.player;
        }

        if (generator == null)
        {
            generator =
                FindFirstObjectByType<Generator>();
        }

        unseenTimer = 0f;
        cooldownTimer = 0f;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (stealthActive)
            return;

        if (player == null)
            return;

        if (playerCamera == null)
            return;


        // -----------------------------------------------------
        // COOLDOWN
        // -----------------------------------------------------

        if (cooldownTimer > 0f)
        {
            cooldownTimer -=
                Time.deltaTime;
        }


        // -----------------------------------------------------
        // GERADOR
        // -----------------------------------------------------

        if (GeneratorHasTwoOrMoreItems())
        {
            unseenTimer = 0f;
            return;
        }


        // -----------------------------------------------------
        // COOLDOWN
        // -----------------------------------------------------

        if (cooldownTimer > 0f)
        {
            return;
        }


        // -----------------------------------------------------
        // PLAYER ESTÁ OLHANDO PARA CHAPPIE?
        // -----------------------------------------------------

        bool playerLookingAtChappie =
            IsPlayerLookingAtChappie();

        if (playerLookingAtChappie)
        {
            unseenTimer = 0f;
            return;
        }


        // -----------------------------------------------------
        // PLAYER NÃO VIU CHAPPIE
        // -----------------------------------------------------

        unseenTimer +=
            Time.deltaTime;

        if (unseenTimer <
            timeWithoutSeeing)
        {
            return;
        }


        // -----------------------------------------------------
        // CHAPPIE PRECISA CONSEGUIR VER PLAYER
        // -----------------------------------------------------

        if (enemyVision == null)
            return;

        if (!enemyVision.CanSeePlayer())
            return;


        // -----------------------------------------------------
        // TENTATIVA
        // -----------------------------------------------------

        TryTriggerStealth();
    }


    // =========================================================
    // TENTAR ATIVAR
    // =========================================================

    private void TryTriggerStealth()
    {
        if (stealthActive)
            return;

        if (cooldownTimer > 0f)
            return;

        if (GeneratorHasTwoOrMoreItems())
        {
            unseenTimer = 0f;
            return;
        }


        // -----------------------------------------------------
        // NOVA JANELA DE TEMPO
        // -----------------------------------------------------

        if (resetTimerAfterAttempt)
        {
            unseenTimer = 0f;
        }


        // -----------------------------------------------------
        // SORTEIO
        // -----------------------------------------------------

        float roll =
            Random.Range(
                0f,
                100f
            );

        Debug.Log(
            "STEALTH SCARE: Sorteio = " +
            roll.ToString("F1") +
            "% | Chance = " +
            stealthChance +
            "%"
        );


        // -----------------------------------------------------
        // 65% → CHASE NORMAL
        // -----------------------------------------------------

        if (roll >= stealthChance)
        {
            Debug.Log(
                "STEALTH SCARE: Sorteio falhou. " +
                "Iniciando CHASE NORMAL."
            );

            if (enemyAI != null)
            {
                enemyAI.ForceChase();
            }

            return;
        }


        // -----------------------------------------------------
        // 35% → STEALTH
        // -----------------------------------------------------

        Debug.Log(
            "STEALTH SCARE: SORTEIO VENCIDO! " +
            "Iniciando Stealth Scare."
        );

        stealthCoroutine =
            StartCoroutine(
                StealthRoutine()
            );
    }


    // =========================================================
    // STEALTH PRINCIPAL
    // =========================================================

    private IEnumerator StealthRoutine()
    {
        stealthActive = true;

        hasSeenPlayerDuringApproach =
            false;


        // -----------------------------------------------------
        // BLOQUEIA ENCOUNTER
        // -----------------------------------------------------

        encounterBlockedUntil =
            Time.time +
            encounterBlockDuration;


        // -----------------------------------------------------
        // ASSUME CONTROLE DO CHAPPIE
        // -----------------------------------------------------

        if (enemyAI != null)
        {
            enemyAI.BeginStealthControl();
        }


        // -----------------------------------------------------
        // DIMINUI PASSOS
        // -----------------------------------------------------

        if (enemyAudio != null)
        {
            enemyAudio.SetFootstepVolume(
                stealthFootstepVolume
            );
        }


        if (agent == null)
        {
            FinishStealthAndChase();
            yield break;
        }

        if (!agent.isOnNavMesh)
        {
            FinishStealthAndChase();
            yield break;
        }


        originalStoppingDistance =
            agent.stoppingDistance;

        agent.stoppingDistance =
            stopDistance;

        agent.speed =
            stealthSpeed;

        agent.isStopped = false;
        agent.updateRotation = true;


        // =====================================================
        // APROXIMAÇÃO
        // =====================================================

        while (true)
        {
            if (player == null)
            {
                FinishStealthAndChase();
                yield break;
            }


            // -------------------------------------------------
            // GERADOR CHEGOU A 2/3
            // -------------------------------------------------

            if (GeneratorHasTwoOrMoreItems())
            {
                Debug.Log(
                    "STEALTH SCARE: Gerador chegou a 2/3. " +
                    "Stealth cancelado."
                );

                FinishStealthAndChase();
                yield break;
            }


            // -------------------------------------------------
            // ATUALIZA DESTINO
            // -------------------------------------------------

            agent.SetDestination(
                player.position
            );


            // -------------------------------------------------
            // PLAYER PERCEBEU CHAPPIE
            // -------------------------------------------------

            if (IsPlayerLookingAtChappie())
            {
                hasSeenPlayerDuringApproach =
                    true;

                yield return PlayerSpottedRoutine();

                yield break;
            }


            // -------------------------------------------------
            // CHEGOU PERTO
            // -------------------------------------------------

            float distance =
                Vector3.Distance(
                    transform.position,
                    player.position
                );

            if (distance <=
                stopDistance + 0.1f)
            {
                break;
            }

            yield return null;
        }


        // =====================================================
        // PARA
        // =====================================================

        agent.isStopped = true;


        // =====================================================
        // GRITO + CÂMERA
        // =====================================================

        if (cameraController != null)
        {
            cameraController.BeginAutomaticLook();
        }


        if (audioSource != null &&
            screamSound != null)
        {
            audioSource.PlayOneShot(
                screamSound,
                screamVolume
            );
        }


        float screamTimer = 0f;

        while (screamTimer <
               screamDuration)
        {
            if (cameraController != null &&
                chappie != null)
            {
                cameraController.RotateAutomaticallyTowards(
                    chappie.position,
                    cameraLookSpeed
                );
            }

            screamTimer +=
                Time.deltaTime;

            yield return null;
        }


        // =====================================================
        // DEVOLVE CÂMERA
        // =====================================================

        if (cameraController != null)
        {
            cameraController.EndAutomaticLook();
        }


        // =====================================================
        // CHAPPIE FICA PARADO
        // =====================================================

        yield return new WaitForSeconds(
            pauseAfterScream
        );


        // =====================================================
        // CHASE NORMAL
        // =====================================================

        FinishStealthAndChase();
    }


    // =========================================================
    // PLAYER PERCEBEU DURANTE A APROXIMAÇÃO
    // =========================================================

    private IEnumerator PlayerSpottedRoutine()
    {
        Debug.Log(
            "STEALTH SCARE: Jogador percebeu o Chappie durante a aproximação."
        );


        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }


        // =====================================================
        // CÂMERA VIRA PARA O CHAPPIE
        // =====================================================

        if (cameraController != null)
        {
            cameraController.BeginAutomaticLook();
        }


        // =====================================================
        // SOM DE SPOTTED
        // =====================================================

        if (audioSource != null &&
            spottedSound != null)
        {
            audioSource.PlayOneShot(
                spottedSound,
                spottedVolume
            );
        }


        // =====================================================
        // MANTÉM A CÂMERA OLHANDO
        // =====================================================

        float spottedTimer = 0f;

        while (spottedTimer <
               spottedPause)
        {
            if (cameraController != null &&
                chappie != null)
            {
                cameraController.RotateAutomaticallyTowards(
                    chappie.position,
                    cameraLookSpeed
                );
            }

            spottedTimer +=
                Time.deltaTime;

            yield return null;
        }


        // =====================================================
        // DEVOLVE CÂMERA
        // =====================================================

        if (cameraController != null)
        {
            cameraController.EndAutomaticLook();
        }


        // =====================================================
        // CHASE NORMAL
        // =====================================================

        FinishStealthAndChase();
    }


    // =========================================================
    // FINALIZAR STEALTH
    // =========================================================

    private void FinishStealthAndChase()
    {
        // -----------------------------------------------------
        // RESTAURA CÂMERA
        // -----------------------------------------------------

        if (cameraController != null)
        {
            cameraController.EndAutomaticLook();
        }


        // -----------------------------------------------------
        // RESTAURA PASSOS
        // -----------------------------------------------------

        if (enemyAudio != null)
        {
            enemyAudio.RestoreFootstepVolume();
        }


        // -----------------------------------------------------
        // RESTAURA NAVMESH
        // -----------------------------------------------------

        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.stoppingDistance =
                originalStoppingDistance;

            agent.isStopped = false;
            agent.updateRotation = true;
        }


        // -----------------------------------------------------
        // FINALIZA STEALTH
        // -----------------------------------------------------

        stealthActive = false;

        cooldownTimer =
            stealthCooldown;

        hasSeenPlayerDuringApproach =
            false;


        // -----------------------------------------------------
        // CHASE NORMAL
        // -----------------------------------------------------

        if (enemyAI != null)
        {
            enemyAI.EndStealthControl();

            enemyAI.ForceChase();
        }


        stealthCoroutine = null;
    }


    // =========================================================
    // VERIFICAR GERADOR
    // =========================================================

    private bool GeneratorHasTwoOrMoreItems()
    {
        if (generator == null)
        {
            generator =
                FindFirstObjectByType<Generator>();
        }

        if (generator == null)
            return false;


        int insertedItems = 0;


        if (generator.GetGasolineInserted() > 0)
        {
            insertedItems +=
                generator.GetGasolineInserted();
        }


        if (generator.IsFuseInserted())
        {
            insertedItems++;
        }


        return
            insertedItems >=
            minimumGeneratorItemsToDisable;
    }


    // =========================================================
    // PLAYER OLHANDO PARA CHAPPIE
    // =========================================================

    private bool IsPlayerLookingAtChappie()
    {
        if (playerCamera == null ||
            chappie == null)
            return false;


        Vector3 viewport =
            playerCamera.WorldToViewportPoint(
                chappie.position
            );


        if (viewport.z <= 0f)
            return false;


        // =====================================================
        // CHAPPIE ESTÁ FORA DA TELA
        // =====================================================

        if (
            viewport.x < 0f ||
            viewport.x > 1f ||
            viewport.y < 0f ||
            viewport.y > 1f
        )
        {
            return false;
        }


        // =====================================================
        // CHAPPIE ESTÁ NA TELA
        // =====================================================

        // Não precisa estar no centro da câmera.
        // Só de aparecer na tela, conta como jogador viu.

        return true;
    }
}