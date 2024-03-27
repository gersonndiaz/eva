using Eva.Helpers.BT;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Eva.Views.Shared;

public partial class ConnectDeviceBTPage : ContentPage
{
    BluetoothService bluetoothService;
    ObservableCollection<IDevice> deviceList = new ObservableCollection<IDevice>();
    IAdapter adapter;
    IBluetoothLE ble;

    public ConnectDeviceBTPage()
	{
		InitializeComponent();

        bluetoothService = BluetoothService.Instance;
        adapter = bluetoothService.adapter;
        ble = bluetoothService.ble;

        bluetoothService.DeviceConnected += BluetoothService_DeviceConnected;
        bluetoothService.DeviceDiscovered += BluetoothService_DeviceDiscovered;
    }

    private async void BluetoothService_DeviceConnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
    {
        //Navigation.PopAsync(true);
    }

    private async void BluetoothService_DeviceDiscovered(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
    {
        //throw new NotImplementedException();
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

#if (ANDROID23_0_OR_GREATER)
            var statusBT = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
            if (statusBT != PermissionStatus.Granted)
            {
                statusBT = await Permissions.RequestAsync<Permissions.Bluetooth>();
            }
#endif

            if (ble.State == BluetoothState.Off)
            {
                // Pedir al usuario que active Bluetooth
                await DisplayAlert("Bluetooth desactivado", "Por favor, activa el Bluetooth", "OK");
                return;
            }

            if (ble.State == BluetoothState.Unavailable)
            {
                await DisplayAlert("Bluetooth no soportado", "Su dispositivo no soporta el uso de Bluetooth!", "OK");
                return;
            }

            await Navigation.PushModalAsync(new LoadingPage("Buscar WIFI", "Buscando dispositivos cercanos..."));
            var tempList = await bluetoothService.FindDevicesBluetooth(null);

            deviceList.Clear();
            deviceList = new ObservableCollection<IDevice>();

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

            deviceListView.ItemsSource = deviceList;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex}");
            Console.WriteLine($"{ex}");
            await DisplayAlert("Error", ex.Message, "OK");
        }

        try
        {
            if (Navigation.ModalStack.Where(x => x is LoadingPage).Count() > 0)
            {
                await Navigation.PopModalAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Debug.WriteLine(ex);
        }
    }

    private async void DeviceSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is IDevice selectedDevice)
        {
            try
            {
                bool isConnected = await bluetoothService.ConnectToDevice(selectedDevice);

                if (isConnected)
                {
                    await Navigation.PopAsync(true);
                }
                else
                {
                    await DisplayAlert("Error", $"No fue posible establecer conexión con el dispositivo {selectedDevice.Name}", "OK");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine(ex);
                await DisplayAlert("Error", $"Se produjo un error al intentar establecer conexión con el dispositivo {selectedDevice.Name}", "OK");
            }
        }
    }
}