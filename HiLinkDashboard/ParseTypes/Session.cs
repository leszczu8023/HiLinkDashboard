using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HiLinkDashboard.ParseTypes
{
    [XmlRoot("response")]
    public class Session
    {
        public string SesInfo, TokInfo;
    }
}
