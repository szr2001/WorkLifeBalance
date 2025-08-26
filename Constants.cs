using System.Collections.Frozen;
using System.Collections.Generic;

namespace WorkLifeBalance;

public static class Constants
{
    public const string ChromeProcess = "chrome.exe";
    public const string EdgeProcess = "msedge.exe";
    public const string FirefoxProcess = "firefox.exe";
    public static readonly FrozenSet<string> BrowserExecutables = new HashSet<string>()
    {
        ChromeProcess,
        EdgeProcess,
        FirefoxProcess
    }.ToFrozenSet();
}