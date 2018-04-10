using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HiLinkDashboard.ParseTypes
{
    [XmlRoot("response")]
    public class BasicInfo
    {
        public string
            DeviceName,
            SerialNumber,
            Imei,
            Imsi,
            Iccid,
            Msisdn,
            HardwareVersion,
            SoftwareVersion,
            WebUIVersion,
            MacAddress1,
            MacAddress2,
            ProductFamily,
            Classify,
            supportmode,
            workmode;
    }
}
