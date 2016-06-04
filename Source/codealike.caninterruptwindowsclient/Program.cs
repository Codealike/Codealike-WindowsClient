using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

            MatchingRules_HARD = new Dictionary<string, Program.CanFocusDeviceStatus>();
            MatchingRules_SOFT = new Dictionary<string, Program.CanFocusDeviceStatus>();

            // Set Matching rules.
            MatchingRules_HARD.Add("no-connection", CanFocusDeviceStatus.Green);
            MatchingRules_HARD.Add("grey", CanFocusDeviceStatus.Green);
            MatchingRules_HARD.Add("green", CanFocusDeviceStatus.Red);
            MatchingRules_HARD.Add("red", CanFocusDeviceStatus.Red);

            MatchingRules_SOFT.Add("no-connection", CanFocusDeviceStatus.Green);
            MatchingRules_SOFT.Add("grey", CanFocusDeviceStatus.Green);
            MatchingRules_SOFT.Add("green", CanFocusDeviceStatus.Green);
            MatchingRules_SOFT.Add("red", CanFocusDeviceStatus.Red);

            MatchingRules = GetMatchingRules();

            mainForm = new ContextMenuMain();
            Application.Run(mainForm);
        }
        // This includes the rules for matching status between Codealike (KEY) and CanFocus (VALUE).
        public static IDictionary<string, CanFocusDeviceStatus> MatchingRules_HARD;
        public static IDictionary<string, CanFocusDeviceStatus> MatchingRules_SOFT;
        public static IDictionary<string, Program.CanFocusDeviceStatus> MatchingRules;
        public enum CanFocusDeviceStatus
        {
            Green,
            Red
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
        public static IDictionary<string, Program.CanFocusDeviceStatus> GetMatchingRules()
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
    }
}
