using UnityEngine;

public class GeneralMQTTPublisher : MonoBehaviour
{
    [Header("Mensagens MQTT")]
    public string MQTTMessageA;
    public string MQTTMessageB;
    public bool Publish;

    void Start()
    {
        if (Publish == true) {
            MQTTManager.Instance.Publish("dayofcleansing/controller", MQTTMessageA);
            MQTTManager.Instance.Publish("dayofcleansing/controller", MQTTMessageB);
        }
    }

}
