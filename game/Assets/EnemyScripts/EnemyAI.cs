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

    // =========================================================
    // STEALTH
    // =========================================================

    [Header("Stealth Scare")]
    public StealthScare stealthScare;

    private bool stealthControlled = false;

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

    private List<int> recentlyVisitedPoints =
        new List<int>();

    private int currentPoint = -1;
    private int previousPoint = -1;

    [Header("Movimento Natural")]
    [Tooltip("Aceleração usada pelo Chappie.")]
    public float movementAcceleration = 8f;

    [Tooltip("Velocidade máxima de rotação do Chappie.")]
    public float rotationSpeed = 180f;

    [Tooltip("Distância mínima para o Chappie desacelerar antes de um ponto.")]
    public float naturalStoppingDistance = 0.5f;

    // =========================================================
    // OLHAR AO REDOR
    // =========================================================

    [Header("Olhar ao Redor")]
    [Tooltip("A cada quantos Patrol Points o Chappie para para olhar.")]
    public int patrolPointsBeforeLookAround = 10;

    [Tooltip("Ângulo que o Chappie gira para cada lado.")]
    public float lookAroundAngle = 60f;

    [Tooltip("Velocidade da rotação durante o olhar ao redor.")]
    public float lookAroundRotationSpeed = 120f;

    [Tooltip("Tempo parado olhando para cada lado.")]
    public float lookAroundSidePause = 0.3f;

    [Tooltip("Tempo parado depois de voltar ao centro.")]
    public float lookAroundCenterPause = 0.25f;

    private int patrolPointsVisited = 0;

    private bool lookingAround = false;

    private Coroutine lookAroundCoroutine;

    [Header("Memory")]
    public float memoryTime = 2f;

    [Tooltip("Tempo que o Chappie segue a posição atual do player após chegar ao último ponto conhecido.")]
    public float lastKnownPositionKnowledgeTime = 1f;

    private float memoryTimer;
    private Vector3 lastKnownPosition;

    private float knowledgeTimer;
    private bool followingLastKnownPlayer = false;

    [Header("Search")]
    public float searchTime = 30f;
    public float searchRadius = 20f;
    public float searchSpeed = 5f;

    private float searchTimer;

    private List<Transform> searchPoints =
        new List<Transform>();

    private int currentSearchIndex;
    private Vector3 searchCenter;

    private bool searchingForPlayer = false;

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

        if (stealthScare == null)
        {
            stealthScare =
                GetComponent<StealthScare>();
        }

        ConfigureNaturalMovement();

        LoadPatrolPoints();

        if (patrolPoints != null &&
            patrolPoints.Length > 0)
        {
            ChooseNextPatrolPoint();
        }
    }

    // =========================================================
    // CONFIGURAÇÃO DO MOVIMENTO
    // =========================================================

    private void ConfigureNaturalMovement()
    {
        if (agent == null)
            return;

        agent.acceleration =
            movementAcceleration;

        agent.angularSpeed =
            rotationSpeed;

        agent.autoBraking = false;

        agent.stoppingDistance =
            naturalStoppingDistance;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // =====================================================
        // STEALTH
        // =====================================================

        if (stealthControlled)
        {
            UpdateAnimation();
            UpdateFootsteps();
            return;
        }

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
    // STEALTH - CONTROLE TEMPORÁRIO
    // =========================================================

    public void BeginStealthControl()
    {
        stealthControlled = true;

        StopLookAround();

        if (musicManager != null)
        {
            musicManager.StopEnemyMusic();
        }

        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.speed = chaseSpeed;
        }
    }

    public void EndStealthControl()
    {
        stealthControlled = false;
    }

    public bool IsStealthControlled()
    {
        return stealthControlled;
    }

    public void ForceChase()
    {
        stealthControlled = false;

        EnterChase();
    }

    // =========================================================
    // CHASE
    // =========================================================

    private void EnterChase()
    {
        if (stealthControlled)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        StopLookAround();

        currentState =
            EnemyState.Chase;

        agent.isStopped = false;
        agent.updateRotation = true;

        agent.speed =
            chaseSpeed;

        if (player != null)
        {
            lastKnownPosition =
                player.position;
        }

        followingLastKnownPlayer = false;
        knowledgeTimer = 0f;

        memoryTimer =
            memoryTime;

        searchingForPlayer =
            true;

        if (musicManager != null)
        {
            musicManager.StartChaseMusic();
        }
    }

    private void ExitChase()
    {
        if (musicManager != null)
        {
            musicManager.StopEnemyMusic();
        }
    }

    private void StopEnemyMusic()
    {
        if (musicManager != null)
        {
            musicManager.StopEnemyMusic();
        }
    }

    private void Chase()
    {
        if (stealthControlled)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        agent.speed =
            chaseSpeed;

        if (player == null)
            return;

        if (vision != null &&
            vision.CanSeePlayer())
        {
            lastKnownPosition =
                player.position;

            memoryTimer =
                memoryTime;

            agent.SetDestination(
                player.position
            );
        }
        else
        {
            memoryTimer -=
                Time.deltaTime;

            if (memoryTimer <= 0f)
            {
                searchingForPlayer =
                    true;

                followingLastKnownPlayer =
                    false;

                currentState =
                    EnemyState.LostSight;

                agent.speed =
                    searchSpeed;

                agent.SetDestination(
                    lastKnownPosition
                );
            }
        }
    }

    // =========================================================
    // PROXIMIDADE
    // =========================================================

    private void HandleProximityDetection()
    {
        if (stealthControlled)
            return;

        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance >
            proximityDetectionRadius)
        {
            wasPlayerInProximity =
                false;

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

        if (Physics.Raycast(
            origin,
            direction.normalized,
            out RaycastHit hit,
            distanceToPlayer,
            proximityObstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            wasPlayerInProximity =
                false;

            return;
        }

        if (!wasPlayerInProximity)
        {
            wasPlayerInProximity =
                true;

            EnterChase();
        }
    }

    // =========================================================
    // GENERATOR / RUN AROUND
    // =========================================================

    private void HandleGeneratorRunAround()
    {
        if (stealthControlled)
            return;

        if (generator == null)
        {
            generator =
                FindFirstObjectByType<Generator>();

            if (generator == null)
                return;
        }

        bool exactlyOneMissing =
            generator.IsExactlyOneItemMissing();

        bool generatorOn =
            generator.IsGeneratorOn();

        if (exactlyOneMissing &&
            !runAroundActive)
        {
            runAroundActive =
                true;

            if (currentState !=
                EnemyState.Chase)
            {
                StopEnemyMusic();

                currentState =
                    EnemyState.RunAround;

                if (agent != null &&
                    agent.isOnNavMesh)
                {
                    agent.speed =
                        chaseSpeed;

                    agent.isStopped = false;
                    agent.updateRotation = true;

                    ChooseNextPatrolPoint();
                }
            }

            return;
        }

        if (generatorOn)
        {
            runAroundActive =
                true;

            if (currentState ==
                EnemyState.Patrol)
            {
                StopEnemyMusic();

                currentState =
                    EnemyState.RunAround;

                if (agent != null &&
                    agent.isOnNavMesh)
                {
                    agent.speed =
                        chaseSpeed;

                    agent.isStopped = false;
                    agent.updateRotation = true;

                    ChooseNextPatrolPoint();
                }
            }

            return;
        }
    }

    private void RunAround()
    {
        if (stealthControlled)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        agent.speed =
            chaseSpeed;

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            ChooseNextPatrolPoint();
        }

        if (vision != null &&
            vision.CanSeePlayer())
        {
            EnterChase();
        }
    }

    // =========================================================
    // PATROL
    // =========================================================

    private void LoadPatrolPoints()
    {
        if (patrolPointsParent == null)
        {
            Debug.LogWarning(
                "EnemyAI: Patrol Points Parent não foi definido."
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
            "EnemyAI: " +
            patrolPoints.Length +
            " Patrol Points encontrados."
        );
    }

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

        List<int> directionalPoints =
            new List<int>();

        if (previousPoint >= 0 &&
            currentPoint >= 0 &&
            previousPoint < patrolPoints.Length &&
            currentPoint < patrolPoints.Length)
        {
            Vector3 previousPosition =
                patrolPoints[
                    previousPoint
                ].position;

            Vector3 currentPosition =
                patrolPoints[
                    currentPoint
                ].position;

            Vector3 travelDirection =
                (
                    currentPosition -
                    previousPosition
                ).normalized;

            foreach (int index
                     in availablePoints)
            {
                Vector3 candidateDirection =
                    (
                        patrolPoints[index].position -
                        currentPosition
                    ).normalized;

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
            availablePoints =
                directionalPoints;
        }

        int selectedPoint =
            availablePoints[
                Random.Range(
                    0,
                    availablePoints.Count
                )
            ];

        previousPoint =
            currentPoint;

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

        agent.SetDestination(
            patrolPoints[
                selectedPoint
            ].position
        );
    }

    private void Patrol()
    {
        if (stealthControlled)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        if (lookingAround)
            return;

        agent.speed =
            patrolSpeed;

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            patrolPointsVisited++;

            Debug.Log(
                "CHAPPIE: Patrol Point visitado: " +
                patrolPointsVisited
            );

            if (patrolPointsBeforeLookAround > 0 &&
                patrolPointsVisited %
                patrolPointsBeforeLookAround == 0)
            {
                StartLookAround();
            }
            else
            {
                ChooseNextPatrolPoint();
            }
        }

        if (vision != null &&
            vision.CanSeePlayer())
        {
            EnterChase();
        }
    }

    // =========================================================
    // OLHAR AO REDOR
    // =========================================================

    private void StartLookAround()
    {
        if (lookingAround)
            return;

        if (currentState !=
            EnemyState.Patrol)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        lookingAround = true;

        if (lookAroundCoroutine != null)
        {
            StopCoroutine(
                lookAroundCoroutine
            );
        }

        lookAroundCoroutine =
            StartCoroutine(
                LookAroundRoutine()
            );
    }

    private IEnumerator LookAroundRoutine()
    {
        Quaternion centerRotation =
            transform.rotation;

        agent.isStopped = true;

        agent.updateRotation = false;

        Quaternion leftRotation =
            centerRotation *
            Quaternion.Euler(
                0f,
                -lookAroundAngle,
                0f
            );

        yield return RotateToLook(
            leftRotation
        );

        yield return new WaitForSeconds(
            lookAroundSidePause
        );

        yield return RotateToLook(
            centerRotation
        );

        yield return new WaitForSeconds(
            lookAroundCenterPause
        );

        Quaternion rightRotation =
            centerRotation *
            Quaternion.Euler(
                0f,
                lookAroundAngle,
                0f
            );

        yield return RotateToLook(
            rightRotation
        );

        yield return new WaitForSeconds(
            lookAroundSidePause
        );

        yield return RotateToLook(
            centerRotation
        );

        yield return new WaitForSeconds(
            lookAroundCenterPause
        );

        transform.rotation =
            centerRotation;

        agent.updateRotation = true;
        agent.isStopped = false;

        lookingAround = false;
        lookAroundCoroutine = null;

        if (currentState ==
            EnemyState.Patrol)
        {
            ChooseNextPatrolPoint();
        }
    }

    private IEnumerator RotateToLook(
        Quaternion targetRotation
    )
    {
        while (
            Quaternion.Angle(
                transform.rotation,
                targetRotation
            ) > 0.5f
        )
        {
            if (currentState !=
                EnemyState.Patrol)
            {
                yield break;
            }

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    lookAroundRotationSpeed *
                    Time.deltaTime
                );

            yield return null;
        }

        transform.rotation =
            targetRotation;
    }

    private void StopLookAround()
    {
        if (!lookingAround)
            return;

        if (lookAroundCoroutine != null)
        {
            StopCoroutine(
                lookAroundCoroutine
            );

            lookAroundCoroutine = null;
        }

        lookingAround = false;

        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
        }
    }

    // =========================================================
    // INVESTIGATE
    // =========================================================

    private void Investigate()
    {
        if (stealthControlled)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        searchingForPlayer =
            false;

        agent.speed =
            searchSpeed;

        agent.SetDestination(
            lastKnownPosition
        );

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
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
        if (stealthControlled)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        if (player == null)
            return;

        agent.speed =
            searchSpeed;

        if (!followingLastKnownPlayer)
        {
            agent.SetDestination(
                lastKnownPosition
            );

            if (!agent.pathPending &&
                agent.remainingDistance <=
                agent.stoppingDistance)
            {
                followingLastKnownPlayer =
                    true;

                knowledgeTimer =
                    lastKnownPositionKnowledgeTime;

                agent.SetDestination(
                    player.position
                );
            }

            if (vision != null &&
                vision.CanSeePlayer())
            {
                followingLastKnownPlayer =
                    false;

                EnterChase();
            }

            return;
        }

        knowledgeTimer -=
            Time.deltaTime;

        lastKnownPosition =
            player.position;

        agent.SetDestination(
            player.position
        );

        if (vision != null &&
            vision.CanSeePlayer())
        {
            followingLastKnownPlayer =
                false;

            EnterChase();

            return;
        }

        if (knowledgeTimer <= 0f)
        {
            followingLastKnownPlayer =
                false;

            lastKnownPosition =
                player.position;

            searchingForPlayer =
                true;

            StartSearch();
        }
    }

    // =========================================================
    // SEARCH
    // =========================================================

    private void StartSearch()
    {
        if (stealthControlled)
            return;

        currentState =
            EnemyState.Search;

        searchTimer =
            searchTime;

        searchCenter =
            lastKnownPosition;

        GenerateSearchPoints();

        currentSearchIndex =
            0;

        if (agent != null &&
            agent.isOnNavMesh &&
            searchPoints.Count > 0)
        {
            agent.speed =
                searchSpeed;

            agent.SetDestination(
                searchPoints[
                    currentSearchIndex
                ].position
            );
        }

        if (searchingForPlayer)
        {
            if (musicManager != null)
            {
                musicManager.StopEnemyMusic();

                musicManager.StartSearchMusic();

                searchMusicTimer =
                    searchMusicDuration;
            }
        }
    }

    private void Search()
    {
        if (stealthControlled)
            return;

        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        searchTimer -=
            Time.deltaTime;

        if (searchTimer <= 0f)
        {
            ClearSearchPoints();

            if (searchingForPlayer)
            {
                StopEnemyMusic();
            }

            searchingForPlayer =
                false;

            if (runAroundActive)
            {
                currentState =
                    EnemyState.RunAround;

                if (agent != null &&
                    agent.isOnNavMesh)
                {
                    agent.speed =
                        chaseSpeed;

                    agent.isStopped = false;
                    agent.updateRotation = true;

                    ChooseNextPatrolPoint();
                }

                return;
            }

            currentState =
                EnemyState.Patrol;

            agent.speed =
                patrolSpeed;

            agent.isStopped = false;
            agent.updateRotation = true;

            ChooseNextPatrolPoint();

            return;
        }

        if (vision != null &&
            vision.CanSeePlayer())
        {
            ClearSearchPoints();

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
                currentSearchIndex =
                    0;
            }

            if (searchPoints[
                currentSearchIndex
            ] != null)
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
    // SEARCH POINTS
    // =========================================================

    private void GenerateSearchPoints()
    {
        searchPoints.Clear();

        int amount = 6;

        for (int i = 0;
             i < amount;
             i++)
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

    private void ClearSearchPoints()
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
    }

    // =========================================================
    // NOISE
    // =========================================================

    public void ReceiveNoise(
        Vector3 noisePosition
    )
    {
        if (stealthControlled)
            return;

        if (currentState ==
            EnemyState.Chase)
        {
            return;
        }

        StopLookAround();

        followingLastKnownPlayer =
            false;

        knowledgeTimer =
            0f;

        searchingForPlayer =
            false;

        if (musicManager != null)
        {
            musicManager.StopEnemyMusic();
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

            agent.isStopped = false;
            agent.updateRotation = true;

            agent.SetDestination(
                noisePosition
            );
        }
    }

    // =========================================================
    // ANIMATION
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
    // FOOTSTEPS
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
    // GIZMOS
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