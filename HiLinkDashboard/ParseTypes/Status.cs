using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HiLinkDashboard.ParseTypes
{
    [XmlRoot("response")]
    public class Status
    {
        public string ConnectionStatus;
        public string WifiConnectionStatus;
        public string SignalStrength;
        public string SignalIcon;
        public string CurrentNetworkType;
        public string CurrentServiceDomain;
        public string RoamingStatus;
        public string BatteryStatus;
        public string BatteryLevel;
        public string BatteryPercent;
        public string simlockStatus;
        public string WanIPAddress;
        public string WanIPv6Address;
        public string PrimaryDns;
        public string SecondaryDns;
        public string PrimaryIPv6Dns;
        public string SecondaryIPv6Dns;
        public string CurrentWifiUser;
        public string TotalWifiUser;
        public string currenttotalwifiuser;
        public string ServiceStatus;
        public string SimStatus;
        public string WifiStatus;
        public string CurrentNetworkTypeEx;
        public string maxsignal;
        public string wifiindooronly;
        public string wififrequence;
        public string classify;
        public string flymode;
        public string cellroam;

    }
}
