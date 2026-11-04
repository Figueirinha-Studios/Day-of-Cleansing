
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingSequence : MonoBehaviour
{
    [Header("Movimento do jogador")]
    public Behaviour[] playerMovementScripts;

    [Header("Metralhadoras - WeaponMain")]
    public Transform[] weaponMains;

    [Header("Animação")]
    public float animationDuration = 1f;
    public float startY = -3.453f;
    public float targetY = 0f;

    [Header("Som da animação")]
    public AudioSource animationAudioSource;
    public AudioClip animationSound;

    [Header("Finalização")]
    public float sceneChangeDelay = 2f;
    public string finalScene = "FinalScene";

    private bool sequenceStarted = false;

    private Vector3[] initialPositions;

    private void Start()
    {
        initialPositions = new Vector3[weaponMains.Length];

        for (int i = 0; i < weaponMains.Length; i++)
        {
            if (weaponMains[i] != null)
            {
                initialPositions[i] = weaponMains[i].localPosition;

                Vector3 position = weaponMains[i].localPosition;
                position.y = startY;
                weaponMains[i].localPosition = position;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sequenceStarted) return;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(EndingSequenceRoutine());
        }
    }

    private IEnumerator EndingSequenceRoutine()
    {
        sequenceStarted = true;

        // Trava o movimento do jogador
        foreach (Behaviour script in playerMovementScripts)
        {
            if (script != null)
                script.enabled = false;
        }

        // Reproduz o som no início da animação
        if (animationAudioSource != null && animationSound != null)
        {
            animationAudioSource.PlayOneShot(animationSound);
        }

        // Anima as três metralhadoras simultaneamente
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / animationDuration);

            // Suaviza o movimento
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < weaponMains.Length; i++)
            {
                if (weaponMains[i] == null) continue;

                Vector3 position = weaponMains[i].localPosition;

                position.y = Mathf.Lerp(
                    startY,
                    targetY,
                    smoothProgress
                );

                weaponMains[i].localPosition = position;
            }

            yield return null;
        }

        // Garante que todas terminem exatamente na posição final
        for (int i = 0; i < weaponMains.Length; i++)
        {
            if (weaponMains[i] == null) continue;

            Vector3 position = weaponMains[i].localPosition;
            position.y = targetY;
            weaponMains[i].localPosition = position;
        }

        // Aguarda antes de carregar a cena final
        yield return new WaitForSeconds(sceneChangeDelay);

        SceneManager.LoadScene(finalScene);
    }
}
