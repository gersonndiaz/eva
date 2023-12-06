using Eva.Helpers.Networks;
using Eva.Models;
using Eva.Views.Shared;
using Newtonsoft.Json;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Eva.Views;

public partial class ConfigWifiToDevicePage : ContentPage
{
    //ObservableCollection<string> jsonItems = new ObservableCollection<string>();
    ObservableCollection<Eva.Models.Network> networksItems = new ObservableCollection<Eva.Models.Network>();
    ObservableCollection<IDevice> deviceList = new ObservableCollection<IDevice>();
    IAdapter adapter;
    IBluetoothLE ble;

    ICharacteristic wifiCharacteristic;

    List<Eva.Models.Network> wifiList;

    public ConfigWifiToDevicePage()
	{
		InitializeComponent();

        jsonPicker.ItemsSource = networksItems;
        deviceListView.ItemsSource = deviceList; // Asumiendo que tienes un ListView para mostrar los dispositivos

        ble = CrossBluetoothLE.Current;
        adapter = CrossBluetoothLE.Current.Adapter;

        adapter.DeviceDiscovered += (s, a) => Console.WriteLine(a.Device);
        adapter.DeviceDisconnected += (s, a) => Console.WriteLine($"Se ha desconectado el dispositivo => {a.Device}");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Navigation.PushModalAsync(new LoadingPage("Buscar WIFI", "Buscando redes disponibles..."));

#if WINDOWS
        var wifiService = new Eva.Platforms.Windows.WifiService();
        wifiList = await wifiService.GetAvailableNetworksAsync();
#endif
#if ANDROID
        var wifiService = new Eva.Platforms.Android.WifiService();
        wifiList = await wifiService.GetAvailableNetworksAsync();
#endif

        if (wifiList != null)
        {
            networksItems.Clear();
            //foreach (var red in wifiList.OrderByDescending(x => x.NetworkRssiInDecibelMilliwatts))
            //{
            //    string calidadSenial = NetworkUtil.GetQualitySignal(red.NetworkRssiInDecibelMilliwatts);

            //    jsonItems.Add($"SSID: {red.SSID} | {red.Band} | Intensidad: {calidadSenial}");
            //}

            //jsonPicker.ItemsSource = jsonItems;

            networksItems = new ObservableCollection<Network>(wifiList.OrderByDescending(x => x.NetworkRssiInDecibelMilliwatts).ToList());
            jsonPicker.ItemsSource = networksItems;
        }

        try
        {
            await Navigation.PopModalAsync();
        }
        catch { }
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        try
        {
            var selectedItem = jsonPicker.SelectedItem?.ToString();
            var password = passwordEntry.Text;

            object wifiConfig = new
            {
                SSID = selectedItem,
                password = password
            };

            var jsonToSend = JsonConvert.SerializeObject(wifiConfig);

            await wifiCharacteristic.WriteAsync(Encoding.ASCII.GetBytes(jsonToSend));
        }
        catch (Exception ex)
        {
            // Manejo de errores de conexión
            Debug.WriteLine($"{ex}");
            Console.WriteLine($"{ex}");
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private async void ConnectToArduinoBluetooth(object sender, EventArgs e)
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (ble.State == BluetoothState.Off)
            {
                // Pedir al usuario que active Bluetooth
                await DisplayAlert("Bluetooth desactivado", "Por favor, activa el Bluetooth", "OK");
                return;
            }

            deviceList.Clear();
            deviceList = new ObservableCollection<IDevice>();

            var tempList = new List<IDevice>();
            adapter.DeviceDiscovered += (s, a) => tempList.Add(a.Device);

            if (!adapter.IsScanning)
            {
                await adapter.StartScanningForDevicesAsync();
            }

            // Detén el escaneo si es necesario después de un tiempo
            await Task.Delay(10000);
            await adapter.StopScanningForDevicesAsync();

            foreach (var device in tempList)
            {
                try
                {
                    if (device != null && !String.IsNullOrEmpty(device.Name) && device.Name.ToLower().Contains("ckelar"))
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

            deviceListView.ItemsSource = deviceList;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex}");
            Console.WriteLine($"{ex}");
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void DeviceSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is IDevice selectedDevice)
        {
            ConnectToDevice(selectedDevice);
        }
    }

    private async void ConnectToDevice(IDevice device)
    {
        try
        {

            //device.UpdateConnectionInterval(ConnectionInterval.Low);
            await adapter.ConnectToDeviceAsync(device);

            if (device.State == DeviceState.Connected)
            {
                var services = await device.GetServicesAsync();

                foreach (var service in services)
                {
                    Console.WriteLine($"Servicio encontrado: {service.Id}");

                    var characteristics = await service.GetCharacteristicsAsync();
                    foreach (var characteristic in characteristics)
                    {
                        try
                        {
                            Console.WriteLine($"Característica encontrada: {characteristic.Id}");
                            Debug.WriteLine($"Característica encontrada: {characteristic.Id}");

                            var v = await characteristic.ReadAsync();
                            string val = Encoding.UTF8.GetString(v.data);
                            Console.WriteLine($"Valor de Característica: {val}");
                            Debug.WriteLine($"Valor de Característica: {val}");

                            try
                            {
                                RedesJson redesWifi = JsonConvert.DeserializeObject<RedesJson>(val);

                                if (redesWifi != null)
                                {
                                    wifiCharacteristic = characteristic;

                                    //Device.BeginInvokeOnMainThread(() =>
                                    //{
                                    //    networksItems.Clear();
                                    //    foreach (var red in redesWifi.networks)
                                    //    {
                                    //        networksItems.Add(red.SSID);
                                    //    }

                                    //    jsonPicker.ItemsSource = networksItems;
                                    //});
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"{ex}");
                                Console.WriteLine($"{ex}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"{ex}");
                            Console.WriteLine($"{ex}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Manejo de errores de conexión
            Debug.WriteLine($"{ex}");
            Console.WriteLine($"{ex}");
            await DisplayAlert("Error", ex.Message, "OK");

            if (device.State == DeviceState.Disconnected)
            {
                device.Dispose();
            }
        }
    }

}