using Eva.Models.App;
using Eva.Shared.Network;
using Windows.Devices.WiFi;

namespace Eva.Platforms.Windows
{
    public class WifiService : IWifiService
    {
        public async Task<List<Network>> GetAvailableNetworksAsync()
        {
            var wifiNetworks = new List<Network>();
            var access = await WiFiAdapter.RequestAccessAsync();

            if (access != WiFiAccessStatus.Allowed)
            {
                throw new InvalidOperationException("WiFi access denied");
            }

            var adapters = await WiFiAdapter.FindAllAdaptersAsync();
            if (adapters.Count == 0)
            {
                return wifiNetworks;
            }

            var adapter = adapters[0];
            await adapter.ScanAsync();

            foreach (var network in adapter.NetworkReport.AvailableNetworks)
            {
                if (wifiNetworks != null 
                    && !wifiNetworks.Any(x => x.SSID == network.Ssid && x.Band == GetBandFromFrequency(network.ChannelCenterFrequencyInKilohertz)))
                {
                    Network netData = new Network();
                    netData.SSID = network.Ssid;
                    netData.BSSID = network.Bssid;
                    netData.Band = GetBandFromFrequency(network.ChannelCenterFrequencyInKilohertz);
                    netData.ChannelCenterFrequencyInKilohertz = network.ChannelCenterFrequencyInKilohertz;
                    //netData.NetworkKind = network.NetworkKind;
                    netData.NetworkRssiInDecibelMilliwatts = network.NetworkRssiInDecibelMilliwatts;
                    //netData.SecuritySettings = network.SecuritySettings;

                    wifiNetworks.Add(netData);
                }
            }

            return wifiNetworks;
        }

        private string GetBandFromFrequency(int frequencyKHz)
        {
            if (frequencyKHz >= 2400000 && frequencyKHz <= 2483500)
            {
                return "2.4GHz";
            }
            else if (frequencyKHz >= 5170000 && frequencyKHz <= 5825000)
            {
                return "5GHz";
            }
            else
            {
                return "Desconocido";
            }
        }

    }
}
