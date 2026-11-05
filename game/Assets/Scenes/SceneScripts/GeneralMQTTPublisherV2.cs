using UnityEngine;

public class GeneralMQTTPublisherV2 : MonoBehaviour
{
    [Header("Mensagens MQTT")]
    public string MQTTMessage;
    public string Topic;
    public bool Publish;

    void Start()
    {
        if (Publish == true)
        {
            MQTTManager.Instance.Publish(Topic, MQTTMessage);
        }
    }

}
