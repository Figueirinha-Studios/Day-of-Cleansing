using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Passos")]
    public AudioClip stepSound1;
    public AudioClip stepSound2;

    [Header("Tempo")]
    public float normalStepInterval = 0.5f;
    public float chaseStepInterval = 0.25f;

    private float stepTimer;
    private bool nextStepIsFirst = true;

    public void UpdateFootsteps(bool isMoving, bool isChasing)
    {
        if (!isMoving)
        {
            stepTimer = 0f;
            nextStepIsFirst = true;
            return;
        }

        float stepInterval = isChasing
            ? chaseStepInterval
            : normalStepInterval;

        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        if (nextStepIsFirst)
        {
            if (stepSound1 != null)
                audioSource.PlayOneShot(stepSound1);
        }
        else
        {
            if (stepSound2 != null)
                audioSource.PlayOneShot(stepSound2);
        }

        nextStepIsFirst = !nextStepIsFirst;
    }
}