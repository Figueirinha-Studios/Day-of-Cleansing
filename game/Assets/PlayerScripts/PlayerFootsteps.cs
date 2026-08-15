using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Passos - Concreto")]
    public AudioClip[] concreteSteps;

    [Header("Passos - Terra")]
    public AudioClip[] dirtSteps;

    [Header("Sons de Pulo e Queda")]
    public AudioClip jumpSound;
    public AudioClip landingSound;

    [Header("Step Timing")]
    public float walkStepInterval = 0.55f;
    public float runStepInterval = 0.30f;

    [Header("Ground Detection")]
    public float rayDistance = 2f;

    private float stepTimer;

    public void UpdateFootsteps(
        bool moving,
        bool running,
        bool crouching,
        bool grounded)
    {
        // Agachado = sem passos
        if (crouching)
        {
            stepTimer = 0f;
            return;
        }

        // No ar = sem passos
        if (!grounded)
        {
            stepTimer = 0f;
            return;
        }

        // Parado = sem passos
        if (!moving)
        {
            stepTimer = 0f;
            return;
        }

        float interval = running
            ? runStepInterval
            : walkStepInterval;

        stepTimer += Time.deltaTime;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        RaycastHit hit;

        if (!Physics.Raycast(
            transform.position,
            Vector3.down,
            out hit,
            rayDistance))
        {
            Debug.Log("PASSO: não encontrou chão");
            return;
        }

        AudioClip[] selectedClips;

        if (hit.collider.CompareTag("Terra"))
        {
            selectedClips = dirtSteps;
        }
        else
        {
            selectedClips = concreteSteps;
        }

        if (selectedClips == null || selectedClips.Length == 0)
        {
            Debug.Log("PASSO: nenhum áudio configurado para esta superfície");
            return;
        }

        AudioClip clip = selectedClips[
            Random.Range(0, selectedClips.Length)
        ];

        audioSource.PlayOneShot(clip);
    }

    public void PlayJumpSound()
    {
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    public void PlayLandingSound()
    {
        if (landingSound != null)
        {
            audioSource.PlayOneShot(landingSound);
        }
    }
}