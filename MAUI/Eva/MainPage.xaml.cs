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
