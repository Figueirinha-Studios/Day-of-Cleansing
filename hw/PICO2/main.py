import machine, time, sys, uselect
from lcd_api import LcdApi
from pico_i2c_lcd import I2cLcd

buzzer = machine.PWM(machine.Pin(28))
sda=machine.Pin(26)
scl=machine.Pin(27)
i2c=machine.SoftI2C(sda=sda, scl=scl, freq=100000)
print(i2c.scan())

lcd = I2cLcd(i2c, 0x27, 2, 16)
lcd.putstr("Text goes here!")

poll = uselect.poll()
poll.register(sys.stdin, uselect.POLLIN)

def tocar_nota(freq, duracao):
    buzzer.freq(freq)
    buzzer.duty_u16(32768)
    time.sleep(duracao)
    buzzer.duty_u16(0)

buffer = ""

while True:
    if poll.poll(0):
        char = sys.stdin.read(1)

        if char:
            if char == "\n":
                mensagem = buffer.strip()
                buffer = ""
                
                if mensagem == 'limpa':
                    lcd.clear()
                elif mensagem:
                    print("RECEBI:", repr(mensagem))
                    lcd.clear()
                    lcd.putstr(mensagem)
                
                
            else:
                buffer += char

    time.sleep(0.01)
