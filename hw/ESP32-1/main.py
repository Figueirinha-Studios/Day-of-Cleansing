from machine import Pin
from umqtt.robust import MQTTClient
from time import sleep, time
import network
from dotenv import load_env

releA = Pin(16, Pin.OUT, value=1)
releB = Pin(17, Pin.OUT, value=1)
panic_button = Pin(36, Pin.IN)

led_NORMAL = Pin(26, Pin.OUT)
led_ALERTA = Pin(25, Pin.OUT)

game_state = True

env = load_env()

def conecta_wifi():
    wlan = network.WLAN(network.STA_IF)
    wlan.active(True)
    if not wlan.isconnected():
        wlan.connect(env["SSID"], env["PASS"])
        while not wlan.isconnected():
            sleep(1)
    print("Conectado ao WiFi")

def callback(topic, msg):
    global game_state
    print("RECEBIDO: " + str(msg))
    
    if game_state == True:
        if msg == b"RELEAon":
            releA.value(1)
        elif msg == b"RELEAoff":
            releA.value(0)
        elif msg == b"RELEBon":
            releB.value(1)
        elif msg == b"RELEBoff":
            releB.value(0)
    else:
        if msg == b"PANICoff":
            game_state = True

def main():
    conecta_wifi()
    
    client = MQTTClient("esp32", env["BROKER"])
    client.set_callback(callback)
    client.connect()
    client.subscribe(b"game/controller")
    print("Conectado ao Broker MQTT")

    while True:
        global game_state
        
        if panic_button.value() == True:
            game_state = False
            client.publish(b"game/panic", "PANIC");
            releA.value(1)
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
