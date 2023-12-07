using AutoMapper.Configuration.Annotations;
using Eva.Helpers.Networks;
using System.Diagnostics;

namespace Eva.Models.App
{
    public class RedWifiModel
    {
        public List<Network> networks { get; set; }
    }

    public class Network
    {
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public string Band { get; set; }
        public int ChannelCenterFrequencyInKilohertz { get; set; }
        public string NetworkKind { get; set; }
        public double NetworkRssiInDecibelMilliwatts { get; set; }
        public string SecuritySettings { get; set; }

        [Ignore]
        public string GetGlosa
        {
            get
            {
                try
                {
                    string calidadSenial = NetworkUtil.GetQualitySignal(NetworkRssiInDecibelMilliwatts);
                    return $"{SSID} | {Band} | Señal: {calidadSenial}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return "";
                }
            }
        }
    }
}
