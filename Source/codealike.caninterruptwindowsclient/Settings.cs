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

namespace Codealike.CanInterruptWindowsClient
{
    public partial class Settings : Form
    {
        public event SaveEventHandler Save;

        public Settings()
        {
            InitializeComponent();

            txtUsername.Text = Program.Username;

            if (Program.MatchingRulesKey == "hard")
            {
                radHard.Checked = true;
                radSoft.Checked = false;
            }
            else
            {
                radSoft.Checked = true;
                radHard.Checked = false;
            }
        }

        private void lnkCancel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Program.Username = txtUsername.Text;

            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Codealike", "Username", Program.Username);

            var matchingRules = "hard";

            if (radSoft.Checked)
            {
                matchingRules = "soft";
            }

            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Codealike", "MatchingRules", matchingRules);
            Program.MatchingRulesKey = matchingRules;
            Program.MatchingRules = Program.GetMatchingRules();

            SaveEventHandler handler = Save;

            if (handler != null)
            {
                handler(this, new SaveEventArgs() { NewUsername = Program.Username });
            }
            
            this.Close();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            this.AcceptButton = btnSave;
        }
    }
    public delegate void SaveEventHandler(object sender, SaveEventArgs e);
    public class SaveEventArgs
    {
        public string NewUsername { get; set; }
    }

}
