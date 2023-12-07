using Eva.Models.Device;
using Newtonsoft.Json;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Diagnostics;
using System.Text;

namespace Eva.Helpers.BT
{
    public class BluetoothService
    {
        private static BluetoothService _instance;
        public IAdapter adapter;
        public IBluetoothLE ble;

        IDevice device;

        public Dictionary<string, ICharacteristic> characteristics;

        // Eventos Adaptador
        public event EventHandler<DeviceEventArgs> DeviceConnected;
        public event EventHandler<DeviceEventArgs> DeviceDisconnected;
        public event EventHandler<DeviceEventArgs> DeviceDiscovered;
        public event EventHandler<DeviceEventArgs> DeviceConnectionLost;
        public event EventHandler<DeviceEventArgs> DeviceConnectionError;

        // Eventos Characteristics
        public event EventHandler<CharacteristicUpdatedEventArgs> CharacteristicUpdated;

        private BluetoothService()
        {
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;

            adapter.DeviceConnected += Adapter_DeviceConnected;
            adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
            adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
            adapter.DeviceConnectionError += Adapter_DeviceConnectionError;
        }

        #region Eventos Adaptador Bluetooth
        /// <summary>
        /// Detecta cuando se ha conectado un dispositivo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Adapter_DeviceConnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            DeviceConnected?.Invoke(this, e);
        }

        /// <summary>
        /// Detecta cuando el dispositivo se ha desconectado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Adapter_DeviceDisconnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            DeviceDisconnected?.Invoke(this, e);
        }

        /// <summary>
        /// Detecta cuando se ha descubierto un dispositivo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Adapter_DeviceDiscovered(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            DeviceDiscovered.Invoke(this, e);
        }

        /// <summary>
        /// Identifica cuando se ha perdido la conexión del bluetooth
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Adapter_DeviceConnectionLost(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            DeviceConnectionLost.Invoke(this, e);
        }

        /// <summary>
        /// Identifica si hubo un error de conexión con el dispositivo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Adapter_DeviceConnectionError(object? sender, DeviceErrorEventArgs e)
        {
            DeviceConnectionError.Invoke(this, e);
        }
        #endregion Eventos Adaptador Bluetooth

        /// <summary>
        /// Obtiene la instancia del servicio de Bluetooth
        /// </summary>
        public static BluetoothService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BluetoothService();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Obtiene el dispositivo que se encuentra actualmente conectado
        /// </summary>
        /// <returns></returns>
        public async Task<IDevice> GetDeviceConnected()
        {
            return device;
        }

        /// <summary>
        /// Obtiene los dispositivos Bluetooth cercanos
        /// </summary>
        /// <param name="timeout">(Opcional) Tiempo en segundos (1s a 20s) para detener el escaneo.</param>
        /// <returns></returns>
        public async Task<List<IDevice>> FindDevicesBluetooth(int? timeout)
        {
            try
            {
                //var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                //if (status != PermissionStatus.Granted)
                //{
                //    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                //}

                //if (ble.State == BluetoothState.Off)
                //{
                //    // Pedir al usuario que active Bluetooth
                //    await DisplayAlert("Bluetooth desactivado", "Por favor, activa el Bluetooth", "OK");
                //    return;
                //}

                var deviceList = new List<IDevice>();

                var tempList = new List<IDevice>();
                adapter.DeviceDiscovered += (s, a) => tempList.Add(a.Device);

                if (!adapter.IsScanning)
                {
                    await adapter.StartScanningForDevicesAsync();
                }

                // Detén el escaneo si es necesario después de un tiempo
                if (timeout.HasValue && timeout.Value >= 1 && timeout.Value <= 20)
                {
                    await Task.Delay((timeout.Value*1000));
                    await adapter.StopScanningForDevicesAsync();
                }

                foreach (var device in tempList)
                {
                    try
                    {
                        if (device != null && !String.IsNullOrEmpty(device.Name))
                        {
                            if (!deviceList.Any(x => x.Id == device.Id))
                            {
                                deviceList.Add(device); // Aquí, deviceList es tu ObservableCollection
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{ex}");
                        Console.WriteLine($"{ex}");
                    }
                }

                return deviceList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                Console.WriteLine($"{ex}");
                throw;
            }
        }

        /// <summary>
        /// Conectar dispositivo seleccionado
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<bool> ConnectToDevice(IDevice device)
        {
            try
            {
                //device.UpdateConnectionInterval(ConnectionInterval.Low);
                await adapter.ConnectToDeviceAsync(device);

                if (device.State == DeviceState.Connected)
                {
                    this.device = device;
                    var services = await device.GetServicesAsync();

                    characteristics = new Dictionary<string, ICharacteristic>();

                    foreach (var service in services)
                    {
                        Console.WriteLine($"Servicio encontrado: {service.Id}");

                        var characteristicsService = await service.GetCharacteristicsAsync();
                        foreach (var characteristic in characteristicsService)
                        {
                            characteristic.ValueUpdated += Characteristic_ValueUpdated;

                            #region Característica WIFI
                            try
                            {
                                var v = await characteristic.ReadAsync();
                                string val = Encoding.UTF8.GetString(v.data);
                                var conexionWifi = JsonConvert.DeserializeObject<ConexionWifiModel>(val);

                                if (conexionWifi != null)
                                {
                                    characteristics.Add("WIFI", characteristic);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"{ex}");
                                Console.WriteLine($"{ex}");
                            }
                            #endregion Característica WIFI
                        }
                    }
                }

                return (device.State == DeviceState.Connected);
            }
            catch (Exception ex)
            {
                // Manejo de errores de conexión
                Debug.WriteLine($"{ex}");
                Console.WriteLine($"{ex}");
                if (device.State == DeviceState.Disconnected)
                {
                    device.Dispose();
                }

                throw;
            }
        }

        /// <summary>
        /// Envía un dato hacia el dispositivo, sobre la característica especificada
        /// </summary>
        /// <param name="characteristic"></param>
        /// <param name="data"></param>
        public async Task<bool> OnSendDataToDevice(ICharacteristic characteristic, string data)
        {
            try
            {
                int response = await characteristic.WriteAsync(Encoding.ASCII.GetBytes(data));

                return (response > 0);
            }
            catch (Exception ex)
            {
                // Manejo de errores de conexión
                Debug.WriteLine($"{ex}");
                Console.WriteLine($"{ex}");
                throw;
            }
        }

        #region Eventos Characteristics
        /// <summary>
        /// Recibe información desde el dispositivo conectado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Characteristic_ValueUpdated(object? sender, CharacteristicUpdatedEventArgs e)
        {
            CharacteristicUpdated.Invoke(sender, e);

            try
            {
                Console.WriteLine($"Característica encontrada: {e.Characteristic.Id}");
                Debug.WriteLine($"Característica encontrada: {e.Characteristic.Id}");

                var v = await e.Characteristic.ReadAsync();
                string val = Encoding.UTF8.GetString(v.data);
                Console.WriteLine($"Valor de Característica: {val}");
                Debug.WriteLine($"Valor de Característica: {val}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                Console.WriteLine($"{ex}");
            }
        }
        #endregion Eventos Characteristics
    }
}
