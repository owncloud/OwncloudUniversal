using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Power;

namespace OwncloudUniversal.Shared.Synchronisation
{
    class ExecutionContext
    {
        public bool AllowRun { get; set; }

        private bool CheckBatteryStatus()
        {
            var report = Windows.Devices.Power.Battery.AggregateBattery.GetReport();
            switch (report.Status)
            {
                case BatteryStatus.Charging:
                    return true;
                case BatteryStatus.NotPresent:
                    return true;
                case BatteryStatus.Idle:
                    return true;
            }
            return false;
        }

        private bool HasWifi()
        {
            var profile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            if (profile != null && profile.IsWlanConnectionProfile)
            {
                return true;
            }
            return false;
        }
    }
}
