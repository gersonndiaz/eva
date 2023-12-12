/*
  Este ejemplo, permite recibir configuraciones de red (SSID y Contraseña) para conextarse por WIFI, desde Bluetooth.

  Ejemplo:
   * Respuestas enviadas por Bluetooth: {"network":{"status":"Conectado","ssid":"","ip":"0.0.0.0"}}
   * SSID y Contraseña formato recepción: {"SSID":"NombreWIFI1","password":"1234"}

  Fecha Creado: 07/12/2023
  por Gerson Díaz
 */

#include <ArduinoJson.h>
#include <ArduinoBLE.h>
#include <SD.h>
#include <SPI.h>
#include <WiFiS3.h>
#include <Wire.h>

File fileWifiData;

BLEService bleWifiService("2093346a-f97d-4849-8a8c-347696a8935b");                                                                        // UUID para el servicio BLE
BLECharacteristic bleWifiScanCharacteristic("9edf7ce0-0839-4d0c-95c6-4511a1cd012d", BLERead | BLEWrite | BLENotify | BLEBroadcast, 512);  // Para enviar la lista de Wi-Fi

int status = WL_IDLE_STATUS;

void setup() {
  Serial.begin(9600);

  delay(2000);
  while (!Serial);

  // Inicialización y configuración del BLE
  if (!BLE.begin()) {
    Serial.println("No fue posible inicializar el módulo Bluetooth®!");
    while (1);
  }

  BLE.setLocalName("Arduino_Ckelar");
  BLE.setAdvertisedService(bleWifiService);
  bleWifiService.addCharacteristic(bleWifiScanCharacteristic);
  BLE.addService(bleWifiService);

  BLE.advertise();

  BLE.setEventHandler(BLEConnected, bleConnectHandler);
  BLE.setEventHandler(BLEDisconnected, bleDisconnectHandler);
  bleWifiScanCharacteristic.setEventHandler(BLEWritten, bleCharacteristicWifiWritten);

  Serial.println("Módulo Bluetooth® activado, esperando conexiones...");

  // Inicialización de Wi-Fi
  if (WiFi.status() == WL_NO_MODULE) {
    Serial.println("No fue posible inicializar el módulo WIFI!");
    while (true);
  }

  String fv = WiFi.firmwareVersion();
  if (fv < WIFI_FIRMWARE_LATEST_VERSION) {
    Serial.println("Por favor actualice el firmware!");
  }

  Serial.println("Módulo WIFI® activado, esperando conexiones...");

  // Inicialización de Módulo SD
  if (!SD.begin(10)) { // Asegúrate de que el pin SD coincida con tu configuración
    Serial.println("Falló la inicialización de la SD");
    while (1);
  }
  Serial.println("Módulo SD® activado!");


  initConnectionWifiFromSD(fileWifiData, "wd.ad");
  getWifiListJson();
}

/*
 Evento loop de Arduino
*/
void loop() {
  BLEDevice central = BLE.central();

  if (central) {
    while (central.connected()) {
    }
  }

  //delay(2000);
  //Serial.println("Servicio en ejecución!");
}

/*
 Evento que detecta la conexión de un dispositivo
*/
void bleConnectHandler(BLEDevice central) {
  Serial.print("Dispositivo conectado: ");
  Serial.println(central.address());

  delay(2000);
  getWifiListJson();
}

/*
 Evento que detecta la desconexión de un dispositivo
*/
void bleDisconnectHandler(BLEDevice central) {
  Serial.print("Dispositivo desconectado: ");
  Serial.println(central.address());
}

/*
 Escribe la respuesta de
*/
void bleCharacteristicWifiWritten(BLEDevice central, BLECharacteristic characteristic)
{
  Serial.print("Evento de escritura de característica: ");

  uint8_t characteristicValue[200];
  int bytesRead = characteristic.readValue(characteristicValue, sizeof(characteristicValue));

  Serial.print("Bytes recibidos: ");
  for (int i = 0; i < bytesRead; i++) {
    Serial.print(characteristicValue[i], HEX);
    Serial.print(" ");
  }
  Serial.println();

  String receivedString = "";

  for (int i = 0; i < bytesRead; i++) {
    receivedString += (char)characteristicValue[i];
  }

  Serial.println("Valor: " + receivedString);


  // Convertir JSON a STRING
  const char* jsonString = receivedString.c_str();

  // Parsear datos JSON a STRING
  StaticJsonDocument<200> doc;  // Ajuste el tamaño según su complejidad JSON
  DeserializationError error = deserializeJson(doc, jsonString);

  // Verificar error al parsear los datos
  if (error) {
    Serial.print("Error al parsear el JSON: ");
    Serial.println(error.c_str());
    return;
  }

  // Acceder a ls valores del JSON
  const char* ssid = doc["SSID"];
  const char* password = doc["password"];

  // Imprimir los valores encontrados
  Serial.print("SSID: ");
  Serial.println(ssid);
  Serial.print("Password: ");
  Serial.println(password);

  // Conectar a red WPA/WPA2:
  WiFi.begin(ssid, password);

  // Esperar 2 segundos para imprimir los datos de la red:
  delay(2000);

  saveDataWifiSD(fileWifiData, "wd.ad", jsonString);

  printWifiData();
}

