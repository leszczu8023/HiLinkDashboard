using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace HiLinkDashboard
{
    public partial class Loader : Form
    {

        public IPAddress ipa;

        public Loader()
        {
            InitializeComponent();
            textBox1.Text = Program.raw_ip;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ipa = IPAddress.Parse(textBox1.Text);

                hostAvaliable(ipa);
                isHiLinkDevice(ipa);
                this.DialogResult = DialogResult.OK;
                this.Close();

            }
            catch (Exception w)
            {
                MessageBox.Show(this, "Connection error:\n" + w.Message + "\n\nCheck your device ip address and try again", "Connection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        public bool hostAvaliable(IPAddress a)
        {
            Ping p = new Ping();
            PingReply reply = p.Send(a);
            return (reply.Status == IPStatus.Success);
        }

        public void isHiLinkDevice(IPAddress a)
        {     
            if (!(new WebClientEx().DownloadString("http://" + a.ToString()).Contains("hilink")))
            {
                throw (new Exception("This is not a IP of HiLink device!"));
            }
        }
    }

}
