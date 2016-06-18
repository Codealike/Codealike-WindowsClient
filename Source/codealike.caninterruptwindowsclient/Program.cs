using Codealike.CanInterruptWindowsClient.Models;
using Microsoft.Win32;
using Sleddog.Blink1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Codealike.CanInterruptWindowsClient
{
    static class Program
    {
        public static Dictionary<string, StoredDevice> Rules;
        public static ContextMenuMain mainForm;

        // These dictionaries include the rules for matching status between Codealike (KEY) and Blink1 (VALUE).
        public static IDictionary<string, Blink1DeviceStatus> MatchingRules_HARD;
        public static IDictionary<string, Blink1DeviceStatus> MatchingRules_SOFT;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            MatchingRules_HARD = new Dictionary<string, Program.Blink1DeviceStatus>();
            MatchingRules_SOFT = new Dictionary<string, Program.Blink1DeviceStatus>();

            // Set Matching rules.
            MatchingRules_HARD.Add("no-connection", Blink1DeviceStatus.NoConnection);
            MatchingRules_HARD.Add("grey", Blink1DeviceStatus.NoActivity);
            MatchingRules_HARD.Add("green", Blink1DeviceStatus.Red);
            MatchingRules_HARD.Add("red", Blink1DeviceStatus.Red);

            MatchingRules_SOFT.Add("no-connection", Blink1DeviceStatus.NoConnection);
            MatchingRules_SOFT.Add("grey", Blink1DeviceStatus.NoActivity);
            MatchingRules_SOFT.Add("green", Blink1DeviceStatus.Green);
            MatchingRules_SOFT.Add("red", Blink1DeviceStatus.Red);

            GetSavedSettings();

            mainForm = new ContextMenuMain();
            Application.Run(mainForm);
        }

        public enum Blink1DeviceStatus
        {
            Green,
            Red,
            NoActivity,
            NoConnection,
            TurnOff
        }

        public static string GetAPIRoot()
        {
            var regValue = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Codealike", "APIRoot", string.Empty);

            if (regValue == null || regValue.ToString() == string.Empty)
            {
                return "https://codealike.com/api/v2/";
            }
            else
            {
                return regValue.ToString();
            }
        }

        public static IDictionary<string, Program.Blink1DeviceStatus> GetMatchingRules(string serial)
        {
            StoredDevice rule;

            if (!Rules.TryGetValue(serial, out rule))
                return MatchingRules_SOFT;

            if (rule.MatchingRules.ToLowerInvariant() == "soft")
                return MatchingRules_SOFT;

            return MatchingRules_HARD;
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            ChangeBlink1DeviceStatus("all", Blink1DeviceStatus.TurnOff);
        }

        public static void ChangeBlink1DeviceStatus(string serial, Blink1DeviceStatus newStatus)
        {
            try
            {
                if (serial != "all")
                {
                    ChangeBlink1DeviceStatus(devices.First(x => x.SerialNumber == serial), newStatus);
                }
                else
                {
                    var devices = Blink1Connector.Scan();

                    foreach (var device in devices)
                    {
                        ChangeBlink1DeviceStatus(device, newStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void ChangeBlink1DeviceStatus(IBlink1 blink1, Blink1DeviceStatus status)
        {
            if (blink1 != null)
            {
                switch (status)
                {
                    case Blink1DeviceStatus.Green:
                        blink1.Set(Color.Green);
                        break;
                    case Blink1DeviceStatus.Red:
                        blink1.Set(Color.Red);
                        break;
                    case Blink1DeviceStatus.NoActivity:
                        blink1.Set(Color.FromArgb(34, 34, 34));
                        break;
                    case Blink1DeviceStatus.NoConnection:
                        blink1.Blink(Color.FromArgb(0, 50, 63), new TimeSpan(0, 0, 1), 5);
                        break;
                    case Blink1DeviceStatus.TurnOff:
                        blink1.TurnOff();
                        break;
                    default:
                        blink1.TurnOff();
                        break;
                }
            }
        }

        private static IEnumerable<IBlink1> devices;

        public static IEnumerable<IBlink1> GetBlinkDevices()
        {
            devices = Blink1Connector.Scan();
            return devices;
        }

        public static bool IdentifyDevice(string serialNumber)
        {
            var device = Blink1Connector.Connect(serialNumber);

            device.Blink(Color.Turquoise, new TimeSpan(0, 0, 1), 3);

            return true;
        }

        private static void GetSavedSettings()
        {
            Rules = new Dictionary<string, StoredDevice>();
            var devices = Program.GetBlinkDevices();

            foreach (var device in devices)
            {
                byte[] bytes = (byte[])Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Codealike", String.Format("{0}", device.SerialNumber), new byte[0]);
                var ms = new MemoryStream(bytes);

                IFormatter formatter = new BinaryFormatter();
                ms.Seek(0, SeekOrigin.Begin);
                StoredDevice storedDevice = (StoredDevice)formatter.Deserialize(ms);

                Program.Rules.Add(device.SerialNumber, storedDevice);
            }
        }

    }
}