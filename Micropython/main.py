from micropython import const
from machine import Timer
import network
import ubluetooth
import ujson

# Definición de constantes para los eventos BLE
_BLE_IRQ_CENTRAL_CONNECT = const(1)  # Evento de conexión de un dispositivo central
_BLE_IRQ_CENTRAL_DISCONNECT = const(2)  # Evento de desconexión de un dispositivo central
_BLE_IRQ_GATTS_WRITE = const(3)  # Evento de escritura en una característica
_BLE_IRQ_GATTS_READ_REQUEST = const(4)  # Evento de solicitud de lectura de una característica

# UUIDs de los servicios y características BLE
_BLE_UUID_WIFI_SERVICE = ubluetooth.UUID('2093346a-f97d-4849-8a8c-347696a8935b')  # Servicio para configurar WiFi
_BLE_UUID_HOUR_SERVICE = ubluetooth.UUID('ea8dd1ba-fffa-4fe3-854b-c92bbfd4fd37')  # Servicio para configurar hora

_BLE_UUID_SET_WIFI_Characteristic = ubluetooth.UUID('9edf7ce0-0839-4d0c-95c6-4511a1cd012d')  # Característica para establecer credenciales WiFi
_BLE_UUID_GET_WIFI_Characteristic = ubluetooth.UUID('645cfbf1-ac92-433d-a1e2-fb87bdfc6a96')  # Característica para obtener estado WiFi
_BLE_UUID_SET_HOUR_Characteristic = ubluetooth.UUID('71fa80e7-1671-4bb6-b9a0-8b4153e90297')  # Característica para establecer hora
_BLE_UUID_GET_HOUR_Characteristic = ubluetooth.UUID('ebe04058-139b-4cbe-817d-9a9cda7225ca')  # Característica para obtener hora

# Inicializador WIFI
wifi = network.WLAN(network.STA_IF)  # Crear una instancia de la interfaz de red WiFi
wifi.active(True)  # Activar la interfaz de red WiFi

