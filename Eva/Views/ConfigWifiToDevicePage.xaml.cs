using Eva.Helpers.BT;
using Eva.Models.App;
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

            //await wifiCharacteristic.WriteAsync(Encoding.ASCII.GetBytes(jsonToSend));

            if (bluetoothService.characteristics != null)
            {
                var wifiCharacteristic = bluetoothService.characteristics.FirstOrDefault(x => x.Key == "WIFI");
                await wifiCharacteristic.Value.WriteAsync(Encoding.ASCII.GetBytes(jsonToSend));
            }
        }
        catch (Exception ex)
        {
            // Manejo de errores de conexión
            Debug.WriteLine($"{ex}");
            Console.WriteLine($"{ex}");
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private void wifiPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        btnSetWifi.IsEnabled = true;
    }
}