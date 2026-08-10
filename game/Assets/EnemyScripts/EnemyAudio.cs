using UnityEngine;
using System.Collections;

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

    [Header("Intervalo entre os dois sons")]
    public float secondSoundDelay = 0.1f;

    private float stepTimer;

    public void UpdateFootsteps(bool isMoving, bool isChasing)
    {
        if (!isMoving)
        {
            stepTimer = 0f;
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
        StartCoroutine(PlayFootstepSequence());
    }

    private IEnumerator PlayFootstepSequence()
    {
        if (stepSound1 != null)
        {
            audioSource.PlayOneShot(stepSound1);
        }

        yield return new WaitForSeconds(secondSoundDelay);

        if (stepSound2 != null)
        {
            audioSource.PlayOneShot(stepSound2);
        }
    }
}