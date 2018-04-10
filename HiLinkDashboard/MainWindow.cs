using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace HiLinkDashboard
{
    public partial class MainWindow : Form
    {

        public bool isWorking = true;
        public System.Net.IPAddress ip = new System.Net.IPAddress(new byte[] { 192, 168, 8, 1 });
        public int tick_time = int.Parse(Program.raw_idle);

        public Rebooting rebodlg;

        public ParseTypes.Session session;

        public MainWindow(IPAddress ip)
        {
            this.ip = ip;
            this.rebodlg = new Rebooting();
            rebodlg.StartPosition = FormStartPosition.CenterParent;
            session = getSession();
            InitializeComponent();
            Thread a = new Thread(signalThread);
            a.SetApartmentState(ApartmentState.MTA);
            a.Start();
            this.FormClosed += MainWindow_FormClosed;
            this.Text = this.Text + " (" + this.ip.ToString() + " connected)";
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            isWorking = false;
        }

        WebClientEx webclient;

        private void signalThread()
        {
            if (hostAvaliable(ip))
            {
                setSession();

                ParseTypes.Network net;
                ParseTypes.BasicInfo basicInfo;
                ParseTypes.Operator op;
                ParseTypes.Status sta;
                ParseTypes.Traffic tra;

                XmlSerializer serializer = new XmlSerializer(typeof(ParseTypes.Network));
                XmlSerializer serializer2 = new XmlSerializer(typeof(ParseTypes.BasicInfo));
                XmlSerializer serializer3 = new XmlSerializer(typeof(ParseTypes.Operator));
                XmlSerializer serializer4 = new XmlSerializer(typeof(ParseTypes.Status));
                XmlSerializer serializer5 = new XmlSerializer(typeof(ParseTypes.Traffic));

                while (isWorking)
                {
                    if (hostAvaliable(ip))
                    {
                        try
                        {
                            //MessageBox.Show(webclient.DownloadString("http://192.168.8.1/api/device/signal"));
                            using (StringReader reader = new StringReader(webclient.DownloadString("http://" + ip.ToString() + "/api/device/signal")))
                            {
                                net = (ParseTypes.Network)(serializer.Deserialize(reader));
                            }
                            Thread.Sleep(200);
                            using (StringReader reader = new StringReader(webclient.DownloadString("http://" + ip.ToString() + "/api/device/information")))
                            {
                                basicInfo = (ParseTypes.BasicInfo)(serializer2.Deserialize(reader));
                            }
                            Thread.Sleep(200);
                            using (StringReader reader = new StringReader(webclient.DownloadString("http://" + ip.ToString() + "/api/monitoring/status")))
                            {
                                sta = (ParseTypes.Status)(serializer4.Deserialize(reader));
                            }
                            Thread.Sleep(200);
                            using (StringReader reader = new StringReader(webclient.DownloadString("http://" + ip.ToString() + "/api/monitoring/traffic-statistics")))
                            {
                                tra = (ParseTypes.Traffic)(serializer5.Deserialize(reader));
                            }
                            Thread.Sleep(200);
                            try

                            {
                                using (StringReader reader = new StringReader(webclient.DownloadString("http://" + ip.ToString() + "/api/net/current-plmn")))
                                {
                                    op = (ParseTypes.Operator)(serializer3.Deserialize(reader));
                                }
                            }
                            catch
                            {
                                op = new ParseTypes.Operator();
                                op.FullName = "Unknown";
                            }

                            parseSignal(net, basicInfo, op, sta, tra);

                            if (isRestarting)
                            {
                                int i = 40;

                                sh_sh();

                                while (i != 0)
                                {
                           
                                    chLabel(toolStripStatusLabel2, "wait " + i + " seconds to reconnect.");
                                    set_time(i);
                                    Thread.Sleep(1000);
                                    i--;
                                }

                                setSession();

                                hd_sh();
                                isRestarting = false;
                            } else {
                                Thread.Sleep(tick_time);
                            }
                        }
                        catch (TimeoutException)
                        {
                            this.Invoke((MethodInvoker)(() => lost_close()));
                        }
                        catch (WebException)
                        {
                             this.Invoke((MethodInvoker)(() => lost_close()));
                        }
                        catch (InvalidOperationException)
                        {
                            this.Invoke((MethodInvoker)(() => lost_close()));
                        }
                        
                    }
                    else
                    {
                        this.Invoke((MethodInvoker)(() => lost_close()));
                    }
                    
                } 
            }
        }

        public void sh_sh()
        {
            this.Invoke((MethodInvoker)(() => rebodlg.Show(this)));
            this.Invoke((MethodInvoker)(() => this.Enabled = false));
        }
        public void set_time(int i)
        {
            this.Invoke((MethodInvoker)(() => rebodlg.setTime(i)));
        }

        public void hd_sh()
        {
            this.Invoke((MethodInvoker)(() => rebodlg.Close()));
            this.Invoke((MethodInvoker)(() => this.Enabled = true));
            this.Invoke((MethodInvoker)(() => this.Focus()));
        }

        void setSession()
        {
            string[] a = (session.SesInfo).Split('=');
            this.session = getSession();
            webclient = new WebClientEx();
            a = (session.SesInfo).Split('=');

            webclient.Headers.Add(HttpRequestHeader.Cookie, session.SesInfo);
            webclient.CookieContainer.Add(new Uri("http://" + ip.ToString()), new Cookie(a[0], a[1]));

        }

        void lost_close()
        {
            MessageBox.Show(this, "Connection lost.", "Disconnected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Application.Exit();
        }

        void parseSignalInv(ParseTypes.Network str, ParseTypes.BasicInfo ne, ParseTypes.Operator op, ParseTypes.Status sta, ParseTypes.Traffic tra)
        {
            try
            {
                if (this.InvokeRequired)
                    this.Invoke(new Action<ParseTypes.Network, ParseTypes.BasicInfo, ParseTypes.Operator, ParseTypes.Status, ParseTypes.Traffic>(parseSignal), str, ne, op, sta, tra);
                else
                    parseSignal(str, ne, op, sta, tra);
            }
            catch (Exception) { }
        }

        void chLabel(Control l, string text)
        {
            if (l.Text != text)
            {
                try
                {
                    if (l.InvokeRequired)
                        l.Invoke((MethodInvoker)(() => l.Text = text
                            ));
                    else
                        l.Text = text;
                }
                catch { }
            }
        }

        void chLabel(ToolStripStatusLabel l, string text)
        {
            try
            {
                if (InvokeRequired)
                    Invoke((MethodInvoker)(() => l.Text = text));
                else
                    l.Text = text;
            }
            catch (Exception)
            {

            }

        }

        int chn = 0;

        private void parseSignal(ParseTypes.Network si, ParseTypes.BasicInfo basicInfo, ParseTypes.Operator op, ParseTypes.Status sta, ParseTypes.Traffic tra)
        {
            EnumNetworkType type = (EnumNetworkType)System.Enum.Parse(typeof(EnumNetworkType), basicInfo.workmode.Replace(" ", "_"));

            chLabel(label22, getTypeNet(int.Parse(sta.CurrentNetworkTypeEx)));

            int a = 0;

            int.TryParse(sta.SignalIcon, out a);
            signalStrength1.Strength = a;

            chLabel(label25, op.FullName);

            chLabel(label33, op.FullName);
            chLabel(label36, op.ShortName);

            chLabel(label39, calcM(tra.CurrentUploadRate) + "/sec");
            chLabel(label40, calcM(tra.CurrentDownloadRate) + "/sec");

            chLabel(toolStripStatusLabel2, getConnStatus(int.Parse(sta.ConnectionStatus)));

            chLabel(label3, si.cell_id.ToString());
            chLabel(label5, si.rsrq);
            chLabel(label6, si.rsrp);
            chLabel(label9, si.rssi);
            chLabel(label11, si.sinr);
            chLabel(label24, si.rscp);

            try
            {
                TimeSpan t = TimeSpan.FromSeconds(int.Parse(tra.CurrentConnectTime));

                string answer = string.Format("{0:D2}h {1:D2}m {2:D2}s",
                                t.Hours,
                                t.Minutes,
                                t.Seconds,
                                t.Milliseconds);

                chLabel(label45, answer);
            } catch { }

            chLabel(label41, calcM(tra.CurrentDownload));
            chLabel(label43, calcM(tra.CurrentUpload));

            chLabel(label18, basicInfo.DeviceName);
            chLabel(label19, basicInfo.Imei);
            chLabel(label20, basicInfo.SerialNumber);
            chLabel(label14, basicInfo.HardwareVersion);
            chLabel(label12, basicInfo.SoftwareVersion);

            chLabel(label28, sta.WanIPAddress);
            chLabel(label29, basicInfo.MacAddress1);
        }

        public bool hostAvaliable(IPAddress a)
        {
            try
            {
                Ping p = new Ping();
                PingReply reply = p.Send(a);
                return (reply.Status == IPStatus.Success);
            } catch
            {
                return false;
            }
            
        }

        public string calcM(string a)
        {
            try
            {
                double _a = double.Parse(a);
                string _b = "n/a";
                if (_a < 1024)
                {
                    _b = _a + "B";
                }
                else if (_a > 1024 && _a < (1024 * 1024))
                {
                    _b = (_a / 1024).ToString("0.00") + "KB";
                }
                else if (_a > (1024 * 1024) && _a < (1024 * 1024 * 1024))
                {
                    _b = (_a / 1024 / 1024).ToString("0.00") + "MB";
                }
                else if (_a > (1024 * 1024* 1024))
                {
                    _b = (_a / 1024 / 1024 / 1024).ToString("0.00") + "GB";
                }
                return _b;
            }
            catch
            {
                return "n/a";
            }
        }

        public string getTypeNet(int n)
        {
            string type = "Unknown";
            switch (n)
            {
                case 0: type = "No service"; break;
                case 1: type = "GSM"; break;
                case 2: type = "GPRS"; break;
                case 3: type = "EDGE"; break;
                case 4: type = "WCDMA"; break;
                case 5: type = "HSDPA"; break;
                case 6: type = "HSUPA"; break;
                case 7: type = "HSPA"; break;
                case 8: type = "TDSCDMA"; break;
                case 9: type = "HSPA+"; break;
                case 10: type = "EVDO rev. 0"; break;
                case 11: type = "EVDO rev. A"; break;
                case 12: type = "EVDO rev. B"; break;
                case 13: type = "1xRTT"; break;
                case 14: type = "UMB"; break;
                case 15: type = "1xEVDV"; break;
                case 16: type = "3xRTT"; break;
                case 17: type = "HSPA+64QAM"; break;
                case 18: type = "HSPA+MIMO"; break;
                case 19: type = "LTE"; break;
                case 21: type = "IS95A"; break;
                case 22: type = "IS95B"; break;
                case 23: type = "CDMA1x"; break;
                case 24: type = "EVDO rev. 0"; break;
                case 25: type = "EVDO rev. A"; break;
                case 26: type = "EVDO rev. B"; break;
                case 27: type = "Hybrid CDMA1x"; break;
                case 28: type = "Hybrid EVDO rev. 0"; break;
                case 29: type = "Hybrid EVDO rev. A"; break;
                case 30: type = "Hybrid EVDO rev. B"; break;
                case 31: type = "EHRPD rev. 0"; break;
                case 32: type = "EHRPD rev. A"; break;
                case 33: type = "EHRPD rev. B"; break;
                case 34: type = "Hybrid EHRPD rev. 0"; break;
                case 35: type = "Hybrid EHRPD rev. A"; break;
                case 36: type = "Hybrid EHRPD rev. B"; break;
                case 41: type = "WCDMA"; break;
                case 42: type = "HSDPA"; break;
                case 43: type = "HSUPA"; break;
                case 44: type = "HSPA"; break;
                case 45: type = "HSPA+"; break;
                case 46: type = "DC HSPA+"; break;
                case 61: type = "TD SCDMA"; break;
                case 62: type = "TD HSDPA"; break;
                case 63: type = "TD HSUPA"; break;
                case 64: type = "TD HSPA"; break;
                case 65: type = "TD HSPA+"; break;
                case 81: type = "802.16E"; break;
                case 101: type = "LTE"; break;
            }
            return type;
        }

        public string getConnStatus(int status)
        {
            string st = "Unknown";
            switch (status)
            {
                case 2: case 3: case 5: case 8: case 20: case 21: case 23: case 27: case 28: case 29: case 30: case 31: case 32: case 33: case 65538: case 65539: case 65567: case 65568: case 131073: case 131074: case 131076: case 131078: st = "No connection (invalid profile)."; break;
                case 7: case 11: case 14: case 37: case 131079: case 131080: case 131081: case 131082: case 131083: case 131084: case 131085: case 131086: case 131087: case 131088: case 131089: st = "No connection (forbitten)"; break;
                case 12: case 13: st = "No connection (no roaming)"; break;
                case 112: st = "Automatic connection disabled"; break;
                case 113: st = "Automatic connection in roaming disabled"; break;
                case 114: st = "Reconnection unavaliable"; break;
                case 115: st = "Reconnection unavaliable in roaming"; break;
                case 201: st = "Connection lost: data transport timeout"; break;
                case 900: st = "Connecting..."; break;
                case 901: st = "Connected"; break;
                case 902: st = "Disconnected"; break;
                case 903: st = "Disconnecting..."; break;
                case 904: st = "Connection unsuccessfull"; break;
                case 905: st = "No connection (low signal)"; break;
                case 906: st = "No connection"; break;
            }
            return st;
        }

        public string getErr(int err)
        {
            string st = "Unknown error";
            switch(err)
            {
                case 100002: st = "Unknown API function"; break;
                case 100003: st = "No permissions"; break;
                case 100004: st = "System busy"; break;
                case 100005: st = "Unknown error"; break;
                case 100006: st = "Invalid parameter"; break;
                case 100009: st = "IO error"; break;
                case 103002: st = "Unknown error"; break;
                case 103015: st = "Unknown error"; break;
                case 108001: st = "Invalid username"; break;
                case 108002: st = "Invalid password"; break;
                case 108003: st = "User logged in"; break;
                case 108006: st = "Unknown username or password"; break;
                case 108007: st = "Unknown username or password"; break;
                case 110024: st = "Battery <50%"; break;
                case 111019: st = "No response"; break;
                case 111020: st = "Timeout"; break;
                case 111022: st = "Network unsupported"; break;
                case 113018: st = "System busy"; break;
                case 114001: st = "File exists"; break;
                case 114002: st = "File exists"; break;
                case 114003: st = "SD Card in use"; break;
                case 114004: st = "Shared path doesnt exists"; break;
                case 114005: st = "Path too long"; break;
                case 114006: st = "Permissions denied"; break;
                case 115001: st = "Unknown error"; break;
                case 117001: st = "Invalid password"; break;
                case 117004: st = "Invalid WISPr password"; break;
                case 120001: st = "Voice call busy"; break;
                case 125001: st = "Invalid token"; break;
            }
            return "(" + err + ") " + st ;
        }

        public string post(string url, string data, string[][] headers)
        {
            string resp;

            using (var client = new WebClientEx())
            {
                string[] a = (session.SesInfo).Split('=');

                client.Headers.Add(HttpRequestHeader.Cookie, session.SesInfo);
                client.CookieContainer.Add(new Uri("http://" + ip.ToString()), new Cookie(a[0], a[1]));

                foreach (string[] s in headers)

                {
                    client.Headers.Add(s[0], s[1]) ;
                }

                resp = client.UploadString(url, data);
            }

            return resp;
        }

        private ParseTypes.Session getSession()
        {
            //
            ParseTypes.Session ses;
            using (StringReader reader = new StringReader(new WebClientEx().DownloadString("http://" + ip.ToString() + "/api/webserver/SesTokInfo")))
            {
                ses = ((ParseTypes.Session)(new XmlSerializer(typeof(ParseTypes.Session)).Deserialize(reader)));
            }
            return ses;
        }

        private bool isRestarting = false;

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to restart your modem?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {

                string[][] head = new string[][] {
                    new string[] { "Content-Type","text/xml" },
                    new string[] { "__RequestVerificationToken", session.TokInfo }
                };

                var po = post("http://" + ip.ToString() + "/api/device/control", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><request><Control>1</Control></request>", head);

                if (po.Contains("OK"))
                {
                    toolStripStatusLabel2.Text = "Rebooting...";
                    isRestarting = true;
                }
                else
                {
                    using (StringReader reader = new StringReader(po))
                    {
                       MessageBox.Show(getErr(int.Parse(((ParseTypes.Error)(new XmlSerializer(typeof(ParseTypes.Error)).Deserialize(reader))).code)), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!File.Exists("putty.exe"))
            {
                WebClient wc = new WebClient();
                wc.DownloadFile("https://the.earth.li/~sgtatham/putty/latest/x86/putty.exe", "putty.exe");
            }
            System.Diagnostics.Process.Start("putty.exe", "-telnet " + ip.ToString());
        }

        Process aa, aa2;

        public bool isPinging = false;
        public bool isPingCancelled = false;
        private void button3_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox2.Text) || !String.IsNullOrWhiteSpace(textBox2.Text))
            {
                if (!isPinging)
                {
                    isPingCancelled = false;
                    if (aa != null) aa.Dispose();
                    textBox1.Clear();

                    isPinging = true;
                    button3.Text = "Cancel";
                    textBox2.Enabled = false;
                    aa = new Process();
                    aa.StartInfo.CreateNoWindow = true;
                    aa.StartInfo.RedirectStandardError = true;
                    aa.StartInfo.RedirectStandardOutput = true;
                    aa.StartInfo.UseShellExecute = false;
                    aa.StartInfo.FileName = "ping.exe";
                    aa.StartInfo.Arguments = textBox2.Text + ((checkBox1.Checked) ? " /t" : "");
                    aa.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
                    aa.ErrorDataReceived += new DataReceivedEventHandler(SortOutputHandler);
                    aa.StartInfo.RedirectStandardInput = true;

                    aa.Start();

                    aa.EnableRaisingEvents = true;
                    aa.Exited += new EventHandler(A_Exited);

                    aa.BeginErrorReadLine();
                    aa.BeginOutputReadLine();
                }
                else
                {
                    aa.Kill();
                    isPingCancelled = true;
                }
            }
            else
            {
                textBox1.Text = "Invalid ip or hostname.";
            }
            
        }

        private void A_Exited(object sender, EventArgs e)
        {
            isPinging = false;
            chLabel(button3, "Send");
            Invoke((MethodInvoker)(() => textBox2.Enabled = true));
            if (isPingCancelled) Invoke((MethodInvoker)(() => textBox1.AppendText("Cancelled!" + Environment.NewLine)));
        }
             
        private void SortOutputHandler(object sendingProcess,
           DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Invoke((MethodInvoker)(() => textBox1.AppendText(outLine.Data + Environment.NewLine)));
            }
        }


        public bool isTracerouting = false;
        public bool isTracertCancelled = false;
        private void button4_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox3.Text) || !String.IsNullOrWhiteSpace(textBox3.Text))
            {
                if (!isTracerouting)
                {
                    isPingCancelled = false;
                    if (aa2 != null) aa2.Dispose();
                    textBox4.Clear();

                    isTracerouting = true;
                    button4.Text = "Cancel";
                    textBox3.Enabled = false;
                    aa2 = new Process();
                    aa2.StartInfo.CreateNoWindow = true;
                    aa2.StartInfo.RedirectStandardError = true;
                    aa2.StartInfo.RedirectStandardOutput = true;
                    aa2.StartInfo.UseShellExecute = false;
                    aa2.StartInfo.FileName = "tracert.exe";
                    aa2.StartInfo.Arguments = textBox3.Text;
                    aa2.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler2);
                    aa2.ErrorDataReceived += new DataReceivedEventHandler(SortOutputHandler2);
                    aa2.StartInfo.RedirectStandardInput = true;

                    aa2.Start();

                    aa2.EnableRaisingEvents = true;
                    aa2.Exited += new EventHandler(A2_Exited);

                    aa2.BeginErrorReadLine();
                    aa2.BeginOutputReadLine();
                }
                else
                {
                    aa2.Kill();
                    isTracertCancelled = true;
                }
            }
            else
            {
                textBox4.Text = "Invalid ip or hostname.";
            }
        }

        private void A2_Exited(object sender, EventArgs e)
        {
            isTracerouting = false;
            chLabel(button4, "Send");
            Invoke((MethodInvoker)(() => textBox3.Enabled = true));
            if (isTracertCancelled) Invoke((MethodInvoker)(() => textBox4.AppendText("Cancelled!" + Environment.NewLine)));
        }


        private void SortOutputHandler2(object sendingProcess,
           DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Invoke((MethodInvoker)(() => textBox4.AppendText(outLine.Data + Environment.NewLine)));
            }
        }

    }
}
