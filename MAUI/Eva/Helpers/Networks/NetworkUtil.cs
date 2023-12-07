using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eva.Helpers.Networks
{
    public class NetworkUtil
    {
        public static string GetQualitySignal(double dbm)
        {
            string quality = "";

            if (dbm > 0 || dbm < -100)
            {
                quality = $"Error";
            }
            else if (dbm >= -30)
            {
                quality = $"Máxima";
            }
            else if (dbm >= -50)
            {
                quality = $"Excelente";
            }
            else if (dbm >= -60)
            {
                quality = $"Buena";
            }
            else if (dbm >= -70)
            {
                quality = $"Media";
            }
            else if (dbm >= -80)
            {
                quality = $"Baja";
            }
            else if (dbm >= -100)
            {
                quality = $"Débil";
            }

            return quality;
        }
    }
}
