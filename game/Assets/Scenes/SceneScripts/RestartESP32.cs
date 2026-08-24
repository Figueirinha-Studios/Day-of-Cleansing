using UnityEngine;

public class RestartESP32 : MonoBehaviour
{
    void Start()
    {
        MQTTManager.Instance.Publish("game/controller", "RELEAoff");
        MQTTManager.Instance.Publish("game/controller", "RELEBoff");
    }
}