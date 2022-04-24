print("IoT Gateway")
import paho.mqtt.client as mqttclient
import time
import json
import serial.tools.list_ports
import winrt.windows.devices.geolocation as wdg, asyncio

async def getCoords():
    loc = wdg.Geolocator()
    pos = await loc.get_geoposition_async()
    return [pos.coordinate.latitude, pos.coordinate.longitude]
def getLoc():
    return asyncio.run(getCoords())
BROKER_ADDRESS = "demo.thingsboard.io"
PORT = 1883
mess = ""

#TODO: Add your token and your comport
#Please check the comport in the device manager
THINGS_BOARD_ACCESS_TOKEN = "XuEHuPrSGBjGZY5JcNKS"
bbc_port = ""
if len(bbc_port) > 0:
    ser = serial.Serial(port=bbc_port, baudrate=115200)

def processData(data):
    data = data.replace("!", "")
    data = data.replace("#", "")
    splitData = data.split(":")
    print(splitData)
    #TODO: Add your source code to publish data to the server
    try:
        if splitData[1] == "TEMP":
            _temp = {'temperature': splitData[2]}
            client.publish('v1/devices/me/telemetry', json.dumps(_temp), 1)
        elif splitData[1] == "LIGHT":
            _light = {'light': splitData[2]}
            client.publish('v1/devices/me/telemetry', json.dumps(_light), 1)
        elif splitData[1] == "HUMI":
            _humi = {'humidity': splitData[2]}
            client.publish('v1/devices/me/telemetry', json.dumps(_humi), 1)
    except:
        pass

def readSerial():
    bytesToRead = ser.inWaiting()
    if (bytesToRead > 0):
        global mess
        mess = mess + ser.read(bytesToRead).decode("UTF-8")
        while ("#" in mess) and ("!" in mess):
            start = mess.find("!")
            end = mess.find("#")
            processData(mess[start:end + 1])
            if (end == len(mess)):
                mess = ""
            else:
                mess = mess[end+1:]


def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")

def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    temp_data = {'value': True}
    cmd = -1
    #TODO: Update the cmd to control 2 devices
    try:
        jsonobj = json.loads(message.payload)
        if jsonobj['method'] == "setLED":
            temp_data['valueLED'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
            if temp_data['valueLED']:
                cmd = 1
            else:
                cmd = 0
        if jsonobj['method'] == "setFAN":
            temp_data['valueFAN'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
            if temp_data['valueFAN']:
                cmd = 3
            else:
                cmd = 2
    except:
        pass

    if len(bbc_port) > 0:
        ser.write((str(cmd) + "#").encode())

def connected(client, usedata, flags, rc):
    if rc == 0:
        print("Thingsboard connected successfully!!")
        client.subscribe("v1/devices/me/rpc/request/+")
    else:
        print("Connection is failed")


client = mqttclient.Client("Gateway_Thingsboard")
client.username_pw_set(THINGS_BOARD_ACCESS_TOKEN)

client.on_connect = connected
client.connect(BROKER_ADDRESS, 1883)
client.loop_start()

client.on_subscribe = subscribed
client.on_message = recv_message

temp = 30
humi = 50
light_intensity = 100
while True:

    if (len(bbc_port) >  0):
        readSerial()
    latitude = getLoc()[0]
    longitude = getLoc()[1]
    collect_data = {'temperature': temp, 'humidity': humi, 'light': light_intensity, 'latitude': latitude,
                    'longitude': longitude}

    client.publish('v1/devices/me/telemetry', json.dumps(collect_data), 1)
    time.sleep(1)