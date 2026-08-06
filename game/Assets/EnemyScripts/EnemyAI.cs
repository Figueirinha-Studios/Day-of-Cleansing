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

    private EnemyHearing hearing;

    [Header("Managers")]
    public MusicManager musicManager;

    public float searchMusicDuration = 8f;
    private float searchMusicTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<EnemyVision>();
        hearing = GetComponent<EnemyHearing>();
        agent.speed = patrolSpeed;
        patrolPoints = new Transform[patrolPointsParent.childCount];

        for (int i = 0; i < patrolPointsParent.childCount; i++)
        {
            patrolPoints[i] = patrolPointsParent.GetChild(i);
        }

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    void Update()
    {
        CheckHearing();

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
                    currentState = EnemyState.LostSight;
                    memoryTimer = memoryTime;
                    lastKnownPosition = player.position;
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
                }
                break;

            case EnemyState.Investigate:

                Investigate();
                break;
        }
    }

    void StartSearch()
    {
        currentState = EnemyState.Search;
        searchMusicTimer = searchMusicDuration;
        musicManager.StartSearchMusic();

        searchCenter = lastKnownPosition;
        searchTimer = searchTime;
        searchPoints.Clear();

        foreach (Transform point in patrolPoints)
        {
            if (Vector3.Distance(point.position, lastKnownPosition) <= searchRadius)
            {
                searchPoints.Add(point);
            }
        }

        // Se nenhum ponto estiver perto,
        // procura exatamente onde perdeu o jogador.
        if (searchPoints.Count == 0)
        {
            agent.SetDestination(lastKnownPosition);
            return;
        }

        ShuffleSearchPoints();
        currentSearchIndex = 0;
        agent.speed = searchSpeed;
        agent.SetDestination(searchPoints[currentSearchIndex].position);
    }

    void ShuffleSearchPoints()
    {
        for (int i = 0; i < searchPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, searchPoints.Count);

            Transform temp = searchPoints[i];
            searchPoints[i] = searchPoints[randomIndex];
            searchPoints[randomIndex] = temp;
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
                    nextPoint = Random.Range(0, patrolPoints.Length);
                }
                while (nextPoint == currentPoint);

                currentPoint = nextPoint;
                agent.SetDestination(patrolPoints[currentPoint].position);
                waitTimer = 0;
            }
        }
    }

    void Chase()
    {
        if (vision.CanSeePlayer())
        {
            lastKnownPosition = player.position;
        }

        agent.speed = chaseSpeed;
        agent.SetDestination(lastKnownPosition);
    }

    void Search()
    {
        if (searchMusicTimer > 0)
        {
            searchMusicTimer -= Time.deltaTime;

            if (searchMusicTimer <= 0)
            {
                musicManager.StopEnemyMusic();
            }
        }

        // Encontrou o jogador?
        if (vision.CanSeePlayer())
        {
            currentState = EnemyState.Chase;
            musicManager.StartChaseMusic();
            return;
        }

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            currentState = EnemyState.Patrol;
            musicManager.StopEnemyMusic();
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPoint].position);
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

            agent.SetDestination(searchPoints[currentSearchIndex].position);
        }
    }

    void Investigate()
    {
        agent.speed = searchSpeed;

        agent.SetDestination(lastKnownPosition);

        // Se enxergar o jogador durante investigação
        if (vision.CanSeePlayer())
        {
            currentState = EnemyState.Chase;
            musicManager.StartChaseMusic();
            return;
        }

        // Chegou no local do barulho
        if (!agent.pathPending &&
           agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = EnemyState.Search;
            StartSearch();
        }
    }

    void LostSight()
    {
        // Continua indo para a última posição conhecida
        agent.SetDestination(lastKnownPosition);

        // Encontrou o jogador novamente?
        if (vision.CanSeePlayer())
        {
            currentState = EnemyState.Chase;
            musicManager.StartChaseMusic();
            return;
        }

        memoryTimer -= Time.deltaTime;

        if (memoryTimer <= 0)
        {
            StartSearch();
        }
    }

    void OnDrawGizmos()
    {
        if (currentState == EnemyState.Search)
        {
            // Centro da busca
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(searchCenter, 0.5f);

            // Área de procura
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(searchCenter, searchRadius);

            // Pontos que ele está procurando
            if (searchPoints != null)
            {
                Gizmos.color = Color.green;

                foreach (Transform point in searchPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawSphere(point.position, 0.3f);
                        Gizmos.DrawLine(searchCenter, point.position);
                    }
                }
            }
        }
    }

    void CheckHearing()
    {
        PlayerNoise noise = player.GetComponent<PlayerNoise>();

        if (noise == null)
            return;

        if (noise.currentNoise <= 0)
            return;

        if (hearing.HeardPlayer(noise.currentNoise))
        {
            lastKnownPosition = player.position;

            // Som perde apenas para visão
            if (currentState != EnemyState.Chase)
            {
                currentState = EnemyState.Investigate;
            }
        }
    }
}