/*
 Escanear redes WIFI disponibles y enviar redes encontradas al dispositivo por Bluetooth
*/
void getWifiListJson() {
  Serial.println("** Enviar conexión actual **");

  IPAddress ip = WiFi.localIP();
  
  StaticJsonDocument<1000> doc;
  JsonObject statusNetwork = doc.createNestedObject("network");
  
  if (ip == "0.0.0.0")
  {
    Serial.println("Conexión WIFI: Desconectado");
    statusNetwork["status"] = "Desconectado";
    statusNetwork["ssid"] = "";
    statusNetwork["ip"] = "";
  }
  else
  {
    
    Serial.println("Conexión WIFI: Conectado");
    Serial.print("Dirección IP: ");
    Serial.println(ip);

    statusNetwork["status"] = "Conectado";
    statusNetwork["ssid"] = WiFi.SSID();
    statusNetwork["ip"] = ip;
  }

  String json;
  serializeJson(doc, json);
  Serial.println(json);

  const char* jsonData = json.c_str();
  bleWifiScanCharacteristic.writeValue((const uint8_t*)jsonData, json.length());
}

/*
 Imprimir red WIFI, tras conectarse a la red seleccionada
*/
void printWifiData() {
  IPAddress ip = WiFi.localIP();
  StaticJsonDocument<1000> doc;
  JsonObject statusNetwork = doc.createNestedObject("network");
  
  if (ip == "0.0.0.0")
  {
    Serial.println("Conexión WIFI: Desconectado");
    statusNetwork["status"] = "Desconectado";
    statusNetwork["ssid"] = "";
    statusNetwork["ip"] = "";
  }
  else
  {
    Serial.println("Conexión WIFI: Conectado");
    Serial.print("Dirección IP: ");
    Serial.println(ip);

    statusNetwork["status"] = "Conectado";
    statusNetwork["ssid"] = WiFi.SSID();
    statusNetwork["ip"] = ip;
  }

  String json;
  serializeJson(doc, json);
  Serial.println(json);

  const char* jsonData = json.c_str();
  bleWifiScanCharacteristic.writeValue((const uint8_t*)jsonData, json.length());
}


/*
  Utilizar SD
*/
bool initConnectionWifiFromSD(File file, const char* fileName)
{
  file = SD.open(fileName);
  if (file) {
    Serial.println("Conexión WIFI:");

    String jsonString;
    while (file.available()) {
      jsonString += (char)file.read();
    }
    file.close();

    // Parsear datos JSON a STRING
    StaticJsonDocument<200> doc;  // Ajuste el tamaño según su complejidad JSON
    DeserializationError error = deserializeJson(doc, jsonString);

    // Verificar error al parsear los datos
    if (error) {
      Serial.print("Error al parsear el JSON: ");
      Serial.println(error.c_str());
      return false;
    }

    // Acceder a ls valores del JSON
    const char* ssid = doc["SSID"];
    const char* password = doc["password"];

    // Conectar a red WPA/WPA2:
    WiFi.begin(ssid, password);

    // Esperar 2 segundos para imprimir los datos de la red:
    delay(2000);

    printWifiData();

    return true;
  } else {
    Serial.println("Error al abrir el archivo!");
    return false;
  }
}

bool saveDataWifiSD(File file, const char* fileName, const char* data)
{
  // Comprueba si el archivo existe
  if (SD.exists(fileName)) {
    // Elimina el archivo
    SD.remove(fileName);
  }

  file = SD.open(fileName, FILE_WRITE);

  if (file) {
    file.println(data);
    file.close();
    Serial.println("Datos WIFI almacenados!");
    return true;
  } else {
    Serial.println("Error al abrir el archivo!");
    return false;
  }
}