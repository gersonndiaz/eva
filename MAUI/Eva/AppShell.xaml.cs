using Eva.Helpers.BT;
using Eva.Views;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;

namespace Eva
{
    public partial class AppShell : Shell
    {
        BluetoothService bluetoothService;
        IAdapter adapter;
        IBluetoothLE ble;
        public AppShell()
        {
            InitializeComponent();

            bluetoothService = BluetoothService.Instance;
            adapter = bluetoothService.adapter;
            ble = bluetoothService.ble;

            bluetoothService.BluetoothStateChanged += BluetoothService_BluetoothStateChanged;

            bluetoothService.DeviceConnected += BluetoothService_DeviceConnected;
            bluetoothService.DeviceDisconnected += BluetoothService_DeviceDisconnected;
            bluetoothService.DeviceConnectionLost += BluetoothService_DeviceConnectionLost;
            bluetoothService.DeviceConnectionError += BluetoothService_DeviceConnectionError;
            bluetoothService.DeviceBondStateChanged += BluetoothService_DeviceBondStateChanged;
        }

        private void BluetoothService_BluetoothStateChanged(object? sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
        {
            //throw new NotImplementedException();
        }

        private void BluetoothService_DeviceBondStateChanged(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceBondStateChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void BluetoothService_DeviceConnectionError(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    FontImageSource fontImageSource = new FontImageSource();
                    fontImageSource.FontFamily = "FontSolid";
                    fontImageSource.Glyph = "\ue55d";
                    ConnectionOp.IconImageSource = fontImageSource;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Debug.WriteLine(ex);
                }
            });
        }

        private async void BluetoothService_DeviceConnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    FontImageSource fontImageSource = new FontImageSource();
                    fontImageSource.FontFamily = "FontSolid";
                    fontImageSource.Glyph = "\ue55c";
                    ConnectionOp.IconImageSource = fontImageSource;
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
                    FontImageSource fontImageSource = new FontImageSource();
                    fontImageSource.FontFamily = "FontSolid";
                    fontImageSource.Glyph = "\ue55d";
                    ConnectionOp.IconImageSource = fontImageSource;
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
                    FontImageSource fontImageSource = new FontImageSource();
                    fontImageSource.FontFamily = "FontSolid";
                    fontImageSource.Glyph = "\ue560";
                    ConnectionOp.IconImageSource = fontImageSource;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Debug.WriteLine(ex);
                }
            });
        }

        private async void AboutItem_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (Navigation.NavigationStack.Where(x => x is AboutPage).Count() > 0)
                    return;
                await Navigation.PushAsync(new AboutPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);
            }
        }

        private async void ConnectionOp_Clicked(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var mDevice = await bluetoothService.GetDeviceConnected();
                    
                    if (mDevice == null)
                    {
                        await DisplayAlert("Conexión", "No hay ningún dispositivo conectado!", "OK");
                    }
                    else if (mDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
                    {
                        await DisplayAlert("Conexión", $"Actualmente se encuentra conectado el dispositivo {mDevice.Name}!", "OK");
                    }
                    else if (mDevice.State == Plugin.BLE.Abstractions.DeviceState.Connecting)
                    {
                        await DisplayAlert("Conexión", $"Actualmente se encuentra estableciendo conexión con el dispositivo {mDevice.Name}!", "OK");
                    }
                    else if (mDevice.State == Plugin.BLE.Abstractions.DeviceState.Limited)
                    {
                        await DisplayAlert("Conexión", $"Actualmente se encuentra conectado el dispositivo {mDevice.Name}, pero se encuentra con acceso limitado!", "OK");
                    }
                    else if (mDevice.State == Plugin.BLE.Abstractions.DeviceState.Disconnected)
                    {
                        await DisplayAlert("Conexión", $"Actualmente el dispositivo {mDevice.Name} se encuentra desconectado!", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Debug.WriteLine(ex);
                    await DisplayAlert("Conexión", $"Se ha producido un error al consultar por el estado de conexión del dispositivo!", "OK");
                }
            });
        }
    }
}
