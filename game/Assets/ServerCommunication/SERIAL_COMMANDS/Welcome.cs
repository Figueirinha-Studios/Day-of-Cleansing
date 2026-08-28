using UnityEngine;

public class Welcome : MonoBehaviour
{
    void Start()
    {
        SerialManager.Instance.Enviar("limpa");
        SerialManager.Instance.Enviar("Day of CleansingThe last dance.");
    }
}
