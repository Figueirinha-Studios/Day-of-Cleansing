using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Investigate,
        Chase,
        LostSight,
        Search,
        RunAround
    }

    [Header("State")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("References")]
    public Transform player;

    private EnemyVision vision;
    private Generator generator;
    private NavMeshAgent agent;
    private Animator animator;
    private EnemyAudio enemyAudio;

    [Header("Detecção de Proximidade")]
    public float proximityDetectionRadius = 1f;

    [Tooltip("Layers das paredes e obstáculos.")]
    public LayerMask proximityObstacleMask;

    private bool wasPlayerInProximity = false;

    [Header("Patrol")]
    public Transform patrolPointsParent;

    private Transform[] patrolPoints;

    public float patrolSpeed = 5f;

    [Tooltip("Velocidade usada no Chase e no RunAround.")]
    public float chaseSpeed = 8f;

    [Tooltip("Quantidade de Patrol Points recentes que não poderão ser escolhidos novamente.")]
    [Min(0)]
    public int rememberedPatrolPoints = 5;

    [Tooltip("Quanto maior, mais ele evita voltar na direção de onde veio.")]
    [Range(-1f, 1f)]
    public float backtrackDotThreshold = -0.25f;

    private List<int> recentlyVisitedPoints = new List<int>();

    private int currentPoint = -1;
    private int previousPoint = -1;

    [Header("Memory")]
    public float memoryTime = 2f;

    private float memoryTimer;
    private Vector3 lastKnownPosition;

    [Header("Search")]
    public float searchTime = 30f;
    public float searchRadius = 20f;
    public float searchSpeed = 5f;

    private float searchTimer;

    private List<Transform> searchPoints = new List<Transform>();

    private int currentSearchIndex;

    private Vector3 searchCenter;

    [Header("Managers")]
    public MusicManager musicManager;

    public float searchMusicDuration = 8f;

    private float searchMusicTimer;

    [Header("Corrida Especial")]
    [SerializeField]
    private bool runAroundActive = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        vision = GetComponent<EnemyVision>();
        enemyAudio = GetComponent<EnemyAudio>();
        animator = GetComponent<Animator>();

        generator = FindFirstObjectByType<Generator>();

        LoadPatrolPoints();

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            ChooseNextPatrolPoint();
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        HandleGeneratorRunAround();

        HandleProximityDetection();

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Investigate:
                Investigate();
                break;

            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.LostSight:
                LostSight();
                break;

            case EnemyState.Search:
                Search();
                break;

            case EnemyState.RunAround:
                RunAround();
                break;
        }

        UpdateAnimation();
        UpdateFootsteps();
    }


    // =========================================================
    // ENTRAR NO CHASE
    // =========================================================

    private void EnterChase()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        currentState = EnemyState.Chase;

        agent.speed = chaseSpeed;

        if (player != null)
        {
            lastKnownPosition = player.position;
        }

        memoryTimer = memoryTime;

        // Toca a mesma música do Chase normal.
        if (musicManager != null)
        {
            musicManager.StartChaseMusic();
        }
    }


    // =========================================================
    // PATROL POINTS
    // =========================================================

    private void LoadPatrolPoints()
    {
        if (patrolPointsParent == null)
        {
            Debug.LogWarning("EnemyAI: Patrol Points Parent não foi definido.");

            patrolPoints = new Transform[0];

            return;
        }

        List<Transform> points = new List<Transform>();

        foreach (Transform child in patrolPointsParent)
        {
            points.Add(child);
        }

        patrolPoints = points.ToArray();

        Debug.Log(
            "EnemyAI: " +
            patrolPoints.Length +
            " Patrol Points encontrados."
        );
    }


    // =========================================================
    // ESCOLHER PRÓXIMO PONTO
    // =========================================================

    private void ChooseNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (agent == null || !agent.isOnNavMesh)
            return;

        List<int> availablePoints = new List<int>();

        // -----------------------------------------------------
        // Remove os últimos 5 pontos da escolha
        // -----------------------------------------------------

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (i == currentPoint)
                continue;

            if (recentlyVisitedPoints.Contains(i))
                continue;

            availablePoints.Add(i);
        }

        // -----------------------------------------------------
        // Caso não existam pontos disponíveis
        // -----------------------------------------------------

        if (availablePoints.Count == 0)
        {
            recentlyVisitedPoints.Clear();

            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (i != currentPoint)
                {
                    availablePoints.Add(i);
                }
            }
        }

        // -----------------------------------------------------
        // EVITA VOLTAR DIRETAMENTE
        // -----------------------------------------------------

        List<int> directionalPoints = new List<int>();

        if (previousPoint >= 0 &&
            currentPoint >= 0 &&
            previousPoint < patrolPoints.Length &&
            currentPoint < patrolPoints.Length)
        {
            Vector3 previousPosition =
                patrolPoints[previousPoint].position;

            Vector3 currentPosition =
                patrolPoints[currentPoint].position;

            Vector3 travelDirection =
                (currentPosition - previousPosition).normalized;

            foreach (int index in availablePoints)
            {
                Vector3 candidateDirection =
                    (patrolPoints[index].position - currentPosition).normalized;

                float dot =
                    Vector3.Dot(
                        travelDirection,
                        candidateDirection
                    );

                if (dot >= backtrackDotThreshold)
                {
                    directionalPoints.Add(index);
                }
            }
        }

        if (directionalPoints.Count > 0)
        {
            availablePoints = directionalPoints;
        }

        // -----------------------------------------------------
        // ESCOLHA ALEATÓRIA
        // -----------------------------------------------------

        int selectedPoint =
            availablePoints[
                Random.Range(
                    0,
                    availablePoints.Count
                )
            ];

        // -----------------------------------------------------
        // ATUALIZA MEMÓRIA
        // -----------------------------------------------------

        previousPoint = currentPoint;

        currentPoint = selectedPoint;

        recentlyVisitedPoints.Add(selectedPoint);

        // Mantém apenas os últimos 5.
        while (recentlyVisitedPoints.Count > rememberedPatrolPoints)
        {
            recentlyVisitedPoints.RemoveAt(0);
        }

        // -----------------------------------------------------
        // MOVE
        // -----------------------------------------------------

        agent.SetDestination(
            patrolPoints[selectedPoint].position
        );
    }


    // =========================================================
    // PATROL NORMAL
    // =========================================================

    private void Patrol()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.speed = patrolSpeed;

        // Não espera no ponto.
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            ChooseNextPatrolPoint();
        }

        // Visão normal.
        if (vision != null &&
            vision.CanSeePlayer())
        {
            EnterChase();
        }
    }


    // =========================================================
    // CORRIDA ESPECIAL
    // =========================================================

    private void RunAround()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        // Corre na mesma velocidade do Chase.
        agent.speed = chaseSpeed;

        // Chegou em um Patrol Point.
        // Não espera, escolhe outro imediatamente.
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            ChooseNextPatrolPoint();
        }

        // -----------------------------------------------------
        // VIU O PLAYER
        // -----------------------------------------------------

        if (vision != null &&
            vision.CanSeePlayer())
        {
            // Entra no Chase NORMAL.
            // Isso também inicia a música de Chase.
            EnterChase();
        }
    }


    // =========================================================
    // CHASE
    // =========================================================

    private void Chase()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.speed = chaseSpeed;

        if (player == null)
            return;

        // Continua vendo o jogador.
        if (vision != null &&
            vision.CanSeePlayer())
        {
            lastKnownPosition = player.position;

            memoryTimer = memoryTime;

            agent.SetDestination(player.position);
        }
        else
        {
            memoryTimer -= Time.deltaTime;

            if (memoryTimer <= 0f)
            {
                // Se o gerador ainda estiver na condição
                // de faltar exatamente 1 item,
                // volta para a corrida especial.
                if (runAroundActive)
                {
                    currentState = EnemyState.RunAround;

                    agent.speed = chaseSpeed;

                    ChooseNextPatrolPoint();
                }
                else
                {
                    currentState = EnemyState.LostSight;

                    agent.speed = searchSpeed;

                    agent.SetDestination(lastKnownPosition);
                }
            }
        }
    }


    // =========================================================
    // DETECÇÃO DE PROXIMIDADE
    // =========================================================

    private void HandleProximityDetection()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance > proximityDetectionRadius)
        {
            wasPlayerInProximity = false;

            return;
        }

        Vector3 origin =
            transform.position +
            Vector3.up * 0.5f;

        Vector3 target =
            player.position +
            Vector3.up * 0.5f;

        Vector3 direction =
            target - origin;

        float distanceToPlayer =
            direction.magnitude;

        // Verifica se existe parede entre os dois.
        if (Physics.Raycast(
            origin,
            direction.normalized,
            out RaycastHit hit,
            distanceToPlayer,
            proximityObstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            wasPlayerInProximity = false;

            return;
        }

        // Player está dentro de 1 metro e sem parede.
        if (!wasPlayerInProximity)
        {
            wasPlayerInProximity = true;

            EnterChase();
        }
    }


    // =========================================================
    // GERADOR / CORRIDA ESPECIAL
    // =========================================================

    private void HandleGeneratorRunAround()
    {
        if (generator == null)
        {
            generator =
                FindFirstObjectByType<Generator>();

            if (generator == null)
                return;
        }

        bool shouldRunAround =
            generator.IsExactlyOneItemMissing();

        // -----------------------------------------------------
        // COMEÇOU A FALTAR EXATAMENTE 1 ITEM
        // -----------------------------------------------------

        if (shouldRunAround &&
            !runAroundActive)
        {
            runAroundActive = true;

            // Se não estiver perseguindo,
            // começa a correr pelos pontos.
            if (currentState != EnemyState.Chase)
            {
                currentState =
                    EnemyState.RunAround;

                agent.speed =
                    chaseSpeed;

                ChooseNextPatrolPoint();
            }
        }

        // -----------------------------------------------------
        // ÚLTIMO ITEM FOI COLOCADO
        // -----------------------------------------------------

        else if (!shouldRunAround &&
                 runAroundActive)
        {
            runAroundActive = false;

            if (currentState ==
                EnemyState.RunAround)
            {
                currentState =
                    EnemyState.Patrol;

                agent.speed =
                    patrolSpeed;

                ChooseNextPatrolPoint();
            }
        }
    }


    // =========================================================
    // INVESTIGATE
    // =========================================================

    private void Investigate()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        agent.speed = searchSpeed;

        agent.SetDestination(lastKnownPosition);

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            StartSearch();
        }

        if (vision != null &&
            vision.CanSeePlayer())
        {
            EnterChase();
        }
    }


    // =========================================================
    // LOST SIGHT
    // =========================================================

    private void LostSight()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        agent.speed = searchSpeed;

        agent.SetDestination(lastKnownPosition);

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            StartSearch();
        }

        if (vision != null &&
            vision.CanSeePlayer())
        {
            EnterChase();
        }
    }


    // =========================================================
    // START SEARCH
    // =========================================================

    private void StartSearch()
    {
        currentState =
            EnemyState.Search;

        searchTimer =
            searchTime;

        searchCenter =
            lastKnownPosition;

        GenerateSearchPoints();

        currentSearchIndex = 0;

        if (searchPoints.Count > 0)
        {
            agent.speed =
                searchSpeed;

            agent.SetDestination(
                searchPoints[
                    currentSearchIndex
                ].position
            );
        }

        if (musicManager != null)
        {
            musicManager.StartSearchMusic();

            searchMusicTimer =
                searchMusicDuration;
        }
    }


    // =========================================================
    // GERAR PONTOS DE SEARCH
    // =========================================================

    private void GenerateSearchPoints()
    {
        searchPoints.Clear();

        int amount = 6;

        for (int i = 0; i < amount; i++)
        {
            Vector2 random =
                Random.insideUnitCircle *
                searchRadius;

            Vector3 point =
                searchCenter +
                new Vector3(
                    random.x,
                    0f,
                    random.y
                );

            if (NavMesh.SamplePosition(
                point,
                out NavMeshHit hit,
                5f,
                NavMesh.AllAreas))
            {
                GameObject searchObject =
                    new GameObject(
                        "SearchPoint"
                    );

                searchObject.transform.position =
                    hit.position;

                searchPoints.Add(
                    searchObject.transform
                );
            }
        }

        ShuffleSearchPoints();
    }


    // =========================================================
    // EMBARALHAR SEARCH
    // =========================================================

    private void ShuffleSearchPoints()
    {
        for (int i = 0;
             i < searchPoints.Count;
             i++)
        {
            int randomIndex =
                Random.Range(
                    i,
                    searchPoints.Count
                );

            Transform temp =
                searchPoints[i];

            searchPoints[i] =
                searchPoints[randomIndex];

            searchPoints[randomIndex] =
                temp;
        }
    }


    // =========================================================
    // SEARCH
    // =========================================================

    private void Search()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        searchTimer -=
            Time.deltaTime;

        if (searchTimer <= 0f)
        {
            foreach (Transform point
                     in searchPoints)
            {
                if (point != null)
                {
                    Destroy(
                        point.gameObject
                    );
                }
            }

            searchPoints.Clear();

            currentState =
                EnemyState.Patrol;

            agent.speed =
                patrolSpeed;

            ChooseNextPatrolPoint();

            return;
        }

        // Encontrou o jogador.
        if (vision != null &&
            vision.CanSeePlayer())
        {
            foreach (Transform point
                     in searchPoints)
            {
                if (point != null)
                {
                    Destroy(
                        point.gameObject
                    );
                }
            }

            searchPoints.Clear();

            EnterChase();

            return;
        }

        if (searchPoints.Count == 0)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            currentSearchIndex++;

            if (currentSearchIndex >=
                searchPoints.Count)
            {
                currentSearchIndex = 0;
            }

            if (searchPoints[
                currentSearchIndex] != null)
            {
                agent.SetDestination(
                    searchPoints[
                        currentSearchIndex
                    ].position
                );
            }
        }
    }


    // =========================================================
    // NOISE
    // =========================================================

    public void ReceiveNoise(
        Vector3 noisePosition)
    {
        // Durante Chase ou RunAround,
        // sons não interrompem o comportamento.
        if (currentState ==
            EnemyState.Chase ||
            currentState ==
            EnemyState.RunAround)
        {
            return;
        }

        lastKnownPosition =
            noisePosition;

        currentState =
            EnemyState.Investigate;

        memoryTimer =
            memoryTime;

        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.speed =
                searchSpeed;

            agent.SetDestination(
                noisePosition
            );
        }
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
    // PASSOS
    // =========================================================

    private void UpdateFootsteps()
    {
        if (enemyAudio == null ||
            agent == null)
            return;

        bool isMoving =
            agent.velocity.magnitude >
            0.1f;

        bool isChasing =
            currentState ==
            EnemyState.Chase;

        enemyAudio.UpdateFootsteps(
            isMoving,
            isChasing
        );
    }


    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmos()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            proximityDetectionRadius
        );

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
    }
}