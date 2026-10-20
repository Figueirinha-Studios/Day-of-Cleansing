from mfrc522 import MFRC522
from lcd_api import LcdApi
from pico_i2c_lcd import I2cLcd
import utime
from time import time
from machine import Pin, I2C, PWM, UART, SPI
from st7565 import ST7565

RST_ST = Pin(21, Pin.OUT)
A0_ST = Pin(22, Pin.OUT)
CS_ST = Pin(20, Pin.OUT)
led = Pin(25, Pin.OUT, value=1)
buzzer = PWM(Pin(13))

spibus = SPI(0, baudrate=500000, polarity=0, phase=0, sck=Pin(2), mosi=Pin(3), miso=Pin(4))
display = ST7565(spibus, A0_ST, CS_ST, RST_ST)
utime.sleep(0.05)
rc522 = MFRC522(spibus, cs=Pin(1, Pin.OUT), rst=Pin(0, Pin.OUT))

uart = UART(1, baudrate=9600, tx=Pin(8), rx=Pin(9))

leituras = 0

display.set_contrast(0x26)

display.fill(0)


display.text("DAY OF CLEANSING", 0, 0, 1)
display.text("----------------", 0, 8, 1)
display.text("Ache o cracha", 0, 20, 1)
display.text("no esqueleto", 0, 32, 1)
display.text("das poltronas.", 0, 44, 1)

display.show()


AUTHORIZED_UIDS = {
    "5288911c",
    "526393e5",
    "bee3d63d",
    "61cfd73d",
    "4dd7d63d",
    "e58f4ed3"
}

def tocar_nota(freq, duracao):
    buzzer.freq(freq)
    buzzer.duty_u16(32768)
    utime.sleep(duracao)
    buzzer.duty_u16(0)
    
def abertura():
    print('Abrindo as portas')
    uart.write("ABRIR\n")

print(" LEITOR RFID PRONTO PARA USO    ")

while True:

    (stat, tag_type) = rc522.request(rc522.REQIDL)

    if stat == rc522.OK:
        (stat_anti, raw_uid) = rc522.anticoll()

        if stat_anti == rc522.OK and len(raw_uid) >= 4:
            rfid_data = "{:02x}{:02x}{:02x}{:02x}".format(
                raw_uid[0], raw_uid[1], raw_uid[2], raw_uid[3]
            )
            print(f"-> TAG DETECTADA! UID Hex: {rfid_data}")

            if rfid_data in AUTHORIZED_UIDS:
                abertura()
                tocar_nota(750, 0.2)
                utime.sleep(0.05)
                tocar_nota(1750, 0.2)
                leituras += 1
                display.fill(0)

                display.text("DAY OF CLEANSING", 0, 0, 1)
                display.text("----------------", 0, 8, 1)
                display.text("Acesso concedido", 0, 20, 1)
                display.text("Boa sorte.", 0, 32, 1)
                display.show()

            else:
                tocar_nota(1750, 1.5)
                utime.sleep(0.05)
                
                display.fill(0)
                display.text("DAY OF CLEANSING", 0, 0, 1)
                display.text("----------------", 0, 8, 1)
                display.text("Acesso Negado", 0, 20, 1)
                display.text("Procure de Novo.", 0, 32, 1)
                display.show()
                utime.sleep(4)
                display.fill(0)

                display.text("DAY OF CLEANSING", 0, 0, 1)
                display.text("----------------", 0, 8, 1)
                display.text("Ache o cracha", 0, 20, 1)
                display.text("no esqueleto", 0, 32, 1)
                display.text("das poltronas.", 0, 44, 1)

                display.show()

            utime.sleep(1)

    utime.sleep_ms(50)
