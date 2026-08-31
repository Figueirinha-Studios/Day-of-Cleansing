using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MQTTnet;
using MQTTnet.Client;

public class MQTTManager : MonoBehaviour
{
    public static MQTTManager Instance { get; private set; }

    [Header("MQTT")]
    public string broker = "localhost";
    public int port = 1883;

    [Header("Botão de Pânico")]
    public string panicTopic = "game/panic";
    public string panicScene = "PanicScene";

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

            // Evento chamado quando uma mensagem chega
            mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .Build();

            await mqttClient.ConnectAsync(options);

            Debug.Log("MQTT CONECTADO!");

            // Subscribe no botão de pânico
            await mqttClient.SubscribeAsync(panicTopic);

            Debug.Log($"MQTT SUBSCRITO - Topico: {panicTopic}");
        }
        catch (Exception e)
        {
            Debug.LogError("ERRO AO CONECTAR MQTT: " + e.Message);
        }
    }

    private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        string topic = e.ApplicationMessage.Topic;
        string message = e.ApplicationMessage.PayloadSegment.ToString();

        Debug.Log(
            $"MQTT RECEBIDO - Topico: {topic} | Mensagem: {message}"
        );

        if (topic == panicTopic)
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log("🚨 BOTÃO DE PÂNICO ATIVADO!");

                SceneManager.LoadScene(panicScene);
            });
        }

        return Task.CompletedTask;
    }

    public async void Publish(string topic, string message)
    {
        if (mqttClient == null || !mqttClient.IsConnected)
        {
            Debug.LogWarning(
                "MQTT não está conectado. Mensagem não enviada."
            );

            return;
        }

        try
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

            await mqttClient.PublishAsync(mqttMessage);

            Debug.Log(
                $"MQTT ENVIADO - Topico: {topic} | Mensagem: {message}"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "ERRO AO ENVIAR MQTT: " + e.Message
            );
        }
    }
}