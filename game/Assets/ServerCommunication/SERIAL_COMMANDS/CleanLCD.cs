using UnityEngine;

public class CleanLCD : MonoBehaviour
{
    void Start()
    {
        SerialManager.Instance.Enviar("limpa");
    }
}