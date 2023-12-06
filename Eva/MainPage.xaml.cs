using Eva.Views;
using System.Diagnostics;

namespace Eva
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
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
