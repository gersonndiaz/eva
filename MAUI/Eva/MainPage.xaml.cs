using Eva.Helpers.BT;
using Eva.Views;
using Eva.Views.Shared;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;

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

            bluetoothService.DeviceDisconnected += BluetoothService_DeviceDisconnected;
            bluetoothService.DeviceConnectionLost += BluetoothService_DeviceConnectionLost;
        }

        private async void BluetoothService_DeviceConnectionLost(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            await DisplayAlert("Error Bluetooth", $"Se ha perdido la conexión con el dispositivo {e.Device.Name}", "OK");
        }

        private async void BluetoothService_DeviceDisconnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            await DisplayAlert("Error Bluetooth", $"Se ha desconectado el dispositivo {e.Device.Name}", "OK");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var device = await bluetoothService.GetDeviceConnected();
            if (device == null)
            {
                try
                {
                    if (Navigation.NavigationStack.Where(x => x is ConnectDeviceBTPage).Count() > 0)
                        return;
                    Navigation.PushAsync(new ConnectDeviceBTPage());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Debug.WriteLine(ex);
                }
            }
        }

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
    }

}
