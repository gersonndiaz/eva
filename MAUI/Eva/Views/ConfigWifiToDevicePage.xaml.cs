using Eva.Helpers.BT;
using Eva.Models.App;
using Eva.Models.Device;
using Eva.Views.Shared;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Eva.Views;

public partial class ConfigWifiToDevicePage : ContentPage
{
    ObservableCollection<Models.App.Network> networksItems = new ObservableCollection<Models.App.Network>();
    List<Models.App.Network> wifiList;

    BluetoothService bluetoothService;

    public ConfigWifiToDevicePage()
	{
		InitializeComponent();

        bluetoothService = BluetoothService.Instance;
        wifiPicker.ItemsSource = networksItems;

        bluetoothService.CharacteristicUpdated += BluetoothService_CharacteristicUpdated;
    }

    private async void BluetoothService_CharacteristicUpdated(object? sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
    {
        if (bluetoothService.characteristics != null)
        {
            var wifiCharacteristic = bluetoothService.characteristics.FirstOrDefault(x => x.Key == "WIFI");

            if (wifiCharacteristic.Value.Id == e.Characteristic.Id)
            {
                #region Característica WIFI
                try
                {
                    var v = await e.Characteristic.ReadAsync();
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

            networksItems = new ObservableCollection<Network>(wifiList.OrderByDescending(x => x.NetworkRssiInDecibelMilliwatts).ToList());
            wifiPicker.ItemsSource = networksItems;
        }

        try
        {
            await Navigation.PopModalAsync();
        }
        catch { }
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new LoadingPage("Configuración", "Enviando configuración..."));

        try
        {
            var selectedItem = wifiPicker.SelectedItem as Network;
            var password = passwordEntry.Text;

            object wifiConfig = new
            {
                SSID = selectedItem.SSID,
                password = password
            };

            var jsonToSend = JsonConvert.SerializeObject(wifiConfig);

            if (bluetoothService.characteristics != null)
            {
                var wifiCharacteristic = bluetoothService.characteristics.FirstOrDefault(x => x.Key == "WIFI");
                await wifiCharacteristic.Value.WriteAsync(Encoding.ASCII.GetBytes(jsonToSend));

                #region Característica WIFI
                try
                {
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex}");
                    Console.WriteLine($"{ex}");

                    await DisplayAlert("Estado Conexión", $"Se produjo un error inesperado. Por favor intentelo nuevamente.", "OK");
                }
                #endregion Característica WIFI
            }
        }
        catch (Exception ex)
        {
            // Manejo de errores de conexión
            Debug.WriteLine($"{ex}");
            Console.WriteLine($"{ex}");
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }

        try
        {
            await Navigation.PopModalAsync();
        }
        catch { }
    }

    private void wifiPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        btnSetWifi.IsEnabled = true;
    }
}