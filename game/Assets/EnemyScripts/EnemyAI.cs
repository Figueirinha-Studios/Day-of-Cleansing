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
        Search
    }

    [Header("State")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("References")]
    public Transform player;

    private EnemyVision vision;

    [Header("Detecção de Proximidade")]
    [Tooltip("Raio ao redor do Chappie que força a perseguição.")]
    public float proximityDetectionRadius = 1f;

    private bool wasPlayerInProximity = false;

    [Header("Patrol")]
    public Transform patrolPointsParent;
    private Transform[] patrolPoints;
    public float patrolSpeed = 5f;
    public float chaseSpeed = 3f;
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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<EnemyVision>();
        enemyAudio = GetComponent<EnemyAudio>();
        animator = GetComponent<Animator>();

        agent.speed = patrolSpeed;

        patrolPoints = new Transform[patrolPointsParent.childCount];

        for (int i = 0; i < patrolPointsParent.childCount; i++)
        {
            patrolPoints[i] = patrolPointsParent.GetChild(i);
        }

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(
                patrolPoints[currentPoint].position
            );
        }
    }

    void Update()
    {
        HandleProximityDetection();

        switch (currentState)
        {
            case EnemyState.Patrol:

                Patrol();

                if (vision.CanSeePlayer())
                {
                    lastKnownPosition = player.position;
                    currentState = EnemyState.Chase;
                    musicManager.StartChaseMusic();
                }

                break;

            case EnemyState.Chase:

                Chase();

                if (!vision.CanSeePlayer())
                {
                    if (!vision.IsVisionStillActive())
                    {
                        currentState = EnemyState.LostSight;
                        memoryTimer = memoryTime;
                        lastKnownPosition = player.position;
                    }
                }

                break;

            case EnemyState.LostSight:

                LostSight();

                break;

            case EnemyState.Search:

                Search();

                if (vision.CanSeePlayer())
                {
                    currentState = EnemyState.Chase;
                    musicManager.StartChaseMusic();
                }

                break;

            case EnemyState.Investigate:

                Investigate();

                break;
        }

        float currentSpeed = agent.velocity.magnitude;

        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                currentSpeed
            );
        }

        bool isMoving = currentSpeed > 0.1f;
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

    void HandleProximityDetection()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        bool playerInProximity =
            distance <= proximityDetectionRadius;

        /*
         * O player acabou de entrar no raio.
         * Força o Chappie a entrar em Chase.
         */
        if (playerInProximity && !wasPlayerInProximity)
        {
            lastKnownPosition = player.position;

            if (currentState != EnemyState.Chase)
            {
                currentState = EnemyState.Chase;

                if (musicManager != null)
                {
                    musicManager.StartChaseMusic();
                }
            }
        }

        wasPlayerInProximity = playerInProximity;
    }

    void StartSearch()
    {
        currentState = EnemyState.Search;

        searchMusicTimer = searchMusicDuration;

        if (musicManager != null)
        {
            musicManager.StartSearchMusic();
        }

        searchCenter = lastKnownPosition;

        searchTimer = searchTime;

        searchPoints.Clear();

        foreach (Transform point in patrolPoints)
        {
            if (Vector3.Distance(
                point.position,
                lastKnownPosition
            ) <= searchRadius)
            {
                searchPoints.Add(point);
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

        agent.speed = searchSpeed;

        agent.SetDestination(
            searchPoints[currentSearchIndex].position
        );
    }

    void ShuffleSearchPoints()
    {
        for (int i = 0; i < searchPoints.Count; i++)
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

    void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                currentPoint++;

                int nextPoint;

                do
                {
                    nextPoint =
                        Random.Range(
                            0,
                            patrolPoints.Length
                        );
                }
                while (
                    patrolPoints.Length > 1 &&
                    nextPoint == currentPoint
                );

                currentPoint = nextPoint;

                agent.SetDestination(
                    patrolPoints[currentPoint].position
                );

                waitTimer = 0;
            }
        }
    }

    void Chase()
    {
        if (vision.CanSeePlayer())
        {
            lastKnownPosition =
                player.position;
        }

        agent.speed = chaseSpeed;

        agent.SetDestination(
            lastKnownPosition
        );
    }

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

        if (vision.CanSeePlayer())
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

            if (patrolPoints.Length > 0)
            {
                agent.SetDestination(
                    patrolPoints[currentPoint].position
                );
            }

            return;
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            currentSearchIndex++;

            if (currentSearchIndex >= searchPoints.Count)
            {
                ShuffleSearchPoints();

                currentSearchIndex = 0;
            }

            agent.SetDestination(
                searchPoints[currentSearchIndex].position
            );
        }
    }

    void Investigate()
    {
        agent.speed = searchSpeed;

        agent.SetDestination(
            lastKnownPosition
        );

        if (vision.CanSeePlayer())
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
            agent.remainingDistance <= agent.stoppingDistance)
        {
            StartSearch();
        }
    }

    void LostSight()
    {
        agent.SetDestination(
            lastKnownPosition
        );

        if (vision.CanSeePlayer())
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

    void OnDrawGizmos()
    {
        if (currentState == EnemyState.Search)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(
                searchCenter,
                0.5f
            );

            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                searchCenter,
                searchRadius
            );

            if (searchPoints != null)
            {
                Gizmos.color = Color.green;

                foreach (Transform point in searchPoints)
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

        /*
         * Mostra o hitbox de proximidade
         * do Chappie no Editor.
         */
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            proximityDetectionRadius
        );
    }

    public void ReceiveNoise(Vector3 noisePosition)
    {
        if (currentState == EnemyState.Chase)
            return;

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