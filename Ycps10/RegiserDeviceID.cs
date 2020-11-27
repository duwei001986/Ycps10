using JSCardKiosks;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ycps10
{
    public partial class RegiserDeviceID : Form
    {
        string temp = "";
        public RegiserDeviceID()
        {
            InitializeComponent();
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        private void RegiserDeviceID_Load(object sender, EventArgs e)
        {
            textBox1.Text = DealAppConfig.GetAppSettingsValue("设备编号头");
            textBox1.Select(textBox1.Text.Length, 1);
        }

        private void buttonN1_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            temp = button.Name.Substring(button.Name.Length - 1);
            textBox1.Text += temp;
            textBox1.SelectionStart = textBox1.Text.Length;
        }

        private void buttonNX_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
            {
                try
                {
                    textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.Length - 1);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            RegistryKey regkeySetKey = Registry.CurrentUser.OpenSubKey(@"Software", true).CreateSubKey(@"科颐\sbkPrinter\DeviceInfo");
            regkeySetKey.SetValue("设备型号", DealAppConfig.GetAppSettingsValue("设备型号"));
            regkeySetKey.SetValue("设备编号", textBox1.Text);
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\科颐\sbkPrinter\DeviceInfo");
            labDeviceID.Text = regkey.GetValue("设备编号").ToString();
        }
    }
}
