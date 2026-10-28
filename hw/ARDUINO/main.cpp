#include <SPI.h>
#include <Ethernet.h>
#include <PubSubClient.h>

byte mac[] = { 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED };

const char* mqtt_server = "day-of-cleansing.feira-de-jogos.dev.br"; // Substitua pelo domínio/URL do seu broker
const int mqtt_port = 1883;                         // Porta padrão MQTT (1883)

const char* mqtt_user = ""; 
const char* mqtt_pass = ""; 

const char* topic_sub = "arduino/controller";  // Tópico para RECEBER ordens (on/off)
const char* topic_pub = "arduino/status";   // Tópico para PUBLICAR o estado atual

const int PIN_RELE = 7;

// Instâncias da rede e do cliente MQTT
EthernetClient ethClient;
PubSubClient client(ethClient);

// Variável para armazenar o estado atual do relé
bool statusRele = false; // false = desligado, true = ligado

// Variáveis de controle de tempo (envio periódico a cada 10s)
unsigned long ultimoEnvio = 0;
const unsigned long intervaloEnvio = 10000; // 10000 ms = 10 segundos

void setup() {
  Serial.begin(9600);

  // Configura o pino do relé (Inicia HIGH = Desligado)
  pinMode(PIN_RELE, OUTPUT);
  digitalWrite(PIN_RELE, HIGH);

  // Inicializa a conexão Ethernet via DHCP
  Serial.println(F("Iniciando Ethernet via DHCP..."));
  if (Ethernet.begin(mac) == 0) {
    Serial.println(F("Falha ao configurar Ethernet via DHCP!"));
    while (true);
  }
  
  Serial.print(F("IP Obtido via DHCP: "));
  Serial.println(Ethernet.localIP());

  // Configura o servidor do MQTT usando a URL do Broker
  client.setServer(mqtt_server, mqtt_port);
  client.setCallback(callback);

  delay(1500);
}

void loop() {
  // Garante que a conexão com o broker continue ativa
  if (!client.connected()) {
    reconnect();
  }
  client.loop();
  
  // Renova o lease do DHCP quando necessário
  Ethernet.maintain();

  // Envia o estado do relé periodicamente a cada 10 segundos
  unsigned long tempoAtual = millis();
  if (tempoAtual - ultimoEnvio >= intervaloEnvio) {
    ultimoEnvio = tempoAtual;
    publicarEstado();
  }
}

// Função chamada quando uma mensagem chega no tópico assinado (topic_sub)
void callback(char* topic, byte* payload, unsigned int length) {
  String mensagem = "";
  for (unsigned int i = 0; i < length; i++) {
    mensagem += (char)payload[i];
  }

  mensagem.trim();
  Serial.print(F("Mensagem recebida no topico ["));
  Serial.print(topic);
  Serial.print(F("]: "));
  Serial.println(mensagem);

  // Processa o comando recebido
  if (mensagem.equalsIgnoreCase("on")) {
    digitalWrite(PIN_RELE, LOW); // Liga em nível BAIXO
    statusRele = true;
    publicarEstado();
  } 
  else if (mensagem.equalsIgnoreCase("off")) {
    digitalWrite(PIN_RELE, HIGH); // Desliga em nível ALTO
    statusRele = false;
    publicarEstado();
  }
}

// Função para publicar no formato: "arduino RELEC=off" ou "arduino RELEC=on"
void publicarEstado() {
  String estadoStr = "arduino RELEC=";
  if (statusRele) {
    estadoStr += "on";
  } else {
    estadoStr += "off";
  }

  client.publish(topic_pub, estadoStr.c_str());
  Serial.print(F("Estado publicado: "));
  Serial.println(estadoStr);
}

// Função para reconectar ao broker MQTT caso caia a conexão
void reconnect() {
  while (!client.connected()) {
    Serial.print(F("Tentando conectar ao Broker MQTT: "));
    Serial.println(mqtt_server);

    String clientId = "ArduinoClient-";
    clientId += String(random(0xffff), HEX);

    bool conectado = false;
    if (strlen(mqtt_user) > 0) {
      conectado = client.connect(clientId.c_str(), mqtt_user, mqtt_pass);
    } else {
      conectado = client.connect(clientId.c_str());
    }

    if (conectado) {
      Serial.println(F("Conectado ao Broker MQTT!"));
      client.subscribe(topic_sub);
      publicarEstado();
    } else {
      Serial.print(F("Falha na conexao MQTT, rc="));
      Serial.print(client.state());
      Serial.println(F(" Tentando novamente em 5 segundos..."));
      delay(5000);
    }
  }
}