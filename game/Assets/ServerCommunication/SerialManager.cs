using System;
using System.IO.Ports;
using UnityEngine;

public class SerialManager : MonoBehaviour
{
    public static SerialManager Instance;

    [Header("Serial")]
    [SerializeField] private string portName = "COM6";
    [SerializeField] private int baudRate = 115200;

    [Header("Reconexão")]
    [SerializeField] private float reconnectInterval = 2f;

    private SerialPort serialPort;
    private float nextReconnectTime;

    private void Awake()
    {
        // Impede múltiplas instâncias
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Conectar();
    }

    private void Update()
    {
        // Se desconectou, tenta reconectar periodicamente
        if (serialPort == null || !serialPort.IsOpen)
        {
            if (Time.unscaledTime >= nextReconnectTime)
            {
                nextReconnectTime = Time.unscaledTime + reconnectInterval;
                Conectar();
            }
        }
    }

    private void Conectar()
    {
        try
        {
            // Limpa conexão anterior
            if (serialPort != null)
            {
                try
                {
                    if (serialPort.IsOpen)
                        serialPort.Close();
                }
                catch
                {
                    // Ignora erro ao fechar
                }

                serialPort.Dispose();
                serialPort = null;
            }

            serialPort = new SerialPort(portName, baudRate);

            serialPort.NewLine = "\n";
            serialPort.ReadTimeout = 100;
            serialPort.WriteTimeout = 100;

            serialPort.Open();

            Debug.Log($"[SERIAL] Conectado em {portName}");
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                $"[SERIAL] Não foi possível conectar em {portName}: {e.Message}"
            );

            if (serialPort != null)
            {
                serialPort.Dispose();
                serialPort = null;
            }
        }
    }

    public void Enviar(string mensagem)
    {
        if (string.IsNullOrEmpty(mensagem))
            return;

        if (serialPort == null || !serialPort.IsOpen)
        {
            Debug.LogWarning(
                "[SERIAL] Pico não está conectada. Mensagem não enviada: " + mensagem
            );

            return;
        }

        try
        {
            serialPort.WriteLine(mensagem);

            Debug.Log("[SERIAL] Enviado: " + mensagem);
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "[SERIAL] Erro ao enviar: " + e.Message
            );

            try
            {
                serialPort.Close();
            }
            catch
            {
            }
        }
    }

    public bool EstaConectado()
    {
        return serialPort != null && serialPort.IsOpen;
    }

    private void OnApplicationQuit()
    {
        FecharSerial();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            FecharSerial();
            Instance = null;
        }
    }

    private void FecharSerial()
    {
        if (serialPort == null)
            return;

        try
        {
            if (serialPort.IsOpen)
                serialPort.Close();

            serialPort.Dispose();
        }
        catch
        {
        }

        serialPort = null;
    }
}