class BLEServer:
    def __init__(self):
        self.ble = ubluetooth.BLE()  # Crear una instancia de BLE
        self.ble.active(True)  # Activar el adaptador BLE
        self.ble.irq(self.ble_irq)  # Registrar la función de callback para eventos BLE
        self.timer = Timer(-1)  # Crear una instancia de Timer (no se usa en este código)
        self.connections = set()  # Conjunto para almacenar los handles de conexiones BLE
        self.register_services()  # Registrar los servicios y características BLE
        self.advertise()  # Iniciar la publicidad BLE

    def register_services(self):
        # Registrar los servicios y características BLE
        self.wifi_service_handle = self.ble.gatts_register_services([
            (_BLE_UUID_WIFI_SERVICE, ((_BLE_UUID_SET_WIFI_Characteristic, ubluetooth.FLAG_READ | ubluetooth.FLAG_WRITE | ubluetooth.FLAG_NOTIFY | ubluetooth.FLAG_INDICATE,),
                                  (_BLE_UUID_GET_WIFI_Characteristic, ubluetooth.FLAG_READ | ubluetooth.FLAG_WRITE | ubluetooth.FLAG_NOTIFY | ubluetooth.FLAG_INDICATE,),)),
            (_BLE_UUID_HOUR_SERVICE, ((_BLE_UUID_SET_HOUR_Characteristic, ubluetooth.FLAG_READ | ubluetooth.FLAG_WRITE | ubluetooth.FLAG_NOTIFY | ubluetooth.FLAG_INDICATE,),
                                  (_BLE_UUID_GET_HOUR_Characteristic, ubluetooth.FLAG_READ | ubluetooth.FLAG_WRITE | ubluetooth.FLAG_NOTIFY | ubluetooth.FLAG_INDICATE,),))
        ])

    def ble_irq(self, event, data):
        # Función de callback para manejar eventos BLE
        if event == _BLE_IRQ_CENTRAL_CONNECT:
            conn_handle, _, _ = data  # Obtener el handle de conexión del evento
            print("Dispositivo conectado:", conn_handle)
            self.connections.add(conn_handle)  # Agregar el handle de conexión al conjunto
            self.send_wifi_config()  # Enviar el estado de la conexión WiFi
        elif event == _BLE_IRQ_CENTRAL_DISCONNECT:
            conn_handle, _, _ = data
            print("Dispositivo desconectado:", conn_handle)
            self.connections.discard(conn_handle)  # Eliminar el handle de conexión del conjunto
            self.advertise()  # Iniciar la publicidad BLE nuevamente
        elif event == _BLE_IRQ_GATTS_WRITE:
            conn_handle, value_handle = data  # Obtener el handle de conexión y de valor
            print("Característica escrita:", value_handle)
            if value_handle == self.wifi_service_handle[0][0]:  # _BLE_UUID_SET_WIFI_Characteristic
                value = self.ble.gatts_read(value_handle)  # Leer el valor escrito en la característica
                self.handle_set_wifi_credentials(value)  # Manejar la escritura de credenciales WiFi
        elif event == _BLE_IRQ_GATTS_READ_REQUEST:
            conn_handle, value_handle = data
            if value_handle == self.wifi_service_handle[0][0]:  # _BLE_UUID_SET_WIFI_Characteristic
                value = self.get_wifi_status()  # Obtener el estado de la conexión WiFi
                self.ble.gatts_write(value_handle, value)  # Escribir el estado en la característica
            elif value_handle == self.wifi_service_handle[0][1]:  # _BLE_UUID_GET_WIFI_Characteristic
                value = self.get_wifi_status()
                self.ble.gatts_write(value_handle, value)

    def advertise(self, name='ESP32_BLE_Server'):
        # Configurar y comenzar la publicidad BLE
        name_data = bytes(name, 'utf-8')  # Convertir el nombre a bytes
        adv_data = bytearray(2 + len(name_data))  # Crear un buffer de bytes para los datos de publicidad
        adv_data[0] = len(name_data) + 1  # Longitud del campo de nombre
        adv_data[1] = 0x09  # Tipo de campo (Nombre Completo Local)
        adv_data[2:] = name_data  # Copiar el nombre en los datos de publicidad
        self.ble.gap_advertise(100, adv_data=adv_data)  # Iniciar la publicidad

    def get_wifi_status(self):
        # Obtener el estado de la conexión WiFi y convertirlo a JSON
        network_status = {
            "network": {
                "status": "Conectado" if wifi.isconnected() else "Desconectado",
                "ssid": wifi.config('essid') if wifi.isconnected() else "",
                "ip": wifi.ifconfig()[0] if wifi.isconnected() else "0.0.0.0"
            }
        }
        status_json = ujson.dumps(network_status)  # Convertir el diccionario a JSON
        print("Estado Actual WIFI:", status_json)
        return status_json.encode('utf-8')  # Codificar el JSON a bytes

    def send_wifi_config(self):
        # Enviar el estado de la conexión WiFi a los dispositivos conectados
        status_bytes = self.get_wifi_status()  # Obtener el estado en bytes
        for conn_handle in self.connections:
            try:
                self.ble.gatts_notify(conn_handle, self.wifi_service_handle[0][1], status_bytes)  # Notificar el estado a través de BLE
                print("Datos WIFI enviados:", status_bytes)
            except Exception as e:
                print("Error al notificar el estado WiFi:", e)

    def handle_set_wifi_credentials(self, value):
        # Manejar la escritura de credenciales WiFi desde un dispositivo conectado
        try:
            data = ujson.loads(value)  # Decodificar el JSON recibido
            ssid = data.get("SSID")
            password = data.get("password")
            if ssid and password:
                self.connect_wifi(ssid, password)  # Conectar a la red WiFi con las nuevas credenciales
            else:
                print("JSON de credenciales WiFi inválido")
        except ValueError:
            print("Error al decodificar JSON de credenciales WiFi")

    def connect_wifi(self, ssid, password):
        # Intentar conectar a una red WiFi con las credenciales proporcionadas
        wifi = network.WLAN(network.STA_IF)
        wifi.active(True)
        try:
            wifi.connect(ssid, password)
            while not wifi.isconnected():
                pass
            print("Conexión WiFi establecida:", wifi.ifconfig())
        except Exception as e:
            print("Error al conectar a WiFi:", e)

def main():
    ble_server = BLEServer()  # Crear una instancia del servidor BLE
    print("Servidor BLE iniciado, esperando conexiones...")

    print('Conexión WiFi establecida')
    print(wifi.ifconfig())  # Imprimir información de la conexión WiFi

if __name__ == "__main__":
    main()
