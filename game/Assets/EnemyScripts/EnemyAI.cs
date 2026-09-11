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

    [Header("Memory")]
    public float memoryTime = 2f;

    [Tooltip("Tempo que o Chappie segue a posição atual do player após chegar ao último ponto conhecido.")]
    public float lastKnownPositionKnowledgeTime = 1f;

    private float memoryTimer;
    private Vector3 lastKnownPosition;

    /*
     * Controla o período especial de 1 segundo
     * após chegar ao último ponto conhecido.
     */
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

    /*
     * TRUE:
     * Está procurando o jogador.
     *
     * FALSE:
     * Está investigando apenas um barulho/objeto.
     */
    private bool searchingForPlayer = false;

    [Header("Managers")]
    public MusicManager musicManager;

    /*
     * Mantido para não quebrar referências
     * já existentes no Inspector.
     */
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

        /*
         * Aceleração mais suave.
         */
        agent.acceleration =
            movementAcceleration;

        /*
         * Rotação gradual.
         */
        agent.angularSpeed =
            rotationSpeed;

        /*
         * Não precisa frear bruscamente ao chegar
         * nos Patrol Points.
         */
        agent.autoBraking = false;

        /*
         * Pequena distância para evitar que ele tente
         * encaixar exatamente no centro do ponto.
         */
        agent.stoppingDistance =
            naturalStoppingDistance;
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
    // CHASE
    // =========================================================

    private void EnterChase()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        /*
         * Se estava investigando um objeto ou procurando,
         * agora passa a procurar o jogador.
         */
        currentState =
            EnemyState.Chase;

        agent.speed =
            chaseSpeed;

        if (player != null)
        {
            lastKnownPosition =
                player.position;
        }

        /*
         * Reseta o período especial de 1 segundo.
         */
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
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        agent.speed =
            chaseSpeed;

        if (player == null)
            return;

        /*
         * Continua vendo o jogador.
         */
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
            /*
             * Perdeu o jogador.
             */
            memoryTimer -=
                Time.deltaTime;

            if (memoryTimer <= 0f)
            {
                /*
                 * Chase -> LostSight -> Search
                 *
                 * A decisão de voltar para RunAround
                 * acontecerá somente depois que o Search
                 * terminar.
                 */
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

                /*
                 * NÃO paramos a música de Chase.
                 *
                 * Ela continua durante LostSight.
                 */
            }
        }
    }


    // =========================================================
    // PROXIMIDADE
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

        /*
         * =====================================================
         * 2/3
         * =====================================================
         *
         * Quando faltar exatamente 1 item,
         * começa o RunAround.
         */
        if (exactlyOneMissing &&
            !runAroundActive)
        {
            runAroundActive =
                true;

            /*
             * Se já estiver em Chase,
             * não interrompe o Chase.
             */
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

                    ChooseNextPatrolPoint();
                }
            }

            return;
        }

        /*
         * =====================================================
         * GENERATOR ON
         * =====================================================
         *
         * Generator ON NÃO encerra o RunAround.
         *
         * O Chappie continua correndo.
         *
         * Se ele estiver em Chase, Investigate, LostSight
         * ou Search, deixa o estado atual terminar normalmente.
         *
         * Quando voltar ao estado de movimento normal,
         * continuará em RunAround.
         */
        if (generatorOn)
        {
            /*
             * Garante que o RunAround continue ativo.
             */
            runAroundActive =
                true;

            /*
             * Se estiver em Patrol, muda para RunAround.
             */
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

                    ChooseNextPatrolPoint();
                }
            }

            return;
        }

        /*
         * =====================================================
         * 3/3
         * =====================================================
         *
         * Depois que RunAround começou em 2/3,
         * ele continua ativo em 3/3.
         *
         * Não fazemos nada aqui para desligá-lo.
         */
    }


    private void RunAround()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        agent.speed =
            chaseSpeed;

        /*
         * Continua correndo pelos Patrol Points.
         */
        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            ChooseNextPatrolPoint();
        }

        /*
         * Se enxergar o jogador:
         *
         * RunAround -> Chase
         */
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
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        agent.speed =
            patrolSpeed;

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
    // INVESTIGATE OBJETO
    // =========================================================

    private void Investigate()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        /*
         * Aqui NÃO estamos procurando o jogador.
         *
         * É somente investigação de barulho.
         */
        searchingForPlayer =
            false;

        agent.speed =
            searchSpeed;

        agent.SetDestination(
            lastKnownPosition
        );

        /*
         * Chegou ao local do objeto.
         */
        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            StartSearch();
        }

        /*
         * Se enxergar o jogador durante a investigação:
         *
         * Investigate -> Chase
         */
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

        if (player == null)
            return;

        agent.speed =
            searchSpeed;

        /*
         * =====================================================
         * FASE 1
         * =====================================================
         *
         * Vai até o último ponto onde viu o player.
         */
        if (!followingLastKnownPlayer)
        {
            agent.SetDestination(
                lastKnownPosition
            );

            /*
             * Chegou ao último ponto conhecido.
             *
             * Agora começa o período de 1 segundo.
             */
            if (!agent.pathPending &&
                agent.remainingDistance <=
                agent.stoppingDistance)
            {
                followingLastKnownPlayer =
                    true;

                knowledgeTimer =
                    lastKnownPositionKnowledgeTime;

                /*
                 * Começa imediatamente a seguir
                 * a posição atual do player.
                 */
                agent.SetDestination(
                    player.position
                );
            }

            /*
             * Se encontrar o player antes de chegar
             * ao último ponto:
             *
             * LostSight -> Chase
             */
            if (vision != null &&
                vision.CanSeePlayer())
            {
                followingLastKnownPlayer =
                    false;

                EnterChase();
            }

            return;
        }

        /*
         * =====================================================
         * FASE 2
         * =====================================================
         *
         * Durante 1 segundo:
         *
         * O Chappie sabe onde o player está e
         * continua seguindo a posição atual dele,
         * mesmo sem enxergá-lo.
         */
        knowledgeTimer -=
            Time.deltaTime;

        /*
         * Atualiza a posição do player constantemente.
         */
        lastKnownPosition =
            player.position;

        agent.SetDestination(
            player.position
        );

        /*
         * Se encontrar o player durante esse 1 segundo:
         *
         * LostSight -> Chase
         */
        if (vision != null &&
            vision.CanSeePlayer())
        {
            followingLastKnownPlayer =
                false;

            EnterChase();

            return;
        }

        /*
         * =====================================================
         * ACABOU O 1 SEGUNDO
         * =====================================================
         *
         * Se não encontrou o player:
         *
         * LostSight -> Search
         */
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

        /*
         * =====================================================
         * SEARCH DE PLAYER
         * =====================================================
         *
         * Só toca Search Music se:
         *
         * searchingForPlayer == true
         */
        if (searchingForPlayer)
        {
            if (musicManager != null)
            {
                /*
                 * Sai Chase Music
                 * e entra Search Music.
                 */
                musicManager.StopEnemyMusic();

                musicManager.StartSearchMusic();

                searchMusicTimer =
                    searchMusicDuration;
            }
        }

        /*
         * Se searchingForPlayer == false:
         *
         * É investigação de objeto.
         *
         * NÃO toca Search Music.
         */
    }


    private void Search()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        searchTimer -=
            Time.deltaTime;

        /*
         * =====================================================
         * SEARCH TERMINOU
         * =====================================================
         */
        if (searchTimer <= 0f)
        {
            ClearSearchPoints();

            /*
             * Para Search Music somente se era
             * Search do jogador.
             */
            if (searchingForPlayer)
            {
                StopEnemyMusic();
            }

            searchingForPlayer =
                false;

            /*
             * =================================================
             * RUNAROUND
             * =================================================
             *
             * Se o RunAround estava ativo,
             * continua nele.
             *
             * Isso vale tanto para:
             *
             * 2/3
             * 3/3
             * Generator ON
             */
            if (runAroundActive)
            {
                currentState =
                    EnemyState.RunAround;

                if (agent != null &&
                    agent.isOnNavMesh)
                {
                    agent.speed =
                        chaseSpeed;

                    ChooseNextPatrolPoint();
                }

                return;
            }

            /*
             * =================================================
             * PATROL
             * =================================================
             */
            currentState =
                EnemyState.Patrol;

            agent.speed =
                patrolSpeed;

            ChooseNextPatrolPoint();

            return;
        }

        /*
         * =====================================================
         * ENCONTROU PLAYER DURANTE SEARCH
         * =====================================================
         */
        if (vision != null &&
            vision.CanSeePlayer())
        {
            ClearSearchPoints();

            EnterChase();

            return;
        }

        if (searchPoints.Count == 0)
            return;

        /*
         * =====================================================
         * PRÓXIMO PONTO
         * =====================================================
         */
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
        /*
         * Não ignoramos o barulho durante RunAround.
         *
         * RunAround + objeto
         * -> Investigate
         *
         * Chase continua ignorando objetos porque ele
         * já está perseguindo o jogador.
         */

        if (currentState ==
            EnemyState.Chase)
        {
            return;
        }

        /*
         * Se estava no período especial de 1 segundo,
         * o barulho interrompe esse comportamento.
         */
        followingLastKnownPlayer =
            false;

        knowledgeTimer =
            0f;

        searchingForPlayer =
            false;

        /*
         * Como agora é investigação de objeto,
         * qualquer Search Music que esteja tocando
         * deve parar.
         */
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
