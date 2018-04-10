using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HiLinkDashboard.ParseTypes
{
    [XmlRoot("response")]
    public class Traffic
    {
        public string CurrentConnectTime;
        public string CurrentUpload;
        public string CurrentDownload;
        public string CurrentDownloadRate;
        public string CurrentUploadRate;
        public string TotalUpload;
        public string TotalDownload;
        public string TotalConnectTime;
        public string showtraffic;
    }
}
