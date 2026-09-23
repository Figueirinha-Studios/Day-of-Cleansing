using UnityEngine;

public class RestartESP32 : MonoBehaviour
{
    void Start()
    {
        MQTTManager.Instance.Publish("dayofcleansing/controller", "RELEAon");
        MQTTManager.Instance.Publish("dayofcleansing/controller", "RELEBon");
    }
}