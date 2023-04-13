import math
import serial
import socket
import time

#file = open('joystickData.csv', 'w+')

def twos_comp(val, bits):
    """compute the 2's complement of int value val"""
    if (val & (1 << (bits - 1))) != 0: # if sign bit is set e.g., 8bit: 128-255
        val = val - (1 << bits)        # compute negative value
    return val                         # return positive value as is


if __name__ == "__main__":

    print("Starting socket ... ")
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
        #s.bind(('10.0.0.117', 23456))
        s.bind(('127.0.0.1', 13467))
        s.listen()
        conn, addr = s.accept()
        print(f"Connected by {addr}")

        ser = serial.Serial(
            port='COM4',
            baudrate=9600,
            parity=serial.PARITY_NONE,
            stopbits=serial.STOPBITS_ONE,
            bytesize=serial.EIGHTBITS)
        xdataInDec = 0
        ydataInDec = 0
        currentRead = 0
        checkforDump = ser.read(1)
        checkforDumpHex = checkforDump.hex()
        print(checkforDumpHex)
        firstTime = True
        while(checkforDumpHex == '61'):
            checkforDump = ser.read(1)
            checkforDumpHex = checkforDump.hex()
            print(checkforDumpHex)
        firstTime = False
        dataPoint = ser.read(1)
        print("entering loop")
#         notFound = True
#         while(notFound):
#             dataPoint = ser.read(1)
#             print(dataPoint)
#             if(dataPoint.hex() == "27"):
#                 notFound = False
#                 dataPoint = ser.read(1)

        for i in range(600):
            toSend = ""
            dataPoint = ser.read(1)
            xPoint = dataPoint

            xPointhex = xPoint.hex()
            print("hex x point", xPointhex)
            if(xPointhex == "01"):
                toSend = "XX,XX"
                conn.sendall(str.encode(toSend + '\n'))
            else:
                dataPoint = ser.read(1)
                yPoint = dataPoint

                yPointhex = yPoint.hex()
                print("hex y point",yPointhex)
                if(yPointhex == "01"):
                    toSend = "XX,XX"
                    conn.sendall(str.encode(toSend + '\n'))
                    dataPoint = ser.read(1)
                else:
                    xdataInDec = twos_comp(int(xPointhex, 16), 8)
                    ydataInDec = twos_comp(int(yPointhex, 16), 8)
                    toSend = str(xdataInDec) + "," + str(ydataInDec)
                    print("Sending: ", toSend)
                    conn.sendall(str.encode(toSend + '\n'))

        conn.sendall(str.encode("done" + '\n'))
        print("Sending Done")
        while 1:
            data = conn.recv(1024)
            print(data.decode())
    print("done")
    #file.close()
    conn.close()
        
