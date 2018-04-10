using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HiLinkDashboard
{
    public partial class SignalStrength : UserControl
    {
        public SignalStrength()
        {
            InitializeComponent();
            this.Strength = 0;
        }

        private int strength = 0;

        public int Strength
        {
            get
            {
                return strength;
            }
            set
            {
                if (value != this.Strength)
                {
                    switch (value)
                    {
                        case 0:
                            this.BackgroundImage = Properties.Resources.icon_signal_00;
                            strength = value;
                            break;
                        case 1:
                            this.BackgroundImage = Properties.Resources.icon_signal_01;
                            strength = value;
                            break;
                        case 2:
                            this.BackgroundImage = Properties.Resources.icon_signal_02;
                            strength = value;
                            break;
                        case 3:
                            this.BackgroundImage = Properties.Resources.icon_signal_03;
                            strength = value;
                            break;
                        case 4:
                            this.BackgroundImage = Properties.Resources.icon_signal_04;
                            strength = value;
                            break;
                        case 5:
                            this.BackgroundImage = Properties.Resources.icon_signal_05;
                            strength = value;
                            break;
                    }
                }
            }
        }
    }
}
