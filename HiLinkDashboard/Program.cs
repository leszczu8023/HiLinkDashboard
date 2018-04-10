using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace HiLinkDashboard
{
    static class Program
    {
        public static string raw_ip;
        public static string raw_idle;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var MyIni = new IniFile(@"settings.ini");

            raw_ip = MyIni.Read("DeviceIp");
            raw_idle = MyIni.Read("IdleTime");


            if (String.IsNullOrEmpty(raw_ip) || String.IsNullOrWhiteSpace(raw_ip))
            {
                MyIni.Write("DeviceIp", "192.168.8.1");
                raw_ip = "192.168.8.1";
            }
            if (String.IsNullOrEmpty(raw_idle) || String.IsNullOrWhiteSpace(raw_idle))
            {
                MyIni.Write("IdleTime", "1000");
                raw_idle = "1000";
            }

            /*
             * var DefaultVolume = IniFile.Read("DefaultVolume");
               var HomePage = IniFile.Read("HomePage");
             */

            Loader a = new Loader();

            if (a.ShowDialog() == DialogResult.OK)
            {
                MyIni.Write("DeviceIp", a.ipa.ToString());
                Application.Run(new MainWindow(a.ipa));
            }
        }
    }
}
