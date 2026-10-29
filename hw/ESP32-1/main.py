from machine import Pin, UART
from umqtt.robust import MQTTClient
from time import sleep, time
import network
from dotenv import load_env

releA = Pin(16, Pin.OUT, value=1)
releB = Pin(17, Pin.OUT, value=1)
panic_button = Pin(23, Pin.IN, Pin.PULL_DOWN)
led_NORMAL = Pin(26, Pin.OUT)
led_ALERTA = Pin(25, Pin.OUT)

uart = UART(1, baudrate=9600, rx=4)

topico_status = b"esp32/status"
topico_controle = b"esp32/controller"
topico_panico = b"esp32/panic"

game_state = True

env = load_env()

tempo = time()

client = None  
wlan = None
precisa_reinstalar_mqtt = False

def serial():
    if uart.any():
        dados_bytes = uart.readline()
        
        try:
            texto = dados_bytes.decode('utf-8').strip()
            print(f"Recebido SERIAL: {texto}")
        except Exception:
            print(f"Recebido (bytes): {dados_bytes}")
            
        if texto == "ABRIR":
            releA.value(0)
            print("Destravando portas")

def envia_estado_tempo():
    global time, tempo
    if (time() - tempo > 15):
        tempo = time()
        msg = ""
        msg += "esp32 RELEA="
        msg += str(releA.value())
        msg += ",RELEB="
        msg += str(releB.value())
        msg += ",GAMESTATE="
        msg += str(int(game_state))
        client.publish(topico_status, msg)
        print("transmitido por tempo: " + msg)
        
def envia_estado_direto():
    msg = ""
    msg += "esp32 RELEA="
    msg += str(releA.value())
    msg += ",RELEB="
    msg += str(releB.value())
    msg += ",GAMESTATE="
    msg += str(int(game_state))
    client.publish(topico_status, msg)
    print("respondeu a mensagem: " + msg)

def conecta_wifi():
    global wlan
    wlan = network.WLAN(network.STA_IF)
    wlan.active(True)
    if not wlan.isconnected():
        wlan.connect(env["SSID"], env["PASS"])
        while not wlan.isconnected():
            sleep(1)
    print("Conectado ao WiFi")

def garante_conexao_mqtt():
    """Recupera a conexão de forma passiva e super rápida, sem travar o loop"""
    global client, wlan, precisa_reinstalar_mqtt
    
    # 1. Se o WiFi caiu, restabelece
    if wlan is None or not wlan.isconnected():
        try:
            conecta_wifi()
        except Exception:
            return False

    # 2. Só tenta reconectar o MQTT se alguma função marcou a flag como True
    if precisa_reinstalar_mqtt:
        print("Recuperando Broker MQTT e renovando inscrições...")
        try:
            if client is not None:
                client.connect()
                client.subscribe(topico_controle)
                precisa_reinstalar_mqtt = False  # Recuperado com sucesso!
                print("Conexão recuperada com sucesso!")
                return True
        except Exception as e:
            print("Aguardando Broker voltar...", e)
            return False
    return True


def callback(topic, msg):
    global game_state
    print("RECEBIDO MQTT: " + str(msg))
    
    if game_state == True:
        if msg == b"RELEAon":
            releA.value(1)
            envia_estado_direto()
        elif msg == b"RELEAoff":
            releA.value(0)
            envia_estado_direto()
        elif msg == b"RELEBon":
            releB.value(1)
            envia_estado_direto()
        elif msg == b"RELEBoff":
            releB.value(0)
            envia_estado_direto()
    else:
        if msg == b"PANICoff":
            game_state = True

def main():
    global client, wlan, game_state
    conecta_wifi()
    client = MQTTClient("esp32", env["BROKER"])
    client.set_callback(callback)
    client.connect()
    client.subscribe(topico_controle)
    print("Conectado ao Broker MQTT")
    envia_estado_direto()

    while True:
        serial()
        garante_conexao_mqtt()
        envia_estado_tempo()
            
        
        if panic_button.value() == True:
            game_state = False
            client.publish(topico_panico, "PANIC");
            releA.value(0)
            releB.value(1)
            print("modo de emergencia INICIADO")
            
        try:
            client.check_msg()
            sleep(0.05)
        except Exception as e:
            print("Erro detectado, reconectando...", e)
            sleep(5)
            machine.reset()

        led_ALERTA.value(not game_state)
        led_NORMAL.value(game_state)
        sleep(0.05)

if __name__ == '__main__':
    main()
