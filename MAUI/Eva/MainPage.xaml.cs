using Eva.Helpers.BT;
using Eva.Views;
using Eva.Views.Shared;
using System.Diagnostics;

namespace Eva
{
    public partial class MainPage : ContentPage
    {
        BluetoothService bluetoothService;

        public MainPage()
        {
            InitializeComponent();

            bluetoothService = BluetoothService.Instance;
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
