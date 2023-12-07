using Newtonsoft.Json;

namespace Eva.Models.Device
{
    public class ConexionWifiModel
    {
        [JsonProperty("network")]
        public NetData network { get; set; }
    }
    public class NetData
    {
        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("ssid")]
        public string ssid { get; set; }

        [JsonProperty("ip")]
        public string ip { get; set; }
    }
}
