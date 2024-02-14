using Eva.Helpers.BT;
using Eva.Models.Device;
using Eva.Views;
using Eva.Views.Shared;
using Newtonsoft.Json;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;
using System.Text;

namespace Eva
{
    public partial class MainPage : ContentPage
    {
        BluetoothService bluetoothService;
        IAdapter adapter;
        IBluetoothLE ble;
        public MainPage()
        {
            InitializeComponent();

            bluetoothService = BluetoothService.Instance;
            adapter = bluetoothService.adapter;
            ble = bluetoothService.ble;

            bluetoothService.BluetoothStateChanged += BluetoothService_BluetoothStateChanged;

            bluetoothService.DeviceDisconnected += BluetoothService_DeviceDisconnected;
            bluetoothService.DeviceConnectionLost += BluetoothService_DeviceConnectionLost;
            bluetoothService.DeviceConnectionError += BluetoothService_DeviceConnectionError;
            bluetoothService.DeviceBondStateChanged += BluetoothService_DeviceBondStateChanged;
        }

        private async void BluetoothService_BluetoothStateChanged(object? sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
        {
            //throw new NotImplementedException();
        }

        private async void BluetoothService_DeviceBondStateChanged(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceBondStateChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Evento que se gatilla al producirse un error en la conexión con el Bluetooth
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BluetoothService_DeviceConnectionError(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await DisplayAlert("Error Bluetooth", $"Ha ocurrido un error con la conexión del dispositivo {e.Device.Name}", "OK");

                    try
                    {
                        if (Navigation.NavigationStack.Where(x => x is ConnectDeviceBTPage).Count() > 0)
                            return;
                        await Navigation.PushAsync(new ConnectDeviceBTPage());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Debug.WriteLine(ex);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Debug.WriteLine(ex);
                }
            });
        }

        /// <summary>
        /// Evento que se gatilla al perder la conexión con el dispositivo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BluetoothService_DeviceConnectionLost(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await DisplayAlert("Error Bluetooth", $"Se ha perdido la conexión con el dispositivo {e.Device.Name}", "OK");

                    try
                    {
                        if (Navigation.NavigationStack.Where(x => x is ConnectDeviceBTPage).Count() > 0)
                            return;
                        await Navigation.PushAsync(new ConnectDeviceBTPage());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Debug.WriteLine(ex);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Debug.WriteLine(ex);
                }
            });
        }

        /// <summary>
        /// Evento que se gatilla al desconectarse del dispositivo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BluetoothService_DeviceDisconnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await DisplayAlert("Error Bluetooth", $"Se ha desconectado el dispositivo {e.Device.Name}", "OK");

                    try
                    {
                        if (Navigation.NavigationStack.Where(x => x is ConnectDeviceBTPage).Count() > 0)
                            return;
                        await Navigation.PushAsync(new ConnectDeviceBTPage());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Debug.WriteLine(ex);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Debug.WriteLine(ex);
                }
            });
        }

        /// <summary>
        /// Carga en pantalla los controles y datos
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var device = await bluetoothService.GetDeviceConnected();
                if (device == null)
                {
                    try
                    {
                        if (Navigation.NavigationStack.Where(x => x is ConnectDeviceBTPage).Count() > 0)
                            return;
                        await Navigation.PushAsync(new ConnectDeviceBTPage());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Debug.WriteLine(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Detecta el tap sobre el botón para realizar la configuración de WIFI desde el móvil hacia el dispositivo Bluetooth
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigWifiToDevice_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (Navigation.NavigationStack.Where(x => x is ConfigWifiToDevicePage).Count() > 0)
                    return;
                Navigation.PushAsync(new ConfigWifiToDevicePage());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Detecta el tap sobre el botón de obtener el estado de la WIFI desde el dispositivo Bluetooth
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetWifiFromDevice_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushModalAsync(new LoadingPage("Configuración", "Obteniendo estado actual WIFI del dispositivo..."));

            try
            {
                if (bluetoothService.characteristics != null)
                {
                    var wifiCharacteristic = bluetoothService.characteristics.FirstOrDefault(x => x.Key == "WIFI");
                    var v = await wifiCharacteristic.Value.ReadAsync();
                    string val = Encoding.UTF8.GetString(v.data);
                    var conexionWifi = JsonConvert.DeserializeObject<ConexionWifiModel>(val);

                    if (conexionWifi != null)
                    {
                        var device = await bluetoothService.GetDeviceConnected();

                        if (conexionWifi.network.status.ToUpper() == "CONECTADO")
                        {
                            await DisplayAlert("Estado Conexión", $"El dispositivo {device.Name} se encuentra conectado a la red {conexionWifi.network.ssid}. Su IP actual es {conexionWifi.network.ip}", "OK");

                            await Navigation.PopAsync();
                        }
                        else
                        {
                            await DisplayAlert("Estado Conexión", $"El dispositivo {device.Name} no pudo conectarse a la red. Por favor intentelo nuevamente.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Estado Conexión", $"No se pudo recuperar el estado de la conexión WIFI del dispositivo. Por favor intentelo nuevamente.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Estado Conexión", $"No se pudo establecer comunicación con el dispositivo. Por favor intentelo nuevamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);

                await DisplayAlert("Estado Conexión", $"Se produjo un error inesperado al consultar por el estado del WIFI del dispositivo. Por favor intentelo nuevamente.", "OK");
            }

            try
            {
                await Navigation.PopModalAsync();
            }
            catch { }
        }

        private async void GetDateTimeFromDevice_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushModalAsync(new LoadingPage("Configuración", "Obteniendo fecha actual del dispositivo..."));

            try
            {
                if (bluetoothService.characteristics != null)
                {
                    var wifiCharacteristic = bluetoothService.characteristics.FirstOrDefault(x => x.Key == "WIFI");
                    var v = await wifiCharacteristic.Value.ReadAsync();
                    string val = Encoding.UTF8.GetString(v.data);
                    var conexionWifi = JsonConvert.DeserializeObject<ConexionWifiModel>(val);

                    if (conexionWifi != null)
                    {
                        var device = await bluetoothService.GetDeviceConnected();

                        if (conexionWifi.network.status.ToUpper() == "CONECTADO")
                        {
                            await DisplayAlert("Estado Conexión", $"El dispositivo {device.Name} se encuentra conectado a la red {conexionWifi.network.ssid}. Su IP actual es {conexionWifi.network.ip}", "OK");

                            await Navigation.PopAsync();
                        }
                        else
                        {
                            await DisplayAlert("Estado Conexión", $"El dispositivo {device.Name} no pudo conectarse a la red. Por favor intentelo nuevamente.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Estado Conexión", $"No se pudo recuperar el estado de la conexión WIFI del dispositivo. Por favor intentelo nuevamente.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Estado Conexión", $"No se pudo establecer comunicación con el dispositivo. Por favor intentelo nuevamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);

                await DisplayAlert("Estado Conexión", $"Se produjo un error inesperado al consultar por el estado del WIFI del dispositivo. Por favor intentelo nuevamente.", "OK");
            }

            try
            {
                await Navigation.PopModalAsync();
            }
            catch { }
        }
    }

}
