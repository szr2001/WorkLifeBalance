using System.Collections.Frozen;
using System.Diagnostics;
using System.Windows.Automation;
using Serilog;

namespace WorkLifeBalance.Shared.Native
{
    public static class BrowserHelper
    {
        public const string ChromeProcess = "chrome.exe";
        public const string EdgeProcess = "msedge.exe";
        public const string FirefoxProcess = "firefox.exe";

        public static readonly FrozenSet<string> BrowserExecutables = new[] { ChromeProcess, EdgeProcess, FirefoxProcess }.ToFrozenSet();

        public static string? GetUrl(Process process) => process.ProcessName switch
        {
            "chrome" => GetChromeBrowserUrl(process),
            "msedge" => GetEdgeBrowserUrl(process),
            "firefox" => GetGeckoBrowserUrl(process),
            _ => null,
        };

        private static string? GetGeckoBrowserUrl(Process process)
        {
            var element = AutomationElement.FromHandle(process.MainWindowHandle);
            var bar = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));
            if (bar == null)
                return null;

            var comboBox = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox));
            if (comboBox != null && comboBox.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
                return (pattern as ValuePattern)?.Current.Value;

            return null;
        }

        private static string? GetChromeBrowserUrl(Process process)
        {
            var element = AutomationElement.FromHandle(process.MainWindowHandle);
            var walker = new TreeWalker(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
            var child = walker.GetFirstChild(element);

            if (child != null && child.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
                return (pattern as ValuePattern)?.Current.Value;

            return null;
        }

        private static string? GetEdgeBrowserUrl(Process process)
        {
            var element = AutomationElement.FromHandle(process.MainWindowHandle);
            var bar = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
            if (bar == null)
                return null;

            var patterns = bar.GetSupportedPatterns();
            if (patterns.Length == 0)
                return null;

            try
            {
                return ((ValuePattern)bar.GetCurrentPattern(patterns[0])).Current.Value;
            }
            catch (InvalidOperationException e)
            {
                Log.Warning("Could not get browser URL: {Message}", e.Message);
                return null;
            }
        }
    }
}
