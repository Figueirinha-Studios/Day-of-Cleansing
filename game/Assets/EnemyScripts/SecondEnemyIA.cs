using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SecondEnemyIA : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public Transform patrolPointsParent;

    private EnemyVision vision;
    private NavMeshAgent agent;
    private Animator animator;

    // =========================================================
    // VELOCIDADE
    // =========================================================

    [Header("Velocidade")]
    public float patrolSpeed = 5f;
    public float chaseSpeed = 8f;

    // =========================================================
    // CHASE
    // =========================================================

    [Header("Distância do Jogador")]
    public float chaseStopDistance = 3f;

    [Header("Memória de Chase")]
    public float chaseMemoryTime = 3f;

    private float chaseMemoryTimer = 0f;

    // =========================================================
    // CHAMAR CHAPPIE
    // =========================================================

    [Header("Chamar Chappie")]
    public AudioClip callChappieSound;

    [Range(0f, 10f)]
    public float callChappieVolume = 5f;

    [Tooltip("Quando o Segundo Inimigo estiver nessa distância ou menos do Player, começa a chamar o Chappie.")]
    public float callChappieDistance = 8f;

    [Tooltip("Raio usado pelo NoiseSystem para o Chappie detectar o chamado.")]
    public float callNoiseRadius = 200f;

    [Tooltip("Tempo entre cada sinal enviado ao NoiseSystem.")]
    public float callNoiseInterval = 2f;

    [Tooltip("Se ativado, o som pode ser ouvido pelo mapa inteiro.")]
    public bool globalCallSound = true;

    private AudioSource callChappieAudioSource;

    private bool callingChappie = false;

    private float callNoiseTimer = 0f;

    // =========================================================
    // MOVIMENTO
    // =========================================================

    [Header("Movimento Natural")]
    public float movementAcceleration = 8f;
    public float rotationSpeed = 180f;
    public float stoppingDistance = 0.5f;

    // =========================================================
    // PATRULHA
    // =========================================================

    [Header("Patrulha")]
    [Min(0)]
    public int rememberedPatrolPoints = 3;

    private Transform[] patrolPoints;

    private List<int> recentlyVisitedPoints =
        new List<int>();

    private int currentPoint = -1;

    // =========================================================
    // ESTADOS
    // =========================================================

    public enum EnemyState
    {
        Patrol,
        Chase
    }

    [Header("Estado")]
    [SerializeField]
    private EnemyState currentState =
        EnemyState.Patrol;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        agent =
            GetComponent<NavMeshAgent>();

        vision =
            GetComponent<EnemyVision>();

        animator =
            GetComponent<Animator>();

        SetupCallChappieAudio();

        ConfigureMovement();

        LoadPatrolPoints();

        if (patrolPoints != null &&
            patrolPoints.Length > 0)
        {
            ChooseNextPatrolPoint();
        }
    }

    // =========================================================
    // CONFIGURA ÁUDIO DO CHAMADO
    // =========================================================

    private void SetupCallChappieAudio()
    {
        callChappieAudioSource =
            GetComponent<AudioSource>();

        if (callChappieAudioSource == null)
        {
            callChappieAudioSource =
                gameObject.AddComponent<AudioSource>();
        }

        callChappieAudioSource.clip =
            callChappieSound;

        callChappieAudioSource.volume =
            callChappieVolume;

        callChappieAudioSource.loop =
            true;

        callChappieAudioSource.playOnAwake =
            false;

        // =====================================================
        // ÁUDIO GLOBAL / 3D
        // =====================================================

        if (globalCallSound)
        {
            // 0 = totalmente 2D.
            // O jogador consegue ouvir independentemente
            // da distância do objeto.
            callChappieAudioSource.spatialBlend =
                0f;
        }
        else
        {
            // 1 = totalmente 3D.
            callChappieAudioSource.spatialBlend =
                1f;

            callChappieAudioSource.minDistance =
                5f;

            callChappieAudioSource.maxDistance =
                200f;

            callChappieAudioSource.rolloffMode =
                AudioRolloffMode.Linear;
        }

        callChappieAudioSource.priority =
            0;

        callChappieAudioSource.Stop();
    }

    // =========================================================
    // CONFIGURA MOVIMENTO
    // =========================================================

    private void ConfigureMovement()
    {
        if (agent == null)
            return;

        agent.acceleration =
            movementAcceleration;

        agent.angularSpeed =
            rotationSpeed;

        agent.stoppingDistance =
            stoppingDistance;

        agent.autoBraking =
            false;

        agent.updateRotation =
            true;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
        {
            StopCallingChappie();
            return;
        }

        UpdateChaseState();

        switch (currentState)
        {
            case EnemyState.Patrol:

                Patrol();

                break;

            case EnemyState.Chase:

                Chase();

                break;
        }

        UpdateChappieCall();

        UpdateAnimation();
    }

    // =========================================================
    // VERIFICA CHASE
    // =========================================================

    private void UpdateChaseState()
    {
        bool canSeePlayer =
            vision != null &&
            vision.CanSeePlayer();

        if (canSeePlayer)
        {
            currentState =
                EnemyState.Chase;

            chaseMemoryTimer =
                chaseMemoryTime;

            return;
        }

        if (currentState ==
            EnemyState.Chase)
        {
            chaseMemoryTimer -=
                Time.deltaTime;

            if (chaseMemoryTimer > 0f)
                return;

            currentState =
                EnemyState.Patrol;

            chaseMemoryTimer =
                0f;

            StopCallingChappie();

            ChooseNextPatrolPoint();

            return;
        }

        currentState =
            EnemyState.Patrol;
    }

    // =========================================================
    // PATRULHA
    // =========================================================

    private void Patrol()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        agent.isStopped =
            false;

        agent.speed =
            patrolSpeed;

        agent.stoppingDistance =
            stoppingDistance;

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            ChooseNextPatrolPoint();
        }
    }

    // =========================================================
    // CHASE
    // =========================================================

    private void Chase()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        if (player == null)
            return;

        agent.isStopped =
            false;

        agent.speed =
            chaseSpeed;

        agent.stoppingDistance =
            chaseStopDistance;

        agent.SetDestination(
            player.position
        );

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance <=
            chaseStopDistance + 0.5f)
        {
            LookAtPlayer();
        }
    }

    // =========================================================
    // CHAMAR CHAPPIE
    // =========================================================

    private void UpdateChappieCall()
    {
        // Só chama durante o Chase.
        if (currentState !=
            EnemyState.Chase)
        {
            StopCallingChappie();
            return;
        }

        if (player == null)
        {
            StopCallingChappie();
            return;
        }

        if (callChappieSound == null)
        {
            StopCallingChappie();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // =====================================================
        // DENTRO DA DISTÂNCIA
        // =====================================================

        if (distance <=
            callChappieDistance)
        {
            StartCallingChappie();
        }

        // =====================================================
        // FORA DA DISTÂNCIA
        // =====================================================

        else
        {
            StopCallingChappie();
        }
    }

    // =========================================================
    // COMEÇA A CHAMAR
    // =========================================================

    private void StartCallingChappie()
    {
        // =====================================================
        // ÁUDIO
        // =====================================================

        if (callChappieAudioSource != null &&
            callChappieSound != null)
        {
            if (callChappieAudioSource.clip !=
                callChappieSound)
            {
                callChappieAudioSource.clip =
                    callChappieSound;
            }

            callChappieAudioSource.volume =
                callChappieVolume;

            callChappieAudioSource.loop =
                true;

            if (!callChappieAudioSource.isPlaying)
            {
                callChappieAudioSource.Play();

                Debug.Log(
                    "SECOND ENEMY: Chamando Chappie!"
                );
            }
        }

        // =====================================================
        // PRIMEIRO CHAMADO
        // =====================================================

        if (!callingChappie)
        {
            callingChappie =
                true;

            // Faz o primeiro ruído imediatamente.
            EmitChappieNoise();

            callNoiseTimer =
                callNoiseInterval;
        }

        // =====================================================
        // CHAMADOS SEGUINTES
        // =====================================================

        else
        {
            callNoiseTimer -=
                Time.deltaTime;

            if (callNoiseTimer <= 0f)
            {
                EmitChappieNoise();

                callNoiseTimer =
                    callNoiseInterval;
            }
        }
    }

    // =========================================================
    // ENVIA RUÍDO PARA O NOISE SYSTEM
    // =========================================================

    private void EmitChappieNoise()
    {
        NoiseSystem.EmitNoise(
            transform.position,
            callNoiseRadius
        );

        Debug.Log(
            "SECOND ENEMY: Chamado enviado ao NoiseSystem. " +
            "Raio: " +
            callNoiseRadius
        );
    }

    // =========================================================
    // PARA DE CHAMAR
    // =========================================================

    private void StopCallingChappie()
    {
        callingChappie =
            false;

        callNoiseTimer =
            0f;

        if (callChappieAudioSource != null)
        {
            if (callChappieAudioSource.isPlaying)
            {
                callChappieAudioSource.Stop();

                Debug.Log(
                    "SECOND ENEMY: Parou de chamar Chappie."
                );
            }
        }
    }

    // =========================================================
    // OLHAR PARA O PLAYER
    // =========================================================

    private void LookAtPlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    // =========================================================
    // CARREGA PATROL POINTS
    // =========================================================

    private void LoadPatrolPoints()
    {
        if (patrolPointsParent == null)
        {
            Debug.LogWarning(
                "SecondEnemyIA: Patrol Points Parent não foi definido."
            );

            patrolPoints =
                new Transform[0];

            return;
        }

        List<Transform> points =
            new List<Transform>();

        foreach (Transform child
                 in patrolPointsParent)
        {
            points.Add(child);
        }

        patrolPoints =
            points.ToArray();

        Debug.Log(
            "SecondEnemyIA: " +
            patrolPoints.Length +
            " Patrol Points encontrados."
        );
    }

    // =========================================================
    // ESCOLHE PRÓXIMO PONTO
    // =========================================================

    private void ChooseNextPatrolPoint()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        List<int> availablePoints =
            new List<int>();

        for (int i = 0;
             i < patrolPoints.Length;
             i++)
        {
            if (i == currentPoint)
                continue;

            if (recentlyVisitedPoints.Contains(i))
                continue;

            availablePoints.Add(i);
        }

        // =====================================================
        // SE NÃO HOUVER PONTOS DISPONÍVEIS
        // =====================================================

        if (availablePoints.Count == 0)
        {
            recentlyVisitedPoints.Clear();

            for (int i = 0;
                 i < patrolPoints.Length;
                 i++)
            {
                if (i != currentPoint)
                {
                    availablePoints.Add(i);
                }
            }
        }

        if (availablePoints.Count == 0)
            return;

        // =====================================================
        // ESCOLHE ALEATORIAMENTE
        // =====================================================

        int selectedPoint =
            availablePoints[
                Random.Range(
                    0,
                    availablePoints.Count
                )
            ];

        currentPoint =
            selectedPoint;

        recentlyVisitedPoints.Add(
            selectedPoint
        );

        while (
            recentlyVisitedPoints.Count >
            rememberedPatrolPoints
        )
        {
            recentlyVisitedPoints.RemoveAt(0);
        }

        // =====================================================
        // MOVE
        // =====================================================

        agent.isStopped =
            false;

        agent.speed =
            patrolSpeed;

        agent.stoppingDistance =
            stoppingDistance;

        agent.SetDestination(
            patrolPoints[
                selectedPoint
            ].position
        );
    }

    // =========================================================
    // ANIMAÇÃO
    // =========================================================

    private void UpdateAnimation()
    {
        if (animator == null ||
            agent == null)
            return;

        float speed =
            agent.velocity.magnitude;

        animator.SetFloat(
            "Speed",
            speed,
            0.1f,
            Time.deltaTime
        );
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmos()
    {
        // =====================================================
        // LINHA ATÉ O PLAYER
        // =====================================================

        if (player != null)
        {
            Gizmos.color =
                Color.red;

            Gizmos.DrawLine(
                transform.position +
                Vector3.up * 0.5f,

                player.position +
                Vector3.up * 0.5f
            );
        }

        // =====================================================
        // DISTÂNCIA PARA COMEÇAR O CHAMADO
        // =====================================================

        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            callChappieDistance
        );

        // =====================================================
        // RAIO DO NOISE SYSTEM
        // =====================================================

        Gizmos.color =
            Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            callNoiseRadius
        );
    }
}