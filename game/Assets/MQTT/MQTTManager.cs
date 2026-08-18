using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

public class MQTTManager : MonoBehaviour
{
    [Header("Broker")]
    public string brokerAddress = "192.168.1.135";
    public int brokerPort = 1883;

    [Header("MQTT")]
    public string topic = "game/noise";

    private IMqttClient mqttClient;
    private MqttClientOptions mqttOptions;

    void Start()
    {
        ConnectMQTT();
    }

    private async void ConnectMQTT()
    {
        try
        {
            var mqttFactory = new MqttFactory();

            mqttClient = mqttFactory.CreateMqttClient();

            mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(brokerAddress, brokerPort)
                .WithClientId(
                    "Unity-" + Guid.NewGuid().ToString()
                )
                .Build();

            // Configura o recebimento ANTES de conectar
            mqttClient.ApplicationMessageReceivedAsync +=
                HandleReceivedMessage;

            await mqttClient.ConnectAsync(
                mqttOptions,
                CancellationToken.None
            );

            Debug.Log("MQTT CONECTADO!");

            var subscribeOptions =
                mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(filter =>
                    {
                        filter.WithTopic(topic);
                        filter.WithQualityOfServiceLevel(
                            MqttQualityOfServiceLevel.AtMostOnce
                        );
                    })
                    .Build();

            await mqttClient.SubscribeAsync(
                subscribeOptions,
                CancellationToken.None
            );

            Debug.Log(
                "Inscrito no topico: " + topic
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Erro ao conectar no MQTT: " + e.Message
            );
        }
    }

    private Task HandleReceivedMessage(
    MqttApplicationMessageReceivedEventArgs e
)
    {
        string message = Encoding.UTF8.GetString(
            e.ApplicationMessage.PayloadSegment
        );

        Debug.Log(
            "MENSAGEM MQTT RECEBIDA: " + message
        );


        UnityMainThreadDispatcher.Enqueue(() =>
        {
            MQTTNoisePoint[] noisePoints =
                FindObjectsByType<MQTTNoisePoint>(
                    FindObjectsSortMode.None
                );

            foreach (MQTTNoisePoint point in noisePoints)
            {

                if (point.eventName == message)
                {
                    Debug.Log(
                        "ENCONTROU O PONTO! Emitindo som."
                    );

                    point.EmitNoise();

                    return;
                }
            }

            Debug.LogWarning(
                "Nenhum ponto corresponde a: " +
                message
            );
        });

        return Task.CompletedTask;
    }

    private async void OnApplicationQuit()
    {
        if (mqttClient != null && mqttClient.IsConnected)
        {
            await mqttClient.DisconnectAsync();
        }
    }
}