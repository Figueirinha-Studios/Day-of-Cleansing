using UnityEngine;
using TMPro;

public class AreaTextTrigger : MonoBehaviour
{
    [Header("Configurações")]
    public GameObject textObject;
    public string playerTag = "Player";

    private void Start()
    {
        textObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            textObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            textObject.SetActive(false);
        }
    }
}