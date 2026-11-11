
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ChappieJumpscare : MonoBehaviour
{
    [Header("Detecção do Jogador")]
    public Transform player;
    public float distanciaMinima = 2f;

    [Header("Câmera")]
    public Transform playerCamera;
    public Transform jumpscareCameraPoint;
    public CameraController cameraController;

    [Header("Animação do Chappie")]
    public Animator jumpscareAnimator;
    public string animationTrigger = "Jumpscare";

    [Header("Movimento do Jogador")]
    [Tooltip("Coloque aqui os scripts responsáveis pelo movimento do jogador.")]
    public Behaviour[] playerMovementScripts;

    [Header("Som")]
    public AudioSource audioSource;
    public AudioClip jumpscareSound;

    [Header("Finalização")]
    public string gameOverScene = "GameOver";
    public float sceneChangeDelay = 2f;

    [Header("Mapa")]
    public MapController mapController;

    private bool jumpscareStarted = false;

    private NavMeshAgent agent;
    private EnemyAI enemyAI;

    private MusicManager musicManager;

    private Vector3 fixedCameraPosition;
    private Quaternion fixedCameraRotation;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();

        musicManager = FindFirstObjectByType<MusicManager>();

        if (mapController == null)
            mapController = FindFirstObjectByType<MapController>();

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (jumpscareStarted) return;
        if (player == null) return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= distanciaMinima)
        {
            StartJumpscare();
        }
    }

    private void StartJumpscare()
    {
        if (jumpscareStarted) return;

        jumpscareStarted = true;

        // =====================================================
        // FECHA O MAPA, CASO ESTEJA ABERTO
        // =====================================================

        if (mapController != null)
        {
            mapController.CloseMapForJumpscare();
        }

        // =====================================================
        // PARA TODAS AS MÚSICAS DO MUSIC MANAGER
        // =====================================================

        if (musicManager != null)
        {
            // Interrompe possíveis corrotinas de música.
            musicManager.StopAllCoroutines();

            // Para a música ambiente.
            if (musicManager.ambientSource != null)
            {
                musicManager.ambientSource.Stop();
            }

            // Para a música do inimigo.
            if (musicManager.enemySourceA != null)
            {
                musicManager.enemySourceA.Stop();
            }

            if (musicManager.enemySourceB != null)
            {
                musicManager.enemySourceB.Stop();
            }

            // Desativa o MusicManager.
            musicManager.enabled = false;
        }

        // Desativa a inteligência artificial do Chappie
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        // Para completamente o NavMeshAgent
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // Desativa a movimentação do jogador
        if (playerMovementScripts != null)
        {
            foreach (Behaviour script in playerMovementScripts)
            {
                if (script != null)
                {
                    script.enabled = false;
                }
            }
        }

        // Desativa o controle normal da câmera
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }

        // Captura a posição fixa da câmera do jumpscare
        if (playerCamera != null && jumpscareCameraPoint != null)
        {
            fixedCameraPosition = jumpscareCameraPoint.position;
            fixedCameraRotation = jumpscareCameraPoint.rotation;

            playerCamera.SetPositionAndRotation(
                fixedCameraPosition,
                fixedCameraRotation
            );
        }

        // Inicia a animação do jumpscare
        if (jumpscareAnimator != null)
        {
            jumpscareAnimator.SetTrigger(animationTrigger);
        }

        // Reproduz o som
        if (audioSource != null && jumpscareSound != null)
        {
            audioSource.PlayOneShot(jumpscareSound);
        }

        // Envia os comandos MQTT
        if (MQTTManager.Instance != null)
        {
            MQTTManager.Instance.Publish(
                "dayofcleansing/controller",
                "RELEAon"
            );

            MQTTManager.Instance.Publish(
                "dayofcleansing/controller",
                "RELEBon"
            );
        }

        // Aguarda e carrega a tela de Game Over
        StartCoroutine(GameOverRoutine());
    }

    private void LateUpdate()
    {
        if (!jumpscareStarted) return;

        // Mantém a câmera exatamente na posição capturada
        if (playerCamera != null)
        {
            playerCamera.SetPositionAndRotation(
                fixedCameraPosition,
                fixedCameraRotation
            );
        }
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(sceneChangeDelay);

        SceneManager.LoadScene(gameOverScene);
    }
}
