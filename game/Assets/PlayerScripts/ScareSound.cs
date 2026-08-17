using UnityEngine;

public class SomPeriodico : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip som;

    public float esperaInicial = 30f;
    public float intervalo = 5f;

    private float timer;
    private bool comecou = false;

    void Start()
    {
        timer = esperaInicial;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            audioSource.PlayOneShot(som);

            if (!comecou)
            {
                comecou = true;
            }

            timer = intervalo;
        }
    }
}