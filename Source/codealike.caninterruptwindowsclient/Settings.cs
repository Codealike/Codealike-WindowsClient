using Codealike.CanInterruptWindowsClient.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Codealike.CanInterruptWindowsClient
{
    public partial class Settings : Form
    {
        public event SaveEventHandler Save;

        public Settings()
        {
            InitializeComponent();

            txtUsername.Text = string.Empty;
            radSoft.Select();
            btnSave.Enabled = false;
        }

        private void lnkCancel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var deviceSerialNumber = lstDevices.SelectedItems[0].SubItems[0].Text;
            var username = txtUsername.Text;
            var matchingRules = "hard";

            if (radSoft.Checked)
            {
                matchingRules = "soft";
            }

            var storedDevice = new StoredDevice() { Username = username, MatchingRules = matchingRules };

            if (Program.Rules.Where(x => x.Key == deviceSerialNumber).Any())
            {
                Program.Rules[deviceSerialNumber] = storedDevice;
            }
            else
            {
                Program.Rules.Add(deviceSerialNumber, storedDevice);
            }

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, storedDevice);

                var data = ms.ToArray();
                
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Codealike", String.Format("{0}", deviceSerialNumber), data, RegistryValueKind.Binary);
            }

            Save?.Invoke(this, new SaveEventArgs() { NewDevice = deviceSerialNumber });

            lblMessage.Text = "New device settings saved";
            lblMessage.ForeColor = Color.LightSeaGreen;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            this.AcceptButton = btnSave;

            lstDevices.Columns.Add("Device");
            lstDevices.Columns.Add("Version");
            lstDevices.Columns.Add("User");
            lstDevices.Columns.Add("Last Update");

            var devices = Program.GetBlinkDevices();

            foreach (var device in devices)
            {
                lstDevices.Items.Add(new ListViewItem(new string[] { device.SerialNumber, device.Version.ToString(), string.Empty, string.Empty }));
            }            
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnIdentify_Click(object sender, EventArgs e)
        {
            Program.IdentifyDevice(lstDevices.SelectedItems[0].SubItems[0].Text);
        }

        private void lstDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstDevices.SelectedItems.Count == 0)
                return; 

            var deviceSerialNumber = lstDevices.SelectedItems[0].SubItems[0].Text;
            StoredDevice storedDevice;

            if (Program.Rules.TryGetValue(deviceSerialNumber, out storedDevice))
            {
                txtUsername.Text = storedDevice.Username;

                if (storedDevice.MatchingRules == "hard")
                    radHard.Checked = true;
            }
            else
            {
                txtUsername.Text = string.Empty;
                radSoft.Checked = true;
            }

            btnIdentify.Enabled = (lstDevices.SelectedItems.Count > 0);
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            btnSave.Enabled = (this.Text != string.Empty);
        }
    }

    public delegate void SaveEventHandler(object sender, SaveEventArgs e);

    public class SaveEventArgs
    {
        public string NewDevice { get; set; }
    }

}
