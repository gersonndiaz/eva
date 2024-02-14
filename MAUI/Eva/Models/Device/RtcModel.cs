using Newtonsoft.Json;

namespace Eva.Models.Device
{
    public class RtcModel
    {
        [JsonProperty("rtc")]
        public RTC rtc { get; set; }
    }

    public class RTC
    {
        [JsonProperty("status")]
        public string status { get; set; }
        [JsonProperty("datetime")]
        public string datetime { get; set; }
    }
}
