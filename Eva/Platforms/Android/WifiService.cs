using Android.Content;
using Android.Net.Wifi;
using Eva.Shared.Network;

namespace Eva.Platforms.Android
{
    public class WifiService : IWifiService
    {
        public async Task<List<Eva.Models.Network>> GetAvailableNetworksAsync()
        {
            var context = Platform.CurrentActivity.ApplicationContext;
            var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);

            var tcs = new TaskCompletionSource<List<Eva.Models.Network>>();
            var wifiScanReceiver = new WifiScanReceiver(wifiManager, tcs);

            context.RegisterReceiver(wifiScanReceiver, new IntentFilter(WifiManager.ScanResultsAvailableAction));
            wifiManager.StartScan();

            var wifiNetworks = await tcs.Task; // Espera a que el escaneo se complete

            context.UnregisterReceiver(wifiScanReceiver); // Importante para evitar fugas de memoria

            return wifiNetworks;
        }

    }
}
