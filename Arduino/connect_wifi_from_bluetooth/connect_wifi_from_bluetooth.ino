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
#include <RTC.h>
#include <SD.h>
#include <SPI.h>
#include <WiFiS3.h>
#include <Wire.h>

File fileWifiData;

BLEService bleWifiService("2093346a-f97d-4849-8a8c-347696a8935b");                                                                        // UUID para el servicio BLE
BLEService bleHourService("ea8dd1ba-fffa-4fe3-854b-c92bbfd4fd37");                                                                        // UUID para el servicio BLE
BLECharacteristic bleWifiScanCharacteristic("9edf7ce0-0839-4d0c-95c6-4511a1cd012d", BLERead | BLEWrite | BLENotify | BLEBroadcast, 512);  // Para enviar la lista de WiFi
BLECharacteristic bleWifiStatusCharacteristic("645cfbf1-ac92-433d-a1e2-fb87bdfc6a96", BLERead | BLEWrite | BLENotify | BLEBroadcast, 512);  // Para consultar el estado del WiFi
BLECharacteristic bleSetHourCharacteristic("71fa80e7-1671-4bb6-b9a0-8b4153e90297", BLERead | BLEWrite | BLENotify | BLEBroadcast, 512);  // Para establecer la Hora
BLECharacteristic bleGetHourCharacteristic("ebe04058-139b-4cbe-817d-9a9cda7225ca", BLERead | BLEWrite | BLENotify | BLEBroadcast, 512);  // Para obtener la Hora

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

  BLE.setLocalName("Arduino Ckelar");
  BLE.setAdvertisedService(bleWifiService);
  BLE.setAdvertisedService(bleHourService);

  bleWifiService.addCharacteristic(bleWifiScanCharacteristic);
  bleWifiService.addCharacteristic(bleWifiStatusCharacteristic);

  bleHourService.addCharacteristic(bleSetHourCharacteristic);
  bleHourService.addCharacteristic(bleGetHourCharacteristic);

  BLE.addService(bleWifiService);
  BLE.addService(bleHourService);

  BLE.advertise();

  BLE.setEventHandler(BLEConnected, bleConnectHandler);
  BLE.setEventHandler(BLEDisconnected, bleDisconnectHandler);

  bleWifiScanCharacteristic.setEventHandler(BLEWritten, bleCharacteristicWifiWritten);
  bleWifiStatusCharacteristic.setEventHandler(BLEWritten, bleCharacteristicWifiStatus);

  bleSetHourCharacteristic.setEventHandler(BLEWritten, bleCharacteristicRtcWritten);
  bleGetHourCharacteristic.setEventHandler(BLERead, bleCharacteristicRTCCurrent);

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

  // Initialize the RTC
  RTC.begin();

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
  printCurrentRTC();
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
  Serial.print("Evento de escritura de característica de seteo de WIFI: ");

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
 Obtiene el estado actual del WIFI
*/
void bleCharacteristicWifiStatus(BLEDevice central, BLECharacteristic characteristic)
{
  Serial.print("Evento de escritura de característica de estado actual WIFI: ");

  // Esperar 2 segundos para imprimir los datos de la red:
  delay(2000);

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

/*
  Utilizar RTC
*/
void bleCharacteristicRTCCurrent(BLEDevice central, BLECharacteristic characteristic)
{
  Serial.print("Evento RTC de escritura de característica de hora actual: ");

  // Esperar 2 segundos para imprimir los datos de la red:
  delay(2000);

  printCurrentRTC();
}

void bleCharacteristicRtcWritten(BLEDevice central, BLECharacteristic characteristic)
{
  Serial.print("Evento de escritura de característica de seteo de Fecha y Hora: ");

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
  const char* rDatetime = doc["rtc"]["datetime"];

  // Extrae solo la fecha del string datetime
  String datetimeStr = String(rDatetime);

  // Extraer componentes de la fecha y hora
  int year = datetimeStr.substring(0, 4).toInt();
  int month = datetimeStr.substring(5, 7).toInt();
  int day = datetimeStr.substring(8, 10).toInt();
  int hour = datetimeStr.substring(11, 13).toInt();
  int minute = datetimeStr.substring(14, 16).toInt();
  int second = datetimeStr.substring(17, 19).toInt();

  // Imprimir los valores encontrados
  Serial.print("DateTime: ");
  Serial.println(datetimeStr);

  // Tu fecha y hora en formato string
  //char dateTime[] = "2024-02-14T14:30:00";

  //int year, month, day, hour, minute, second;
  // Parsea la fecha y hora del string
  sscanf(rDatetime, "%d-%d-%dT%d:%d:%d", &year, &month, &day, &hour, &minute, &second);
  
  // Crea un objeto RTCTime con la fecha y hora parseadas
  // Asumimos que el horario de verano está inactivo y usamos un valor dummy para el día de la semana
  RTCTime mytime(day, static_cast<Month>(month), year, hour, minute, second, DayOfWeek::SUNDAY, SaveLight::SAVING_TIME_INACTIVE);

  // Guarda la nueva fecha y hora
  RTC.setTime(mytime);

  // Esperar 2 segundos para imprimir los datos de la red:
  delay(2000);

  printCurrentRTC();
}

/*
 Imprimir datos RTC actuales
*/
void printCurrentRTC() {
  
  RTCTime currentTime;

  StaticJsonDocument<1000> doc;
  JsonObject statusRTC = doc.createNestedObject("rtc");

  if (RTC.isRunning()) {
    Serial.println("RTC en ejecución");
    statusRTC["status"] = "Corriendo";
  }
  else {
    Serial.println("RTC detenido");
    statusRTC["status"] = "Detenido";
  }

  /* GET CURRENT TIME FROM RTC */
  RTC.getTime(currentTime);

  /* PRINT CURRENT TIME on Serial */
  Serial.print("Fecha actual: ");
  Serial.print(currentTime);
  //statusRTC["datetime"] = currentTime;

  // Convertir DateTime a String
  String dateStr = String(currentTime.getYear(), DEC) + '-' + 
                    String(Month2int(currentTime.getMonth()), DEC) + '-' + 
                    String(currentTime.getDayOfMonth(), DEC) + ' ' + 
                    String(currentTime.getHour(), DEC) + ':' + 
                    String(currentTime.getMinutes(), DEC) + ':' + 
                    String(currentTime.getSeconds(), DEC);

  statusRTC["datetime"] = dateStr;

  String json;
  serializeJson(doc, json);
  Serial.println(json);

  const char* jsonData = json.c_str();
  //bleWifiScanCharacteristic.writeValue((const uint8_t*)jsonData, json.length());
  bleSetHourCharacteristic.writeValue((const uint8_t*)jsonData, json.length());
}