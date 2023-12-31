﻿using Android.Content;
using Android.Net.Wifi;
using Android.Util;
using Eva.Models.App;
using System.Diagnostics;

namespace Eva.Platforms.Android
{
    public class WifiScanReceiver : BroadcastReceiver
    {
        private TaskCompletionSource<List<Network>> _taskCompletionSource;
        private WifiManager _wifiManager;

        public WifiScanReceiver(WifiManager wifiManager, TaskCompletionSource<List<Network>> taskCompletionSource)
        {
            _wifiManager = wifiManager;
            _taskCompletionSource = taskCompletionSource;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var networks = new List<Network>();
            
            try
            {
                var results = _wifiManager.ScanResults;
                foreach (var result in results)
                {
                    try
                    {
                        if (networks != null
                        && !networks.Any(x => x.SSID == result.Ssid))
                        {

                            var netData = new Eva.Models.App.Network
                            {
                                SSID = result.Ssid,
                                BSSID = result.Bssid,
                                Band = GetBandFromFrequency(result.Frequency),
                                ChannelCenterFrequencyInKilohertz = result.ChannelWidth,
                                NetworkRssiInDecibelMilliwatts = result.Level,
                                // Completar con otros campos si es necesario
                            };
                            networks.Add(netData);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Debug.WriteLine(e);
                        Log.Error("WifiScanReceiver", $"{e}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.WriteLine(e);
                Log.Error("WifiScanReceiver", $"{e}");
            }

            _taskCompletionSource.SetResult(networks);
        }

        private string GetBandFromFrequency(int frequency)
        {
            if (frequency >= 2400 && frequency <= 2483)
            {
                return "2.4GHz";
            }
            else if (frequency >= 5170 && frequency <= 5825)
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
