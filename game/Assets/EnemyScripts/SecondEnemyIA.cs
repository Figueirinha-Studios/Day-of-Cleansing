using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SecondEnemyIA : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase
    }

    [Header("Referências")]
    public Transform player;
    public Transform patrolPointsParent;

    private NavMeshAgent agent;
    private EnemyVision enemyVision;
    private Animator animator;

    [Header("Movimento")]
    public float patrolSpeed = 5f;
    public float chaseSpeed = 8f;

    [Tooltip("Distância que o inimigo mantém do Player durante a perseguição.")]
    public float chaseStopDistance = 3f;

    [Tooltip("Tempo que o inimigo continua perseguindo depois de perder o Player.")]
    public float chaseMemoryTime = 3f;

    [Header("Chamar Chappie")]
    public AudioClip callChappieSound;
    public float callChappieVolume = 5f;

    [Tooltip("O inimigo chama o Chappie quando estiver a esta distância ou menos do Player.")]
    public float callChappieDistance = 8f;

    [Tooltip("Raio utilizado pelo NoiseSystem para o Chappie detectar o chamado.")]
    public float callNoiseRadius = 200f;

    [Tooltip("Intervalo entre cada sinal de ruído enviado ao NoiseSystem.")]
    public float callNoiseInterval = 2f;

    [Tooltip("Se ativado, o som será ouvido pelo mapa inteiro.")]
    public bool globalCallSound = true;

    [Header("Movimento do NavMesh")]
    public float movementAcceleration = 8f;
    public float rotationSpeed = 180f;
    public float stoppingDistance = 0.5f;

    [Header("Patrulha")]
    [Tooltip("Quantidade de pontos recentes que não serão escolhidos novamente.")]
    public int rememberedPatrolPoints = 3;

    [Header("Chute")]
    public float kickForce = 8f;
    public float kickUpForce = 3f;
    public float kickRotationForce = 360f;
    public float kickAirTime = 1.2f;

    [Header("Fuga após o chute")]
    public float fleeDistance = 15f;
    public float fleeSpeed = 9f;
    public float fleeDuration = 5f;

    [Header("Som durante o voo")]
    public AudioClip kickFlySound;

    [Range(0f, 10f)]
    public float kickFlyVolume = 1f;

    private AudioSource callAudioSource;
    private AudioSource kickFlyAudioSource;

    private List<Transform> patrolPoints =
        new List<Transform>();

    private List<int> recentlyVisitedPatrolPoints =
        new List<int>();

    private EnemyState currentState =
        EnemyState.Patrol;

    private int currentPatrolIndex = -1;

    private float chaseMemoryTimer = 0f;
    private float callNoiseTimer = 0f;

    private bool isCallingChappie = false;

    private bool isBeingKicked = false;
    private bool isFleeing = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyVision = GetComponent<EnemyVision>();
        animator = GetComponent<Animator>();

        SetupCallChappieAudio();
        SetupKickFlyAudio();

        SetupMovement();

        LoadPatrolPoints();

        currentState = EnemyState.Patrol;

        ChooseNextPatrolPoint();
    }

    private void Update()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (isBeingKicked)
        {
            UpdateAnimator();
            return;
        }

        if (isFleeing)
        {
            UpdateAnimator();
            return;
        }

        UpdateState();

        UpdateCallChappie();

        UpdateAnimator();
    }

    // =========================================================
    // SETUP
    // =========================================================

    private void SetupMovement()
    {
        if (agent == null)
            return;

        agent.acceleration = movementAcceleration;
        agent.angularSpeed = rotationSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = false;
        agent.updateRotation = true;
    }

    private void SetupCallChappieAudio()
    {
        callAudioSource =
            gameObject.AddComponent<AudioSource>();

        callAudioSource.playOnAwake = false;
        callAudioSource.loop = true;

        if (globalCallSound)
            callAudioSource.spatialBlend = 0f;
        else
            callAudioSource.spatialBlend = 1f;

        callAudioSource.volume = callChappieVolume;
    }

    private void SetupKickFlyAudio()
    {
        kickFlyAudioSource =
            gameObject.AddComponent<AudioSource>();

        kickFlyAudioSource.playOnAwake = false;
        kickFlyAudioSource.loop = true;

        // Som 3D, acompanhando a posição do inimigo enquanto ele voa.
        kickFlyAudioSource.spatialBlend = 1f;

        kickFlyAudioSource.volume = kickFlyVolume;
    }

    private void LoadPatrolPoints()
    {
        patrolPoints.Clear();

        if (patrolPointsParent == null)
            return;

        foreach (Transform child in patrolPointsParent)
        {
            patrolPoints.Add(child);
        }
    }

    // =========================================================
    // ESTADOS
    // =========================================================

    private void UpdateState()
    {
        bool seesPlayer = false;

        if (enemyVision != null)
            seesPlayer = enemyVision.CanSeePlayer();

        if (currentState == EnemyState.Patrol)
        {
            if (seesPlayer)
            {
                EnterChase();
                return;
            }

            UpdatePatrol();
        }
        else if (currentState == EnemyState.Chase)
        {
            if (seesPlayer)
            {
                chaseMemoryTimer = chaseMemoryTime;

                UpdateChase();
            }
            else
            {
                chaseMemoryTimer -= Time.deltaTime;

                if (chaseMemoryTimer <= 0f)
                {
                    EnterPatrol();
                }
                else
                {
                    UpdateChase();
                }
            }
        }
    }

    private void EnterPatrol()
    {
        currentState = EnemyState.Patrol;

        StopCallingChappie();

        if (agent != null)
        {
            agent.speed = patrolSpeed;
            agent.stoppingDistance = stoppingDistance;
        }

        ChooseNextPatrolPoint();
    }

    private void EnterChase()
    {
        currentState = EnemyState.Chase;

        chaseMemoryTimer = chaseMemoryTime;

        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = chaseStopDistance;
        }
    }

    // =========================================================
    // PATRULHA
    // =========================================================

    private void UpdatePatrol()
    {
        if (patrolPoints.Count == 0)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <=
            agent.stoppingDistance + 0.2f)
        {
            ChooseNextPatrolPoint();
        }
    }

    private void ChooseNextPatrolPoint()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (patrolPoints.Count == 0)
            return;

        int selectedIndex = -1;

        List<int> availableIndices =
            new List<int>();

        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (!recentlyVisitedPatrolPoints.Contains(i))
                availableIndices.Add(i);
        }

        if (availableIndices.Count > 0)
        {
            selectedIndex =
                availableIndices[
                    Random.Range(
                        0,
                        availableIndices.Count
                    )
                ];
        }
        else
        {
            selectedIndex =
                Random.Range(
                    0,
                    patrolPoints.Count
                );
        }

        currentPatrolIndex = selectedIndex;

        Transform selectedPoint =
            patrolPoints[selectedIndex];

        if (selectedPoint == null)
            return;

        if (recentlyVisitedPatrolPoints.Contains(selectedIndex))
            recentlyVisitedPatrolPoints.Remove(selectedIndex);

        recentlyVisitedPatrolPoints.Add(selectedIndex);

        while (recentlyVisitedPatrolPoints.Count >
               rememberedPatrolPoints)
        {
            recentlyVisitedPatrolPoints.RemoveAt(0);
        }

        agent.speed = patrolSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(selectedPoint.position);
    }

    // =========================================================
    // CHASE
    // =========================================================

    private void UpdateChase()
    {
        if (player == null)
            return;

        if (agent == null)
            return;

        agent.speed = chaseSpeed;
        agent.stoppingDistance = chaseStopDistance;

        agent.SetDestination(player.position);

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance <= chaseStopDistance)
        {
            Vector3 direction =
                player.position -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation =
                    Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed *
                        Time.deltaTime
                    );
            }
        }
    }

    // =========================================================
    // CHAMAR CHAPPIE
    // =========================================================

    private void UpdateCallChappie()
    {
        if (currentState != EnemyState.Chase)
        {
            StopCallingChappie();
            return;
        }

        if (player == null)
        {
            StopCallingChappie();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance <= callChappieDistance)
        {
            if (!isCallingChappie)
            {
                StartCallingChappie();
            }

            callNoiseTimer -= Time.deltaTime;

            if (callNoiseTimer <= 0f)
            {
                EmitChappieCallNoise();

                callNoiseTimer =
                    callNoiseInterval;
            }
        }
        else
        {
            StopCallingChappie();
        }
    }

    private void StartCallingChappie()
    {
        isCallingChappie = true;

        callNoiseTimer = 0f;

        if (callAudioSource != null &&
            callChappieSound != null)
        {
            callAudioSource.clip =
                callChappieSound;

            callAudioSource.volume =
                callChappieVolume;

            callAudioSource.loop = true;

            if (!callAudioSource.isPlaying)
                callAudioSource.Play();
        }

        EmitChappieCallNoise();
    }

    private void StopCallingChappie()
    {
        if (!isCallingChappie)
            return;

        isCallingChappie = false;

        callNoiseTimer = 0f;

        if (callAudioSource != null &&
            callAudioSource.isPlaying)
        {
            callAudioSource.Stop();
        }
    }

    private void EmitChappieCallNoise()
    {
        NoiseSystem.EmitNoise(
            transform.position,
            callNoiseRadius
        );
    }

    // =========================================================
    // CHUTE
    // =========================================================

    public void ReceiveKick(
        Vector3 playerPosition,
        float force,
        float upForce,
        float rotationForce,
        float airTime)
    {
        if (isBeingKicked)
            return;

        if (isFleeing)
            return;

        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        isBeingKicked = true;

        StopCallingChappie();

        chaseMemoryTimer = 0f;

        agent.isStopped = true;

        Vector3 direction =
            transform.position -
            playerPosition;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = -transform.forward;
            direction.y = 0f;
        }

        direction.Normalize();

        StartKickFlySound();

        StartCoroutine(
            KickMovement(
                direction,
                force,
                upForce,
                rotationForce,
                airTime
            )
        );
    }

    private IEnumerator KickMovement(
        Vector3 direction,
        float force,
        float upForce,
        float rotationForce,
        float airTime)
    {
        Vector3 startPosition =
            transform.position;

        Vector3 horizontalMovement =
            direction * force;

        Quaternion startRotation =
            transform.rotation;

        float timer = 0f;

        while (timer < airTime)
        {
            timer += Time.deltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    timer / airTime
                );

            float verticalOffset =
                Mathf.Sin(
                    normalizedTime *
                    Mathf.PI
                ) * upForce;

            Vector3 nextPosition =
                startPosition +
                horizontalMovement *
                normalizedTime;

            nextPosition.y =
                startPosition.y +
                verticalOffset;

            transform.position =
                nextPosition;

            float rotationAmount =
                rotationForce *
                Time.deltaTime;

            transform.Rotate(
                rotationAmount,
                rotationAmount,
                rotationAmount,
                Space.Self
            );

            yield return null;
        }

        StopKickFlySound();

        Vector3 finalPosition =
            transform.position;

        NavMeshHit navHit;

        if (NavMesh.SamplePosition(
            finalPosition,
            out navHit,
            3f,
            NavMesh.AllAreas))
        {
            transform.position =
                navHit.position;
        }

        Vector3 currentEuler =
            transform.eulerAngles;

        transform.rotation =
            Quaternion.Euler(
                0f,
                currentEuler.y,
                0f
            );

        isBeingKicked = false;

        agent.isStopped = false;

        StartCoroutine(
            FleeFromPlayer(
                direction
            )
        );
    }

    // =========================================================
    // SOM DO VOO
    // =========================================================

    private void StartKickFlySound()
    {
        if (kickFlyAudioSource == null)
            return;

        if (kickFlySound == null)
            return;

        kickFlyAudioSource.clip =
            kickFlySound;

        kickFlyAudioSource.volume =
            kickFlyVolume;

        kickFlyAudioSource.loop = true;

        if (!kickFlyAudioSource.isPlaying)
            kickFlyAudioSource.Play();
    }

    private void StopKickFlySound()
    {
        if (kickFlyAudioSource != null &&
            kickFlyAudioSource.isPlaying)
        {
            kickFlyAudioSource.Stop();
        }
    }

    // =========================================================
    // FUGA
    // =========================================================

    private IEnumerator FleeFromPlayer(
        Vector3 direction)
    {
        if (agent == null)
            yield break;

        if (!agent.isOnNavMesh)
            yield break;

        isFleeing = true;

        StopCallingChappie();

        agent.isStopped = false;
        agent.speed = fleeSpeed;
        agent.stoppingDistance = 0f;

        Vector3 fleeTarget =
            transform.position +
            direction.normalized *
            fleeDistance;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            fleeTarget,
            out hit,
            fleeDistance,
            NavMesh.AllAreas))
        {
            fleeTarget = hit.position;
        }

        agent.SetDestination(fleeTarget);

        float timer = 0f;

        while (timer < fleeDuration)
        {
            if (agent == null)
                break;

            if (!agent.isOnNavMesh)
                break;

            timer += Time.deltaTime;

            if (agent.remainingDistance <=
                1f)
            {
                break;
            }

            yield return null;
        }

        isFleeing = false;

        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.stoppingDistance =
                stoppingDistance;
        }

        currentState =
            EnemyState.Patrol;

        ChooseNextPatrolPoint();
    }

    // =========================================================
    // ANIMAÇÃO
    // =========================================================

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        if (agent == null)
            return;

        animator.SetFloat(
            "Speed",
            agent.velocity.magnitude
        );
    }

    // =========================================================
    // ACESSO PARA O SECOND ENEMY KICK
    // =========================================================

    public bool IsBeingKicked()
    {
        return isBeingKicked;
    }

    public bool IsFleeing()
    {
        return isFleeing;
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            callChappieDistance
        );

        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            callNoiseRadius
        );
    }
}