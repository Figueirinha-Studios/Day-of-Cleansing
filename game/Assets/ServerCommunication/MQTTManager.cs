using System;
using System.Threading.Tasks;
using UnityEngine;
using MQTTnet;
using MQTTnet.Client;

public class MQTTManager : MonoBehaviour
{
    public static MQTTManager Instance { get; private set; }

    [Header("MQTT")]
    public string broker = "localhost";
    public int port = 1883;

    private IMqttClient mqttClient;

    private async void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await Connect();
    }

    private async Task Connect()
    {
        try
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .Build();

            await mqttClient.ConnectAsync(options);

            Debug.Log("MQTT CONECTADO!");
        }
        catch (Exception e)
        {
            Debug.LogError("ERRO AO CONECTAR MQTT: " + e.Message);
        }
    }

    /// <summary>
    /// Envia uma mensagem MQTT.
    /// Pode ser chamado de qualquer outro script.
    /// </summary>
    public async void Publish(string topic, string message)
    {
        if (mqttClient == null || !mqttClient.IsConnected)
        {
            Debug.LogWarning("MQTT não está conectado. Mensagem não enviada.");
            return;
        }

        try
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

            await mqttClient.PublishAsync(mqttMessage);

            Debug.Log($"MQTT ENVIADO - Topico: {topic} | Mensagem: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError("ERRO AO ENVIAR MQTT: " + e.Message);
        }
    }
}