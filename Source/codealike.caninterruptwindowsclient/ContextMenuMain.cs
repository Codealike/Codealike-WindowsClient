using Codealike.CanInterruptWindowsClient.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using Sleddog.Blink1;

namespace Codealike.CanInterruptWindowsClient
{
    public partial class ContextMenuMain : Form
    {

        public void Settings_Save(object sender, SaveEventArgs e)
        {
            UpdateStatus();
        }

        public ContextMenuMain()
        {
            InitializeComponent();

            var regValue = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Codealike", "Username", string.Empty);

            var frmSettings = new Settings();

            frmSettings.Save += new SaveEventHandler(Settings_Save);

            if (regValue == null)
            {
                frmSettings.Show();
            }
            else
            {
                Program.Username = regValue.ToString();
                UpdateStatus();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void UpdateStatus()
        {
            try
            {
                var currentCodealikeStatus = checkStatus(Program.Username);

                notifyIcon1.Icon = new Icon(GetType(), currentCodealikeStatus.Status + ".ico");

                if (Program.Username == string.Empty)
                {
                    notifyIcon1.Text = "You should provide an Username - " + DateTime.Now.ToString("hh:mm:ss");
                }
                else
                {
                    var value = currentCodealikeStatus.Message + " (" + Program.Username + " - " + DateTime.Now.ToString("hh:mm:ss") + ")";
                    value = value.Substring(0, value.Length < 63 ? value.Length : 63);
                    notifyIcon1.Text = value;
                    ChangeBlink1DeviceStatus(currentCodealikeStatus.Status);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private CanInterruptResponse checkStatus(string username)
        {
            var response = new CanInterruptResponse();

            response.Status = "no-connection";
            response.Title = "Connectivity issues";
            response.Message = "Can't connect.";

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Program.GetAPIRoot() + "public/CanInterruptUser");

                httpWebRequest.Method = "POST";

                httpWebRequest.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {

                    string json = "{" + "\"UserNames\": [\"" + username + "\",]}";

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var resultsJson = JsonConvert.DeserializeObject<dynamic>(result);

                        if (resultsJson != null && resultsJson.HasValues)
                        {
                            string val = resultsJson[0].m_Item2;

                            switch (val)
                            {
                                case "CanInterrupt":
                                    response.Status = "green";
                                    response.Title = "Reaching Nirvana";
                                    response.Message = "Trying to get focused.";
                                    break;
                                case "CannotInterrupt":
                                    response.Status = "red";
                                    response.Title = "Seize your focus";
                                    response.Message = "On a streak.";
                                    break;
                                case "NoActivity":
                                    response.Status = "grey";
                                    response.Title = "Inactive";
                                    response.Message = "Not coding.";
                                    break;
                                default:
                                    response.Status = "no-connection";
                                    response.Title = "Connectivity issues";
                                    response.Message = "Can't connect.";
                                    break;
                            }
                        }
                        else
                        {
                            response.Status = "grey";
                            response.Title = "Inactive";
                            response.Message = "Not coding.";
                        }
                    }
                }
                return response;

            }
            catch (Exception ex)
            {
                response.Message = ex.Message;

                return response;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frmSettings = new Settings();
            frmSettings.Save += new SaveEventHandler(Settings_Save);

            frmSettings.Show();
        }

        public void StopTimer()
        {
            this.timer1.Stop();
        }
        public void StartTimer()
        {
            this.timer1.Start();
        }

        private void refreshNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void ChangeBlink1DeviceStatus(string codealikeStatus)
        {
            Program.Blink1DeviceStatus blink1NewStatus;

            if (Program.MatchingRules.TryGetValue(codealikeStatus, out blink1NewStatus))
            {
                Program.ChangeBlink1DeviceStatus(blink1NewStatus);
            }
        }


        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                contextMenuStrip1.Show(Cursor.Position.X, Cursor.Position.Y);
            }
        }
    }
}
