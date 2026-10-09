
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SecondEnemyKick : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Camera playerCamera;

    [Header("Imagem de Interação")]
    public Image kickImage;

    [Tooltip("Distância máxima para poder chutar o inimigo.")]
    public float kickDistance = 3f;

    [Tooltip("Espessura da mira para detectar o inimigo.")]
    public float kickAimRadius = 0.25f;

    [Header("Chute")]
    public float kickForce = 8f;
    public float kickUpForce = 3f;
    public float kickRotationForce = 720f;
    public float kickAirTime = 1.2f;

    [Header("Som do Chute")]
    public AudioClip kickSound;

    [Range(0f, 10f)]
    public float kickSoundVolume = 1f;

    [Header("Movimento da Cabeça")]
    public Transform playerHead;
    public float headKickAngle = 8f;
    public float headKickSpeed = 12f;

    [Header("Referência")]
    public SecondEnemyIA secondEnemyIA;

    private AudioSource kickAudioSource;
    private bool isBeingKicked = false;

    private void Start()
    {
        if (secondEnemyIA == null)
            secondEnemyIA = GetComponent<SecondEnemyIA>();

        SetupKickAudio();
        HideKickImage();
    }

    private void SetupKickAudio()
    {
        kickAudioSource = gameObject.AddComponent<AudioSource>();

        kickAudioSource.playOnAwake = false;
        kickAudioSource.loop = false;
        kickAudioSource.spatialBlend = 0f;
        kickAudioSource.volume = kickSoundVolume;
    }

    private void Update()
    {
        if (player == null || playerCamera == null)
        {
            HideKickImage();
            return;
        }

        if (isBeingKicked)
        {
            HideKickImage();
            return;
        }

        // Este script não processa a tecla E.
        // O PlayerPickup controla o chute e a prioridade dos HUDs.
        UpdateKickDetection();
    }

    public static bool TryKickForPlayer(
        Camera camera,
        Transform playerTransform,
        bool executeKick = true)
    {
        if (camera == null || playerTransform == null)
            return false;

        SecondEnemyKick[] enemies =
            FindObjectsByType<SecondEnemyKick>(
                FindObjectsSortMode.None
            );

        foreach (SecondEnemyKick enemy in enemies)
        {
            if (enemy == null || !enemy.isActiveAndEnabled)
                continue;

            if (enemy.player != playerTransform)
                continue;

            if (enemy.playerCamera != camera)
                continue;

            if (!enemy.CanKick())
                continue;

            if (executeKick)
                enemy.KickEnemy();

            return true;
        }

        return false;
    }

    private void UpdateKickDetection()
    {
        bool canKick = CanKick();

        if (kickImage != null)
            kickImage.gameObject.SetActive(canKick);
    }

    private bool CanKick()
    {
        if (player == null || playerCamera == null)
            return false;

        if (secondEnemyIA == null)
            return false;

        if (isBeingKicked)
            return false;

        if (secondEnemyIA.IsBeingKicked())
            return false;

        if (secondEnemyIA.IsFleeing())
            return false;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        if (distance > kickDistance)
            return false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            kickAimRadius,
            kickDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        float closestDistance = Mathf.Infinity;
        RaycastHit closestHit = new RaycastHit();
        bool foundHit = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            // Ignora os colliders pertencentes ao jogador.
            if (hit.collider.transform.IsChildOf(player))
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
                foundHit = true;
            }
        }

        if (!foundHit)
            return false;

        SecondEnemyIA hitEnemy =
            closestHit.collider.GetComponentInParent<SecondEnemyIA>();

        return hitEnemy == secondEnemyIA;
    }

    private void KickEnemy()
    {
        if (isBeingKicked ||
            secondEnemyIA == null ||
            !CanKick())
        {
            return;
        }

        isBeingKicked = true;
        HideKickImage();

        PlayKickSound();

        if (playerHead != null)
            StartCoroutine(HeadKickMotion());

        secondEnemyIA.ReceiveKick(
            player.position,
            kickForce,
            kickUpForce,
            kickRotationForce,
            kickAirTime
        );

        StartCoroutine(ResetKickInput());
    }

    private void PlayKickSound()
    {
        if (kickAudioSource == null || kickSound == null)
            return;

        kickAudioSource.volume = kickSoundVolume;
        kickAudioSource.PlayOneShot(kickSound);
    }

    private IEnumerator ResetKickInput()
    {
        yield return new WaitForSeconds(0.15f);
        isBeingKicked = false;
    }

    private IEnumerator HeadKickMotion()
    {
        if (playerHead == null)
            yield break;

        Quaternion originalRotation =
            playerHead.localRotation;

        Quaternion kickRotation =
            originalRotation *
            Quaternion.Euler(-headKickAngle, 0f, 0f);

        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * headKickSpeed;

            playerHead.localRotation = Quaternion.Slerp(
                originalRotation,
                kickRotation,
                timer
            );

            yield return null;
        }

        timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * headKickSpeed;

            playerHead.localRotation = Quaternion.Slerp(
                kickRotation,
                originalRotation,
                timer
            );

            yield return null;
        }

        playerHead.localRotation = originalRotation;
    }

    private void HideKickImage()
    {
        if (kickImage != null)
            kickImage.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            kickDistance
        );
    }
}