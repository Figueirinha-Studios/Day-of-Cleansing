
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

    [Header("Olhar ao redor")]
    [Tooltip("Quantidade de pontos de patrulha concluídos antes de olhar ao redor.")]
    public int patrolPointsBeforeLookAround = 3;

    [Tooltip("Velocidade da rotação de 360 graus, em graus por segundo.")]
    public float lookAroundRotationSpeed = 120f;

    private int patrolPointsVisited = 0;
    private bool isLookingAround = false;
    private Coroutine lookAroundCoroutine;
    private bool previousUpdateRotation = true;

    [Header("Chute")]
    public float kickForce = 8f;
    public float kickUpForce = 3f;
    public float kickRotationForce = 360f;
    public float kickAirTime = 1.2f;

    [Header("Fuga após o chute")]
    public float fleeDistance = 15f;
    public float fleeSpeed = 9f;
    public float fleeDuration = 5f;

    [Header("Casinha")]
    public float houseSearchRadius = 100f;
    public float houseArrivalDistance = 1f;

    private ChappieJrHouse currentHouse;
    private Coroutine houseCoroutine;

    private Renderer[] enemyRenderers;
    private bool[] originalRendererStates;

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

        enemyRenderers =
            GetComponentsInChildren<Renderer>(true);

        originalRendererStates =
            new bool[enemyRenderers.Length];

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            originalRendererStates[i] =
                enemyRenderers[i].enabled;
        }

        currentState = EnemyState.Patrol;

        ChooseNextPatrolPoint();
    }

    private void Update()
    {
        if (agent == null || !agent.enabled)
        {
            UpdateAnimator();
            return;
        }

        if (!agent.isOnNavMesh)
        {
            UpdateAnimator();
            return;
        }

        if (isBeingKicked || isFleeing)
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

        callAudioSource.spatialBlend =
            globalCallSound ? 0f : 1f;

        callAudioSource.volume = callChappieVolume;
    }

    private void SetupKickFlyAudio()
    {
        kickFlyAudioSource =
            gameObject.AddComponent<AudioSource>();

        kickFlyAudioSource.playOnAwake = false;
        kickFlyAudioSource.loop = true;
        kickFlyAudioSource.spatialBlend = 1f;
        kickFlyAudioSource.volume = kickFlyVolume;
    }

    private void LoadPatrolPoints()
    {
        patrolPoints.Clear();

        if (patrolPointsParent == null)
            return;

        foreach (Transform child in patrolPointsParent)
            patrolPoints.Add(child);
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
                    EnterPatrol();
                else
                    UpdateChase();
            }
        }
    }

    private void EnterPatrol()
    {
        CancelLookAround();

        currentState = EnemyState.Patrol;

        StopCallingChappie();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.speed = patrolSpeed;
            agent.stoppingDistance = stoppingDistance;
        }

        ChooseNextPatrolPoint();
    }

    private void EnterChase()
    {
        CancelLookAround();

        currentState = EnemyState.Chase;
        chaseMemoryTimer = chaseMemoryTime;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
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
        if (isLookingAround || patrolPoints.Count == 0)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <=
            agent.stoppingDistance + 0.2f)
        {
            patrolPointsVisited++;

            int pointsRequired =
                Mathf.Max(1, patrolPointsBeforeLookAround);

            if (patrolPointsVisited >= pointsRequired)
            {
                patrolPointsVisited = 0;
                StartLookAround();
                return;
            }

            ChooseNextPatrolPoint();
        }
    }

    private void ChooseNextPatrolPoint()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (patrolPoints.Count == 0)
            return;

        int selectedIndex = -1;

        List<int> availableIndices = new List<int>();

        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (!recentlyVisitedPatrolPoints.Contains(i))
                availableIndices.Add(i);
        }

        if (availableIndices.Count > 0)
        {
            selectedIndex =
                availableIndices[
                    Random.Range(0, availableIndices.Count)
                ];
        }
        else
        {
            selectedIndex =
                Random.Range(0, patrolPoints.Count);
        }

        currentPatrolIndex = selectedIndex;

        Transform selectedPoint = patrolPoints[selectedIndex];

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
        agent.isStopped = false;
        agent.SetDestination(selectedPoint.position);
    }

    // =========================================================
    // OLHAR AO REDOR
    // =========================================================

    private void StartLookAround()
    {
        if (isLookingAround)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        isLookingAround = true;

        previousUpdateRotation = agent.updateRotation;

        agent.isStopped = true;
        agent.updateRotation = false;
        agent.ResetPath();

        lookAroundCoroutine =
            StartCoroutine(LookAround360());
    }

    private IEnumerator LookAround360()
    {
        Quaternion startingRotation = transform.rotation;

        float rotationAmount = 0f;

        float speed =
            Mathf.Max(1f, lookAroundRotationSpeed);

        while (rotationAmount < 360f)
        {
            if (currentState != EnemyState.Patrol ||
                isBeingKicked ||
                isFleeing)
            {
                yield break;
            }

            rotationAmount += speed * Time.deltaTime;

            float currentAngle =
                Mathf.Min(rotationAmount, 360f);

            transform.rotation =
                startingRotation *
                Quaternion.Euler(0f, currentAngle, 0f);

            yield return null;
        }

        transform.rotation = startingRotation;

        lookAroundCoroutine = null;
        isLookingAround = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.updateRotation = previousUpdateRotation;
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = stoppingDistance;
        }

        if (currentState == EnemyState.Patrol &&
            !isBeingKicked &&
            !isFleeing)
        {
            ChooseNextPatrolPoint();
        }
    }

    private void CancelLookAround()
    {
        if (!isLookingAround)
            return;

        if (lookAroundCoroutine != null)
        {
            StopCoroutine(lookAroundCoroutine);
            lookAroundCoroutine = null;
        }

        isLookingAround = false;

        if (agent != null && agent.enabled)
        {
            agent.updateRotation = previousUpdateRotation;

            if (agent.isOnNavMesh)
                agent.isStopped = false;
        }
    }

    // =========================================================
    // PERSEGUIÇÃO
    // =========================================================

    private void UpdateChase()
    {
        if (player == null || agent == null)
            return;

        agent.speed = chaseSpeed;
        agent.stoppingDistance = chaseStopDistance;
        agent.SetDestination(player.position);

        float distance =
            Vector3.Distance(transform.position, player.position);

        if (distance <= chaseStopDistance)
        {
            Vector3 direction =
                player.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation =
                    Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
            }
        }
    }

    // =========================================================
    // CHAMAR CHAPPIE
    // =========================================================

    private void UpdateCallChappie()
    {
        if (currentState != EnemyState.Chase || player == null)
        {
            StopCallingChappie();
            return;
        }

        float distance =
            Vector3.Distance(transform.position, player.position);

        if (distance <= callChappieDistance)
        {
            if (!isCallingChappie)
                StartCallingChappie();

            callNoiseTimer -= Time.deltaTime;

            if (callNoiseTimer <= 0f)
            {
                EmitChappieCallNoise();
                callNoiseTimer = callNoiseInterval;
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

        if (callAudioSource != null && callChappieSound != null)
        {
            callAudioSource.clip = callChappieSound;
            callAudioSource.volume = callChappieVolume;
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

        if (callAudioSource != null && callAudioSource.isPlaying)
            callAudioSource.Stop();
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
        if (isBeingKicked || isFleeing)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        CancelLookAround();

        isBeingKicked = true;

        StopCallingChappie();

        chaseMemoryTimer = 0f;

        agent.isStopped = true;

        Vector3 direction =
            transform.position - playerPosition;

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
        Vector3 startPosition = transform.position;

        Vector3 horizontalMovement = direction * force;

        float timer = 0f;

        while (timer < airTime)
        {
            timer += Time.deltaTime;

            float normalizedTime =
                Mathf.Clamp01(timer / airTime);

            float verticalOffset =
                Mathf.Sin(normalizedTime * Mathf.PI) * upForce;

            Vector3 nextPosition =
                startPosition + horizontalMovement * normalizedTime;

            nextPosition.y =
                startPosition.y + verticalOffset;

            transform.position = nextPosition;

            float rotationAmount =
                rotationForce * Time.deltaTime;

            transform.Rotate(
                rotationAmount,
                rotationAmount,
                rotationAmount,
                Space.Self
            );

            yield return null;
        }

        StopKickFlySound();

        Vector3 finalPosition = transform.position;

        NavMeshHit navHit;

        if (NavMesh.SamplePosition(
            finalPosition,
            out navHit,
            3f,
            NavMesh.AllAreas))
        {
            transform.position = navHit.position;
        }

        Vector3 currentEuler = transform.eulerAngles;

        transform.rotation =
            Quaternion.Euler(0f, currentEuler.y, 0f);

        isBeingKicked = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;

        houseCoroutine = StartCoroutine(GoToNearestHouse());
    }

    // =========================================================
    // SOM DURANTE O VOO
    // =========================================================

    private void StartKickFlySound()
    {
        if (kickFlyAudioSource == null || kickFlySound == null)
            return;

        kickFlyAudioSource.clip = kickFlySound;
        kickFlyAudioSource.volume = kickFlyVolume;
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
    // CASINHA
    // =========================================================

    private IEnumerator GoToNearestHouse()
    {
        isFleeing = true;
        StopCallingChappie();

        ChappieJrHouse[] houses =
            FindObjectsByType<ChappieJrHouse>(
                FindObjectsSortMode.None
            );

        ChappieJrHouse nearestHouse = null;
        float nearestDistance = float.MaxValue;

        foreach (ChappieJrHouse house in houses)
        {
            if (house == null || house.isOccupied)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    house.EntrancePosition
                );

            if (distance <= houseSearchRadius &&
                distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestHouse = house;
            }
        }

        // Sem casinhas disponíveis: volta à patrulha.
        if (nearestHouse == null)
        {
            FinishHouseBehavior();
            yield break;
        }

        currentHouse = nearestHouse;
        currentHouse.isOccupied = true;

        agent.isStopped = false;
        agent.speed = fleeSpeed;
        agent.stoppingDistance = houseArrivalDistance;

        agent.SetDestination(currentHouse.EntrancePosition);

        // Caminhar até o lado escolhido da casinha.
        while (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= houseArrivalDistance + 0.1f)
            {
                break;
            }

            if (!agent.pathPending &&
                (!agent.hasPath ||
                 agent.pathStatus == NavMeshPathStatus.PathInvalid))
            {
                currentHouse.isOccupied = false;
                currentHouse = null;

                FinishHouseBehavior();
                yield break;
            }

            yield return null;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            if (currentHouse != null)
                currentHouse.isOccupied = false;

            currentHouse = null;
            FinishHouseBehavior();
            yield break;
        }

        // Parar na entrada.
        agent.isStopped = true;
        agent.ResetPath();

        Vector3 entrancePosition = currentHouse.EntrancePosition;

        // Virar para a entrada antes de desaparecer.
        Vector3 direction =
            entrancePosition - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);

        // Esconder o modelo.
        SetHouseVisibility(false);

        // Desativar o agente enquanto está escondido.
        agent.enabled = false;

        yield return new WaitForSeconds(
            Mathf.Max(0f, currentHouse.hideDuration)
        );

        // Reaparecer na entrada da casinha.
        transform.position = entrancePosition;

        agent.enabled = true;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            entrancePosition,
            out hit,
            3f,
            NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        // Restaurar o modelo.
        SetHouseVisibility(true);

        currentHouse.isOccupied = false;
        currentHouse = null;

        // Voltar à patrulha.
        FinishHouseBehavior();
        houseCoroutine = null;
    }

    private void SetHouseVisibility(bool visible)
    {
        if (enemyRenderers == null)
            return;

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i] != null)
            {
                enemyRenderers[i].enabled =
                    visible && originalRendererStates[i];
            }
        }
    }

    private void FinishHouseBehavior()
    {
        isFleeing = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = stoppingDistance;
        }

        currentState = EnemyState.Patrol;

        ChooseNextPatrolPoint();
    }

    // =========================================================
    // FUGA ORIGINAL
    // =========================================================

    private IEnumerator FleeFromPlayer(Vector3 direction)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            yield break;

        isFleeing = true;

        StopCallingChappie();

        agent.isStopped = false;
        agent.speed = fleeSpeed;
        agent.stoppingDistance = 0f;

        Vector3 fleeTarget =
            transform.position + direction.normalized * fleeDistance;

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
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
                break;

            timer += Time.deltaTime;

            if (!agent.pathPending &&
                agent.remainingDistance <= 1f)
            {
                break;
            }

            yield return null;
        }

        isFleeing = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = stoppingDistance;
        }

        currentState = EnemyState.Patrol;

        ChooseNextPatrolPoint();
    }

    // =========================================================
    // ANIMAÇÃO
    // =========================================================

    private void UpdateAnimator()
    {
        if (animator == null || agent == null)
            return;

        float speed = 0f;

        if (agent.enabled)
            speed = agent.velocity.magnitude;

        animator.SetFloat("Speed", speed);
    }

    // =========================================================
    // ACESSO PARA SECOND ENEMY KICK
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

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            houseSearchRadius
        );

        if (currentHouse != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawLine(
                transform.position,
                currentHouse.EntrancePosition
            );
        }
    }
}
