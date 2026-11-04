
using System.Collections;
using UnityEngine;

public class CarHornEvent : MonoBehaviour
{
    [Header("Buzina")]
    public AudioSource hornAudioSource;
    public AudioClip hornSound;
    public float hornDuration = 4f;

    [Header("Farol")]
    public GameObject beaconModel;
    public Light beaconLight;

    [Min(0.01f)]
    public float blinkInterval = 0.25f;

    [Header("Som e alcance")]
    public float noiseRadius = 200f;

    [Header("Estado")]
    [SerializeField]
    private bool eventTriggered = false;

    private void Start()
    {
        // Farol começa escondido e desligado.
        SetBeaconState(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (eventTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        eventTriggered = true;

        // Avisa o Chappie sobre o barulho.
        NoiseSystem.EmitNoise(transform.position, noiseRadius);

        // Inicia a sequência do carro.
        StartCoroutine(CarHornRoutine());
    }


    private IEnumerator CarHornRoutine()
    {
        float elapsedTime = 0f;
        bool beaconOn = true;

        // Liga o farol imediatamente.
        SetBeaconState(true);

        // Controla o tempo das piscadas.
        float nextBlink = blinkInterval;

        while (elapsedTime < hornDuration)
        {
            // Toca uma buzina.
            if (hornAudioSource != null && hornSound != null)
            {
                hornAudioSource.PlayOneShot(hornSound);
            }

            // Duração de cada buzina e intervalo entre elas.
            float hornTime = 0.4f;
            float silenceTime = 0.3f;

            yield return new WaitForSeconds(hornTime);
            elapsedTime += hornTime;

            yield return new WaitForSeconds(silenceTime);
            elapsedTime += silenceTime;

            // Alterna o farol.
            if (elapsedTime >= nextBlink)
            {
                beaconOn = !beaconOn;
                SetBeaconState(beaconOn);

                nextBlink += blinkInterval;
            }
        }

        // Desliga definitivamente o farol.
        SetBeaconState(false);

        if (hornAudioSource != null)
        {
            hornAudioSource.Stop();
        }
    }


    private void SetBeaconState(bool state)
    {
        if (beaconModel != null)
            beaconModel.SetActive(state);

        if (beaconLight != null)
            beaconLight.enabled = state;
    }
}
