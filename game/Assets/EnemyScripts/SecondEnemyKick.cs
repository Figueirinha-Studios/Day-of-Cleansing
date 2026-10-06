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
        kickAudioSource =
            gameObject.AddComponent<AudioSource>();

        kickAudioSource.playOnAwake = false;
        kickAudioSource.loop = false;

        // O som vem do Player.
        kickAudioSource.spatialBlend = 0f;

        kickAudioSource.volume =
            kickSoundVolume;
    }

    private void Update()
    {
        if (player == null)
        {
            HideKickImage();
            return;
        }

        if (playerCamera == null)
        {
            HideKickImage();
            return;
        }

        if (isBeingKicked)
        {
            HideKickImage();
            return;
        }

        UpdateKickDetection();

        if (CanKick() &&
            Input.GetKeyDown(KeyCode.E))
        {
            KickEnemy();
        }
    }

    private void UpdateKickDetection()
    {
        bool canKick = CanKick();

        if (kickImage != null)
        {
            kickImage.gameObject.SetActive(
                canKick
            );
        }
    }

    private bool CanKick()
    {
        if (secondEnemyIA == null)
            return false;

        if (secondEnemyIA.IsBeingKicked())
            return false;

        if (secondEnemyIA.IsFleeing())
            return false;

        float distance =
            Vector3.Distance(
                player.position,
                transform.position
            );

        if (distance > kickDistance)
            return false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit[] hits =
            Physics.SphereCastAll(
                ray,
                kickAimRadius,
                kickDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );

        float closestDistance =
            Mathf.Infinity;

        bool foundEnemy = false;

        foreach (RaycastHit hit in hits)
        {
            SecondEnemyIA enemy =
                hit.collider
                    .GetComponentInParent<SecondEnemyIA>();

            if (enemy == null)
                continue;

            if (enemy != secondEnemyIA)
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance =
                    hit.distance;

                foundEnemy = true;
            }
        }

        return foundEnemy;
    }

    private void KickEnemy()
    {
        if (isBeingKicked)
            return;

        if (secondEnemyIA == null)
            return;

        if (!CanKick())
            return;

        isBeingKicked = true;

        HideKickImage();

        // SOM DO CHUTE
        PlayKickSound();

        // Movimento da cabeça
        if (playerHead != null)
        {
            StartCoroutine(
                HeadKickMotion()
            );
        }

        // Manda o inimigo voar
        secondEnemyIA.ReceiveKick(
            player.position,
            kickForce,
            kickUpForce,
            kickRotationForce,
            kickAirTime
        );

        StartCoroutine(
            ResetKickInput()
        );
    }

    private void PlayKickSound()
    {
        if (kickAudioSource == null)
            return;

        if (kickSound == null)
            return;

        kickAudioSource.volume =
            kickSoundVolume;

        kickAudioSource.PlayOneShot(
            kickSound
        );
    }

    private IEnumerator ResetKickInput()
    {
        yield return new WaitForSeconds(
            0.15f
        );

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
            Quaternion.Euler(
                -headKickAngle,
                0f,
                0f
            );

        float timer = 0f;

        while (timer < 1f)
        {
            timer +=
                Time.deltaTime *
                headKickSpeed;

            playerHead.localRotation =
                Quaternion.Slerp(
                    originalRotation,
                    kickRotation,
                    timer
                );

            yield return null;
        }

        timer = 0f;

        while (timer < 1f)
        {
            timer +=
                Time.deltaTime *
                headKickSpeed;

            playerHead.localRotation =
                Quaternion.Slerp(
                    kickRotation,
                    originalRotation,
                    timer
                );

            yield return null;
        }

        playerHead.localRotation =
            originalRotation;
    }

    private void HideKickImage()
    {
        if (kickImage != null)
        {
            kickImage.gameObject.SetActive(
                false
            );
        }
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