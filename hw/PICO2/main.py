import machine, time
from lcd_api import LcdApi
from pico_i2c_lcd import I2cLcd

sda=machine.Pin(26)
scl=machine.Pin(27)
i2c=machine.SoftI2C(sda=sda, scl=scl, freq=100000)
print(i2c.scan())

lcd = I2cLcd(i2c, 0x27, 2, 16)
lcd.putstr("Text goes here!")


a = 0

while True:
    lcd.clear()
    lcd.putstr(str(a))
    a = a + 1
    time.sleep(1)
