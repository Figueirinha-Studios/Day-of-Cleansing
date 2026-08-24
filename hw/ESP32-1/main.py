from machine import Pin
from umqtt.robust import MQTTClient
from time import sleep
import network
from dotenv import load_env

releA = Pin(16, Pin.OUT, value=0)
releB = Pin(17, Pin.OUT, value=0)

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
    if msg == b"RELEAon":
        releA.value(1)
    elif msg == b"RELEAoff":
        releA.value(0)
    elif msg == b"RELEBon":
        releB.value(1)
    elif msg == b"RELEBoff":
        releB.value(0)

def main():
    conecta_wifi()
    
    client = MQTTClient("esp32", env["BROKER"])
    client.set_callback(callback)
    client.connect()
    client.subscribe(b"game/controller")
    print("Conectado ao Broker MQTT")

    while True:
        try:
            client.check_msg()
            sleep(0.5)
        except Exception as e:
            print("Erro detectado, reconectando...", e)
            sleep(5)
            machine.reset()

if __name__ == '__main__':
    main()
