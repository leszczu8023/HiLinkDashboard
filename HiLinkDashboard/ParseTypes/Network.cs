using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HiLinkDashboard.ParseTypes
{
    [XmlRoot("response")]
    public class Network
    {
        public string pci;
        public string sc;
        public string cell_id;
        public string rsrq;
        public string rsrp;
        public string rssi;
        public string sinr;
        public string rscp;
        public string ecio;
        public string mode;
    }
}
