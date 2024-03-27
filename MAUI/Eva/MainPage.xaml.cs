using Eva.Helpers.BT;
using Eva.Models.Device;
using Eva.Views;
using Eva.Views.Shared;
using Newtonsoft.Json;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

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

            bluetoothService.CharacteristicUpdated += BluetoothService_CharacteristicUpdated;
        }

        private async void BluetoothService_CharacteristicUpdated(object? sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
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
        /// Tap en botón de comandos personalizados
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SendCommand_Tapped(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Comandos personalizados", $"Característica aún no disponible.", "OK");
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
                var device = await bluetoothService.GetDeviceConnected();

                var services = await device.GetServicesAsync();

                ConexionWifiModel conexionWifi = null;

                if (services != null && services.Count > 0)
                {
                    foreach (var service in services)
                    {
                        var characteristicsService = await service.GetCharacteristicsAsync();
                        //var guidWifiCharacteristic = Guid.Parse("9edf7ce0-0839-4d0c-95c6-4511a1cd012d");
                        var guidWifiCharacteristic = Guid.Parse("645cfbf1-ac92-433d-a1e2-fb87bdfc6a96");
                        foreach (var characteristic in characteristicsService)
                        {
                            if (characteristic.Id == guidWifiCharacteristic)
                            {
                                characteristic.ValueUpdated += BluetoothService_CharacteristicUpdated;
                                await characteristic.StartUpdatesAsync();

                                var v = await characteristic.ReadAsync();
                                string val = Encoding.UTF8.GetString(v.data);
                                conexionWifi = JsonConvert.DeserializeObject<ConexionWifiModel>(val);

                            }
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Estado Conexión", $"No se pudo establecer comunicación con el dispositivo. Por favor intentelo nuevamente.", "OK");
                }

                try
                {
                    await Navigation.PopModalAsync();
                }
                catch { }

                if (conexionWifi != null)
                {
                    var actualizar = await DisplayAlert("Estado Conexión", $"El dispositivo {device.Name} se encuentra conectado a la red {conexionWifi.network.ssid}. Su IP actual es {conexionWifi.network.ip}", "Actualizar", "Cerrar");

                    if (actualizar)
                    {
                        try
                        {
                            if (Navigation.NavigationStack.Where(x => x is ConfigWifiToDevicePage).Count() > 0)
                                return;
                            await Navigation.PushAsync(new ConfigWifiToDevicePage());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            Debug.WriteLine(ex);
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Estado Conexión", $"No se pudo recuperar el estado de la conexión WIFI del dispositivo. Por favor intentelo nuevamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);

                try
                {
                    await Navigation.PopModalAsync();
                }
                catch { }

                await DisplayAlert("Estado Conexión", $"Se produjo un error inesperado al consultar por el estado del WIFI del dispositivo. Por favor intentelo nuevamente.", "OK");
            }
        }

        /// <summary>
        /// Detecta el tap sobre el botón de obtener el estado del RTC desde el dispositivo Bluetooth
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetDateTimeFromDevice_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushModalAsync(new LoadingPage("Configuración", "Obteniendo fecha actual del dispositivo..."));

            try
            {
                var device = await bluetoothService.GetDeviceConnected();

                var services = await device.GetServicesAsync();

                if (services != null && services.Count > 0)
                {
                    #region No funciona obtener la característica directamente. Confirmar después
                    ////var rtcCharacteristic = bluetoothService.characteristics.FirstOrDefault(x => x.Key == "RTC");
                    //var guidRtcService = Guid.Parse("ea8dd1ba-fffa-4fe3-854b-c92bbfd4fd37");
                    //var guidRtcGetCharacteristic = Guid.Parse("ebe04058-139b-4cbe-817d-9a9cda7225ca");

                    ////var rtcService = await device.GetServiceAsync(guidRtcService);
                    //var rtcService = services.FirstOrDefault(x => x.Id == guidRtcService);
                    //var rtcCharacteristic = await rtcService.GetCharacteristicAsync(guidRtcGetCharacteristic);

                    ////var v = await rtcCharacteristic.Value.ReadAsync();

                    //rtcCharacteristic.ValueUpdated += BluetoothService_CharacteristicUpdated;
                    //await rtcCharacteristic.StartUpdatesAsync();

                    //var v = await rtcCharacteristic.ReadAsync();
                    ////var v = await rtcCharacteristic.Value
                    //string val = Encoding.UTF8.GetString(v.data);
                    ////string val = rtcCharacteristic.StringValue;
                    //var rtcModel = JsonConvert.DeserializeObject<RtcModel>(val);

                    //if (rtcModel != null)
                    //{
                    //    //var device = await bluetoothService.GetDeviceConnected();

                    //    var actualizar = await DisplayAlert("Estado RTC", $"Estado del RTC del dispositivo {device.Name}: \n\nEstado: {rtcModel.rtc.status}\n\n* Fecha: {rtcModel.rtc.datetime}", "Actualizar", "Cerrar");

                    //    if (actualizar)
                    //    {
                    //        rtcModel.rtc.datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace(" ", "T");
                    //        var jsonToSend = JsonConvert.SerializeObject(rtcModel);

                    //        //await rtcCharacteristic.Value.WriteAsync(Encoding.ASCII.GetBytes(jsonToSend));
                    //        var guidRtcSetCharacteristic = Guid.Parse("71fa80e7-1671-4bb6-b9a0-8b4153e90297");
                    //        var rtcSetCharacteristic = await rtcService.GetCharacteristicAsync(guidRtcSetCharacteristic);
                    //        await rtcSetCharacteristic.WriteAsync(Encoding.ASCII.GetBytes(jsonToSend));

                    //        #region Característica RTC
                    //        try
                    //        {
                    //            var cVal = await rtcCharacteristic.ReadAsync();
                    //            string cValData = Encoding.UTF8.GetString(cVal.data);
                    //            var rtcData = JsonConvert.DeserializeObject<RtcModel>(cValData);

                    //            if (rtcData != null)
                    //            {
                    //                await DisplayAlert("Estado RTC", $"Estado del RTC del dispositivo {device.Name}: \n\nEstado: {rtcData.rtc.status}\n\n* Fecha: {rtcData.rtc.datetime}", "OK");
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            Debug.WriteLine($"{ex}");
                    //            Console.WriteLine($"{ex}");
                    //        }
                    //        #endregion Característica RTC
                    //    }
                    //}
                    //else
                    //{
                    //    await DisplayAlert("Estado Conexión", $"No se pudo recuperar el estado de la conexión WIFI del dispositivo. Por favor intentelo nuevamente.", "OK");
                    //}
                    #endregion No funciona obtener la característica directamente. Confirmar después

                    foreach (var service in services)
                    {
                        var characteristicsService = await service.GetCharacteristicsAsync();
                        var guidRtcSetCharacteristic = Guid.Parse("71fa80e7-1671-4bb6-b9a0-8b4153e90297");
                        foreach (var characteristic in characteristicsService)
                        {
                            if (characteristic.Id ==  guidRtcSetCharacteristic)
                            {
                                characteristic.ValueUpdated += BluetoothService_CharacteristicUpdated;
                                await characteristic.StartUpdatesAsync();

                                var v = await characteristic.ReadAsync();
                                string val = Encoding.UTF8.GetString(v.data);
                                var rtcModel = JsonConvert.DeserializeObject<RtcModel>(val);

                                if (rtcModel != null)
                                {
                                    var actualizar = await DisplayAlert("Estado RTC", $"Estado del RTC del dispositivo {device.Name}: \n\nEstado: {rtcModel.rtc.status}\n\n* Fecha: {rtcModel.rtc.datetime}", "Actualizar", "Cerrar");

                                    if (actualizar)
                                    {
                                        rtcModel.rtc.datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace(" ", "T");
                                        var jsonToSend = JsonConvert.SerializeObject(rtcModel);

                                        await characteristic.WriteAsync(Encoding.ASCII.GetBytes(jsonToSend));

                                        await DisplayAlert("Estado RTC", $"RTC actualizado con éxito", "OK");

                                        //#region Característica RTC
                                        //try
                                        //{

                                        //    var cVal = await rtcCharacteristic.ReadAsync();
                                        //    string cValData = Encoding.UTF8.GetString(cVal.data);
                                        //    var rtcData = JsonConvert.DeserializeObject<RtcModel>(cValData);

                                        //    if (rtcData != null)
                                        //    {
                                        //        await DisplayAlert("Estado RTC", $"Estado del RTC del dispositivo {device.Name}: \n\nEstado: {rtcData.rtc.status}\n\n* Fecha: {rtcData.rtc.datetime}", "OK");
                                        //    }
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    Debug.WriteLine($"{ex}");
                                        //    Console.WriteLine($"{ex}");
                                        //}
                                        //#endregion Característica RTC
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("Estado RTC", $"No se pudo recuperar el estado de la fecha y hora del dispositivo. Por favor intentelo nuevamente.", "OK");
                                }
                            }
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Estado RTC", $"No se pudo establecer comunicación con el dispositivo. Por favor intentelo nuevamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);

                await DisplayAlert("Estado RTC", $"Se produjo un error inesperado al consultar por el estado de la fecha y hora del dispositivo. Por favor intentelo nuevamente.", "OK");
            }

            try
            {
                await Navigation.PopModalAsync();
            }
            catch { }
        }
    }

}
