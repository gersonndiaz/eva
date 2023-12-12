using Eva.Helpers.BT;
using Eva.Views;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;
using Windows.Devices.Bluetooth;

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

            bluetoothService.DeviceConnected += BluetoothService_DeviceConnected;
            bluetoothService.DeviceDisconnected += BluetoothService_DeviceDisconnected;
            bluetoothService.DeviceConnectionLost += BluetoothService_DeviceConnectionLost;
        }

        private async void BluetoothService_DeviceConnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            FontImageSource fontImageSource = new FontImageSource();
            fontImageSource.FontFamily = "FontSolid";
            fontImageSource.Glyph = "\ue55c";
            ConnectionOp.IconImageSource = fontImageSource;
        }

        private async void BluetoothService_DeviceConnectionLost(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            FontImageSource fontImageSource = new FontImageSource();
            fontImageSource.FontFamily = "FontSolid";
            fontImageSource.Glyph = "\ue55d";
            ConnectionOp.IconImageSource = fontImageSource;
        }

        private async void BluetoothService_DeviceDisconnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            FontImageSource fontImageSource = new FontImageSource();
            fontImageSource.FontFamily = "FontSolid";
            fontImageSource.Glyph = "\ue560";
            ConnectionOp.IconImageSource = fontImageSource;
        }

        private void AboutItem_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (Navigation.NavigationStack.Where(x => x is AboutPage).Count() > 0)
                    return;
                Navigation.PushAsync(new AboutPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);
            }
        }
    }
}
