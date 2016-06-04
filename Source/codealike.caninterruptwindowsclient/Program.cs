using Microsoft.Win32;
using Sleddog.Blink1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Codealike.CanInterruptWindowsClient
{
    static class Program
    {
        public static string Username;
        public static string MatchingRulesKey;
        public static ContextMenuMain mainForm;
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
            MatchingRules_HARD.Add("grey", Blink1DeviceStatus.Off);
            MatchingRules_HARD.Add("green", Blink1DeviceStatus.Red);
            MatchingRules_HARD.Add("red", Blink1DeviceStatus.Red);

            MatchingRules_SOFT.Add("no-connection", Blink1DeviceStatus.NoConnection);
            MatchingRules_SOFT.Add("grey", Blink1DeviceStatus.Off);
            MatchingRules_SOFT.Add("green", Blink1DeviceStatus.Green);
            MatchingRules_SOFT.Add("red", Blink1DeviceStatus.Red);

            MatchingRules = GetMatchingRules();

            mainForm = new ContextMenuMain();
            Application.Run(mainForm);
        }
        // This includes the rules for matching status between Codealike (KEY) and CanFocus (VALUE).
        public static IDictionary<string, Blink1DeviceStatus> MatchingRules_HARD;
        public static IDictionary<string, Blink1DeviceStatus> MatchingRules_SOFT;
        public static IDictionary<string, Program.Blink1DeviceStatus> MatchingRules;
        public enum Blink1DeviceStatus
        {
            Green,
            Red,
            Off,
            NoConnection
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
        public static IDictionary<string, Program.Blink1DeviceStatus> GetMatchingRules()
        {
            var regValue = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Codealike", "MatchingRules", string.Empty);

            if (regValue == null || regValue.ToString() == string.Empty)
            {
                MatchingRulesKey = "hard";
                return MatchingRules_HARD;
            }

            MatchingRulesKey = regValue.ToString();

            if (regValue.ToString().ToLowerInvariant() == "hard")
            {
                return MatchingRules_HARD;
            }

            if (regValue.ToString().ToLowerInvariant() == "soft")
            {
                return MatchingRules_SOFT;
            }

            return MatchingRules_HARD;
        }
        static void OnProcessExit(object sender, EventArgs e)
        {
            ChangeBlink1DeviceStatus(Blink1DeviceStatus.Off);
        }

        public static void ChangeBlink1DeviceStatus(Blink1DeviceStatus newStatus)
        {
            try
            {
                var blink1Devices = Blink1Connector.Scan();

                IBlink1 blink1;

                if (blink1Devices.Any())
                {
                    blink1 = blink1Devices.First();

                    switch (newStatus)
                    {
                        case Program.Blink1DeviceStatus.Green:
                            blink1.Set(Color.Green);
                            break;
                        case Program.Blink1DeviceStatus.Red:
                            blink1.Set(Color.Red);
                            break;
                        case Program.Blink1DeviceStatus.Off:
                            blink1.Blink(Color.White, new TimeSpan(0, 0, 1), 5);
                            break;
                        case Program.Blink1DeviceStatus.NoConnection:
                            blink1.Blink(Color.White, new TimeSpan(0, 0, 1), 5);
                            break;
                        default:
                            blink1.TurnOff();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
