import math
import serial
import socket
import time
import mouse

#file = open('joystickData.csv', 'w+')

def twos_comp(val, bits):
    """compute the 2's complement of int value val"""
    if (val & (1 << (bits - 1))) != 0: # if sign bit is set e.g., 8bit: 128-255
        val = val - (1 << bits)        # compute negative value
    return val                         # return positive value as is


ser = serial.Serial(
    port='COM4',
    baudrate=9600,
    parity=serial.PARITY_NONE,
    stopbits=serial.STOPBITS_ONE,
    bytesize=serial.EIGHTBITS)

notFound = True
while(notFound):
    dataPoint = ser.read(1)
    print(dataPoint)
    if(dataPoint.hex() == "27"):
        notFound = False
        dataPoint = ser.read(1)

xdataInDec = 0
ydataInDec = 0
for i in range(1000):
    for x in range(2):
        dataPoint = ser.read(1)
        highPoint = dataPoint
        
        dataPoint = ser.read(1)
        lowPoint = dataPoint
        
        dataInHex = highPoint.hex() +  lowPoint.hex()
        dataInDec = int(dataInHex[1:],16) 

        if(x == 0):
            prevx = xdataInDec
            xdataInDec = dataInDec + 59 - 2048
            val = str(dataInDec) + ","
        else:
            prevy = ydataInDec
            ydataInDec = dataInDec + 30 - 2048
            val = str(dataInDec)
        #file.write(val)

        print(str(dataInDec))

    mouse.move(xdataInDec - prevx ,ydataInDec - prevy, absolute=False)
    
    #file.write("\n")

print("done")
#file.close()
        