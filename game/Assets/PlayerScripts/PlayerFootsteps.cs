using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Concrete")]
    public AudioClip[] concreteSteps;

    [Header("Dirt")]
    public AudioClip[] dirtSteps;

    [Header("Step Timing")]
    public float walkStepInterval = 0.55f;
    public float runStepInterval = 0.30f;

    [Header("Ground Detection")]
    public float rayDistance = 2f;

    private float stepTimer;

    public void UpdateFootsteps(
        bool moving,
        bool running,
        bool crouching)
    {
        // Agachado = sem som
        if (crouching)
        {
            stepTimer = 0f;
            return;
        }

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

        if (selectedClips.Length == 0)
            return;

        AudioClip clip = selectedClips[
            Random.Range(0, selectedClips.Length)
        ];

        audioSource.PlayOneShot(clip);
    }
}