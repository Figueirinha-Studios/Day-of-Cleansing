
using UnityEngine;

public class TrashCanEvent : MonoBehaviour
{
    [Header("Referências")]
    public Rigidbody trashCanRigidbody;
    public AudioSource audioSource;
    public AudioClip fallSound;

    [Header("Som e alcance")]
    [Min(0f)]
    public float noiseRadius = 200f;

    [Header("Estado")]
    [SerializeField]
    private bool eventTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (eventTriggered)
            return;

        // Só ativa quando o jogador entra no gatilho.
        if (!other.CompareTag("Player"))
            return;

        if (trashCanRigidbody == null)
        {
            Debug.LogWarning("TrashCanEvent: Rigidbody da lixeira não foi configurado.", this);
            return;
        }

        eventTriggered = true;

        // Libera a física para a lixeira cair.
        trashCanRigidbody.isKinematic = false;
        trashCanRigidbody.useGravity = true;

        // Reproduz o som e avisa o sistema de ruídos.
        if (audioSource != null && fallSound != null)
        {
            audioSource.PlayOneShot(fallSound);
            NoiseSystem.EmitNoise(trashCanRigidbody.position, noiseRadius);
        }
        else
        {
            Debug.LogWarning("TrashCanEvent: configure o AudioSource e o Fall Sound.", this);
        }
    }
}
