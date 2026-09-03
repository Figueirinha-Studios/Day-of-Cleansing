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

    [Header("Detecção de Proximidade")]
    [Tooltip("Distância necessária para o Chappie detectar o player mesmo sem visão normal.")]
    public float proximityDetectionRadius = 1f;

    [Tooltip("Layers que representam paredes/obstáculos.")]
    public LayerMask proximityObstacleMask;

    private bool wasPlayerInProximity = false;

    [Header("Patrol")]
    public Transform patrolPointsParent;

    private Transform[] patrolPoints;

    public float patrolSpeed = 5f;

    [Tooltip("Velocidade usada tanto no Chase quanto na corrida especial.")]
    public float chaseSpeed = 8f;

    public float waitTime = 2f;

    [Header("Memory")]
    public float memoryTime = 2f;

    private float memoryTimer;

    private NavMeshAgent agent;

    private int currentPoint = 0;

    private float waitTimer;

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

    private Animator animator;

    private EnemyAudio enemyAudio;

    [Header("Corrida Especial")]
    [Tooltip("Indica se o Chappie está atualmente na corrida especial.")]
    private bool runAroundActive = false;


    // ============================================================
    // START
    // ============================================================

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        vision = GetComponent<EnemyVision>();

        enemyAudio = GetComponent<EnemyAudio>();

        animator = GetComponent<Animator>();

        generator = FindFirstObjectByType<Generator>();

        agent.speed = patrolSpeed;

        if (patrolPointsParent != null)
        {
            patrolPoints = new Transform[
                patrolPointsParent.childCount
            ];

            for (int i = 0; i < patrolPointsParent.childCount; i++)
            {
                patrolPoints[i] =
                    patrolPointsParent.GetChild(i);
            }
        }

        if (patrolPoints != null &&
            patrolPoints.Length > 0)
        {
            agent.SetDestination(
                patrolPoints[currentPoint].position
            );
        }
    }


    // ============================================================
    // UPDATE
    // ============================================================

    void Update()
    {
        HandleGeneratorRunAround();

        HandleProximityDetection();

        switch (currentState)
        {
            case EnemyState.Patrol:

                Patrol();

                if (vision != null &&
                    vision.CanSeePlayer())
                {
                    lastKnownPosition =
                        player.position;

                    currentState =
                        EnemyState.Chase;

                    if (musicManager != null)
                    {
                        musicManager.StartChaseMusic();
                    }
                }

                break;


            case EnemyState.Chase:

                Chase();

                if (vision != null &&
                    !vision.CanSeePlayer())
                {
                    if (!vision.IsVisionStillActive())
                    {
                        /*
                         * Se estava na corrida especial,
                         * NÃO entra em LostSight/Search.
                         *
                         * Ele simplesmente volta a correr
                         * pelos Patrol Points.
                         */
                        if (runAroundActive)
                        {
                            currentState =
                                EnemyState.RunAround;

                            agent.speed =
                                chaseSpeed;

                            SetRandomRunPoint();
                        }
                        else
                        {
                            currentState =
                                EnemyState.LostSight;

                            memoryTimer =
                                memoryTime;

                            lastKnownPosition =
                                player.position;
                        }
                    }
                }

                break;


            case EnemyState.LostSight:

                LostSight();

                break;


            case EnemyState.Search:

                Search();

                if (vision != null &&
                    vision.CanSeePlayer())
                {
                    currentState =
                        EnemyState.Chase;

                    if (musicManager != null)
                    {
                        musicManager.StartChaseMusic();
                    }
                }

                break;


            case EnemyState.Investigate:

                Investigate();

                break;


            case EnemyState.RunAround:

                RunAround();

                break;
        }


        // ========================================================
        // ANIMAÇÃO
        // ========================================================

        float currentSpeed =
            agent.velocity.magnitude;

        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                currentSpeed
            );
        }


        // ========================================================
        // PASSOS
        // ========================================================

        bool isMoving =
            currentSpeed > 0.1f;

        bool isChasing =
            currentState == EnemyState.Chase;

        if (enemyAudio != null)
        {
            enemyAudio.UpdateFootsteps(
                isMoving,
                isChasing
            );
        }
    }


    // ============================================================
    // VERIFICA CORRIDA ESPECIAL
    // ============================================================

    void HandleGeneratorRunAround()
    {
        if (generator == null)
        {
            generator =
                FindFirstObjectByType<Generator>();

            return;
        }


        bool shouldRunAround =
            generator.IsExactlyOneItemMissing();


        // --------------------------------------------------------
        // COMEÇA A CORRIDA
        // --------------------------------------------------------

        if (shouldRunAround)
        {
            if (!runAroundActive)
            {
                runAroundActive = true;

                /*
                 * Só muda para RunAround se não estiver
                 * atualmente perseguindo o player.
                 *
                 * Se já estiver em Chase, continua em Chase.
                 */
                if (currentState != EnemyState.Chase)
                {
                    currentState =
                        EnemyState.RunAround;

                    agent.speed =
                        chaseSpeed;

                    SetRandomRunPoint();
                }

                Debug.Log(
                    "CHAPPIE: Falta exatamente 1 item. Corrida especial ativada."
                );
            }

            return;
        }


        // --------------------------------------------------------
        // TERMINA A CORRIDA
        // --------------------------------------------------------

        if (runAroundActive)
        {
            runAroundActive = false;


            /*
             * Se estiver perseguindo o player, não interrompe
             * o Chase imediatamente.
             *
             * Caso contrário, volta ao Patrol normal.
             */
            if (currentState == EnemyState.RunAround)
            {
                currentState =
                    EnemyState.Patrol;

                agent.speed =
                    patrolSpeed;

                if (patrolPoints != null &&
                    patrolPoints.Length > 0)
                {
                    agent.SetDestination(
                        patrolPoints[currentPoint].position
                    );
                }
            }

            Debug.Log(
                "CHAPPIE: Corrida especial encerrada."
            );
        }
    }


    // ============================================================
    // DETECÇÃO DE PROXIMIDADE
    // ============================================================

    void HandleProximityDetection()
    {
        if (player == null)
            return;


        Vector3 origin =
            transform.position;

        Vector3 target =
            player.position;


        float distance =
            Vector3.Distance(
                origin,
                target
            );


        /*
         * Fora do raio de 1 metro:
         * não faz nada.
         */
        if (distance >
            proximityDetectionRadius)
        {
            wasPlayerInProximity =
                false;

            return;
        }


        Vector3 direction =
            target - origin;


        /*
         * Verifica se existe uma parede entre
         * o Chappie e o player.
         */
        bool blocked =
            Physics.Raycast(
                origin,
                direction.normalized,
                out RaycastHit hit,
                distance,
                proximityObstacleMask,
                QueryTriggerInteraction.Ignore
            );


        /*
         * Existe parede.
         */
        if (blocked)
        {
            wasPlayerInProximity =
                false;

            return;
        }


        /*
         * Player está realmente dentro de 1 metro
         * e não existe parede.
         */
        if (!wasPlayerInProximity)
        {
            lastKnownPosition =
                player.position;


            currentState =
                EnemyState.Chase;


            /*
             * Durante o Chase, usa chaseSpeed.
             */
            agent.speed =
                chaseSpeed;


            if (musicManager != null)
            {
                musicManager.StartChaseMusic();
            }


            Debug.Log(
                "CHAPPIE: Player entrou no raio de proximidade! CHASE!"
            );
        }


        wasPlayerInProximity =
            true;
    }


    // ============================================================
    // RUN AROUND
    // ============================================================

    void RunAround()
    {
        /*
         * Segurança:
         * se por algum motivo o estado mudou,
         * não executa essa função.
         */
        if (currentState !=
            EnemyState.RunAround)
        {
            return;
        }


        /*
         * A corrida especial usa EXATAMENTE
         * a velocidade do Chase.
         */
        agent.speed =
            chaseSpeed;


        /*
         * Não usa waitTime.
         *
         * Assim que chegar no ponto,
         * escolhe outro imediatamente.
         */
        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            SetRandomRunPoint();
        }
    }


    // ============================================================
    // ESCOLHER NOVO PONTO
    // ============================================================

    void SetRandomRunPoint()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }


        int nextPoint;


        /*
         * Escolhe um Patrol Point aleatório,
         * evitando repetir o atual quando existem
         * pelo menos dois pontos.
         */
        do
        {
            nextPoint =
                Random.Range(
                    0,
                    patrolPoints.Length
                );

        } while (
            patrolPoints.Length > 1 &&
            nextPoint == currentPoint
        );


        currentPoint =
            nextPoint;


        agent.speed =
            chaseSpeed;


        agent.SetDestination(
            patrolPoints[currentPoint].position
        );
    }


    // ============================================================
    // PATROL NORMAL
    // ============================================================

    void Patrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }


        agent.speed =
            patrolSpeed;


        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            waitTimer +=
                Time.deltaTime;


            if (waitTimer >= waitTime)
            {
                int nextPoint;


                do
                {
                    nextPoint =
                        Random.Range(
                            0,
                            patrolPoints.Length
                        );

                } while (
                    patrolPoints.Length > 1 &&
                    nextPoint == currentPoint
                );


                currentPoint =
                    nextPoint;


                agent.SetDestination(
                    patrolPoints[currentPoint].position
                );


                waitTimer = 0f;
            }
        }
    }


    // ============================================================
    // CHASE
    // ============================================================

    void Chase()
    {
        /*
         * Chase usa chaseSpeed.
         */
        agent.speed =
            chaseSpeed;


        if (vision != null &&
            vision.CanSeePlayer())
        {
            lastKnownPosition =
                player.position;
        }


        agent.SetDestination(
            lastKnownPosition
        );
    }


    // ============================================================
    // LOST SIGHT
    // ============================================================

    void LostSight()
    {
        agent.speed =
            chaseSpeed;


        agent.SetDestination(
            lastKnownPosition
        );


        if (vision != null &&
            vision.CanSeePlayer())
        {
            currentState =
                EnemyState.Chase;

            if (musicManager != null)
            {
                musicManager.StartChaseMusic();
            }

            return;
        }


        memoryTimer -=
            Time.deltaTime;


        if (memoryTimer <= 0)
        {
            StartSearch();
        }
    }


    // ============================================================
    // INVESTIGATE
    // ============================================================

    void Investigate()
    {
        agent.speed =
            searchSpeed;


        agent.SetDestination(
            lastKnownPosition
        );


        if (vision != null &&
            vision.CanSeePlayer())
        {
            currentState =
                EnemyState.Chase;

            if (musicManager != null)
            {
                musicManager.StartChaseMusic();
            }

            return;
        }


        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            StartSearch();
        }
    }


    // ============================================================
    // START SEARCH
    // ============================================================

    void StartSearch()
    {
        currentState =
            EnemyState.Search;


        searchMusicTimer =
            searchMusicDuration;


        if (musicManager != null)
        {
            musicManager.StartSearchMusic();
        }


        searchCenter =
            lastKnownPosition;


        searchTimer =
            searchTime;


        searchPoints.Clear();


        if (patrolPoints != null)
        {
            foreach (Transform point in patrolPoints)
            {
                if (point == null)
                    continue;


                if (Vector3.Distance(
                    point.position,
                    lastKnownPosition
                ) <= searchRadius)
                {
                    searchPoints.Add(point);
                }
            }
        }


        if (searchPoints.Count == 0)
        {
            agent.SetDestination(
                lastKnownPosition
            );

            return;
        }


        ShuffleSearchPoints();


        currentSearchIndex = 0;


        agent.speed =
            searchSpeed;


        agent.SetDestination(
            searchPoints[
                currentSearchIndex
            ].position
        );
    }


    // ============================================================
    // SHUFFLE SEARCH POINTS
    // ============================================================

    void ShuffleSearchPoints()
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


    // ============================================================
    // SEARCH
    // ============================================================

    void Search()
    {
        if (searchMusicTimer > 0)
        {
            searchMusicTimer -=
                Time.deltaTime;


            if (searchMusicTimer <= 0)
            {
                if (musicManager != null)
                {
                    musicManager.StopEnemyMusic();
                }
            }
        }


        if (vision != null &&
            vision.CanSeePlayer())
        {
            currentState =
                EnemyState.Chase;

            if (musicManager != null)
            {
                musicManager.StartChaseMusic();
            }

            return;
        }


        searchTimer -=
            Time.deltaTime;


        if (searchTimer <= 0f)
        {
            currentState =
                EnemyState.Patrol;


            if (musicManager != null)
            {
                musicManager.StopEnemyMusic();
            }


            agent.speed =
                patrolSpeed;


            if (patrolPoints != null &&
                patrolPoints.Length > 0)
            {
                agent.SetDestination(
                    patrolPoints[currentPoint].position
                );
            }


            return;
        }


        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            currentSearchIndex++;


            if (currentSearchIndex >=
                searchPoints.Count)
            {
                ShuffleSearchPoints();

                currentSearchIndex = 0;
            }


            agent.SetDestination(
                searchPoints[
                    currentSearchIndex
                ].position
            );
        }
    }


    // ============================================================
    // GIZMOS
    // ============================================================

    void OnDrawGizmos()
    {
        /*
         * Raio de proximidade.
         */
        Gizmos.color =
            Color.magenta;


        Gizmos.DrawWireSphere(
            transform.position,
            proximityDetectionRadius
        );


        /*
         * Área de Search.
         */
        if (currentState ==
            EnemyState.Search)
        {
            Gizmos.color =
                Color.red;


            Gizmos.DrawSphere(
                searchCenter,
                0.5f
            );


            Gizmos.color =
                Color.yellow;


            Gizmos.DrawWireSphere(
                searchCenter,
                searchRadius
            );


            if (searchPoints != null)
            {
                Gizmos.color =
                    Color.green;


                foreach (
                    Transform point
                    in searchPoints
                )
                {
                    if (point != null)
                    {
                        Gizmos.DrawSphere(
                            point.position,
                            0.3f
                        );


                        Gizmos.DrawLine(
                            searchCenter,
                            point.position
                        );
                    }
                }
            }
        }
    }


    // ============================================================
    // RECEBER SOM
    // ============================================================

    public void ReceiveNoise(
        Vector3 noisePosition
    )
    {
        /*
         * Se estiver perseguindo,
         * ignora sons.
         */
        if (currentState ==
            EnemyState.Chase)
        {
            return;
        }


        /*
         * Durante a corrida especial,
         * também não queremos que um som
         * faça ele abandonar os Patrol Points.
         */
        if (currentState ==
            EnemyState.RunAround)
        {
            return;
        }


        lastKnownPosition =
            noisePosition;


        Debug.Log(
            "OUVIU UM SOM EM: " +
            noisePosition
        );


        currentState =
            EnemyState.Investigate;
    }
}