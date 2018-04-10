using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HiLinkDashboard
{
    public partial class Rebooting : Form
    {
        public Rebooting()
        {
            InitializeComponent();
        }

        public void setTime(int a)
        {
            label2.Text = a.ToString() + " second(s) left";
        }
    }
}
