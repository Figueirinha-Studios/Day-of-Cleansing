using UnityEngine;

public class RestartESP32 : MonoBehaviour
{
    void Start()
    {
        MQTTManager.Instance.Publish("game/controller", "RELEAon");
        MQTTManager.Instance.Publish("game/controller", "RELEBon");
    }
}