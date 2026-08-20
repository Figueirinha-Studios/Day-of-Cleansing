using UnityEngine;
using NativeWebSocket;

public class WebSocketManager : MonoBehaviour
{
    [Header("Servidor")]
    public string serverUrl = "0.0.0.0";

    [Header("Objetos do jogo")]
    public Transform player;
    public Transform robot;

    [Header("Atualização")]
    public float updateInterval = 0.1f;

    private WebSocket websocket;
    private float updateTimer;

    [System.Serializable]
    public class WebSocketMessage
    {
        public string type;
    }

    [System.Serializable]
    public class PositionData
    {
        public string type;
        public Position player;
        public Position robot;
    }

    [System.Serializable]
    public class Position
    {
        public float x;
        public float y;
        public float z;
    }

    async void Start()
    {
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("WEBSOCKET CONECTADO!");
            IdentifyClient();
        };

        websocket.OnError += (error) =>
        {
            Debug.LogError(
                "ERRO WEBSOCKET: " + error
            );
        };

        websocket.OnClose += (code) =>
        {
            Debug.Log(
                "WEBSOCKET DESCONECTADO: " + code
            );
        };

        websocket.OnMessage += (bytes) =>
        {
            string message =
                System.Text.Encoding.UTF8.GetString(bytes);

            Debug.Log(
                "WEBSOCKET RECEBIDO: " + message
            );

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                HandleMessage(message);
            });
        };

        await websocket.Connect();
    }

    void Update()
    {
        if (websocket == null)
            return;

        if (websocket.State != WebSocketState.Open)
            return;

        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            SendPositions();
        }
    }

    async void SendPositions()
    {
        if (player == null || robot == null)
            return;

        PositionData data = new PositionData();

        data.type = "positions";

        data.player = new Position
        {
            x = player.position.x,
            y = player.position.y,
            z = player.position.z
        };

        data.robot = new Position
        {
            x = robot.position.x,
            y = robot.position.y,
            z = robot.position.z
        };

        string json = JsonUtility.ToJson(data);

        await websocket.SendText(json);
    }

    void HandleMessage(string message)
    {
        message = message.Trim();

        Debug.Log(
            "JSON RECEBIDO: " + message
        );

        WebSocketMessage data;

        try
        {
            data = JsonUtility.FromJson<WebSocketMessage>(message);
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "Erro ao interpretar JSON: " + e.Message
            );

            return;
        }

        if (data == null || string.IsNullOrEmpty(data.type))
        {
            Debug.LogWarning(
                "Mensagem WebSocket sem campo 'type'."
            );

            return;
        }

        Debug.Log(
            "TIPO RECEBIDO: " + data.type
        );

        RemoteNoisePoint[] noisePoints =
            FindObjectsByType<RemoteNoisePoint>(
                FindObjectsSortMode.None
            );

        foreach (RemoteNoisePoint point in noisePoints)
        {
            Debug.Log(
                "Ponto: " + point.name +
                " | Evento: " + point.eventName
            );

            if (point.eventName.Trim() == data.type.Trim())
            {
                Debug.Log(
                    "EVENTO ENCONTRADO! Emitindo som: " +
                    data.type
                );

                point.EmitNoise();
                return;
            }
        }

        Debug.LogWarning(
            "Nenhum RemoteNoisePoint encontrado para: " +
            data.type
        );
    }

    async void OnApplicationQuit()
    {
        if (websocket != null &&
            websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
    async void IdentifyClient()
    {
        if (websocket == null ||
            websocket.State != WebSocketState.Open)
        {
            return;
        }

        string identifyMessage =
            "{\"type\":\"identify\",\"client\":\"unity\"}";

        await websocket.SendText(identifyMessage);

        Debug.Log(
            "IDENTIFICAÇÃO ENVIADA AO SERVIDOR: unity"
        );
    }
}