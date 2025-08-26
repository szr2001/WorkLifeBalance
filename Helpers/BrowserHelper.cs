using System;
using System.Diagnostics;
using System.Windows.Automation;
using Serilog;

namespace WorkLifeBalance.Helpers;

public static class BrowserHelper
{
    public static string? GetUrl(Process process)
    {
        string processName = process.ProcessName;
        switch (processName)
        {
            case "chrome":
                return GetChromeBrowserUrl(process);
            case "msedge":
                return GetEdgeBrowserUrl(process);
            case "firefox":
                return GetGeckoBrowserUrl(process);
        }

        return null;
    }

    private static string? GetGeckoBrowserUrl(Process process)
    {
        AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
        AutomationElement bar = element.FindFirst(TreeScope.Descendants,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));

        if (bar == null)
        {
            return null;
        }
        
        AutomationElement comboBox = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox));
        
        if (comboBox != null && comboBox.TryGetCurrentPattern(ValuePattern.Pattern, out object patternObject))
        {
            return (patternObject as ValuePattern)?.Current.Value;
        }

        return null;
    }

    private static string? GetChromeBrowserUrl(Process process)
    {
        AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
        TreeWalker walker = new TreeWalker(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
        AutomationElement? child = walker.GetFirstChild(element);

        if (child != null && child.TryGetCurrentPattern(ValuePattern.Pattern, out var patternObject))
        {
            return (patternObject as ValuePattern)?.Current.Value;
        }
        
        return null;
    }
    
    private static string? GetEdgeBrowserUrl(Process process)
    {
        AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
        AutomationElement bar = element.FindFirst(TreeScope.Descendants,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

        if (bar != null)
        {
            AutomationPattern[]? patterns = bar.GetSupportedPatterns();
            if (patterns.Length > 0)
            {
                try
                {
                    ValuePattern value = (ValuePattern)bar.GetCurrentPattern(patterns[0]);

                    return value.Current.Value;
                }
                catch (InvalidOperationException e)
                {
                    Log.Warning("Could not get browser URL: {Message}", e.Message);

                    return null;
                }
            }
        }
        
        return null;
    }
}