using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace WorkLifeBalance.Shared.Native
{
    public class LowLevelHandler
    {
        private const byte VK_LWIN = 0x5B;
        private const byte VK_D = 0x44;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private Dictionary<string, List<uint>> _browserMap = new();

        private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        public void EnableConsole() => AllocConsole();

        public void OpenLink(string link) => Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });

        public void MinimizeAllApps()
        {
            keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
            keybd_event(VK_D, 0, 0, UIntPtr.Zero);
            keybd_event(VK_D, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public nint ReadForegroundWindow() => GetForegroundWindow();

        public string GetProcessWithId(nint hWnd, out uint processId)
        {
            GetWindowThreadProcessId(hWnd, out processId);
            var process = Process.GetProcessById((int)processId);

            const int nChars = 1024;
            var applicationName = new StringBuilder(nChars);
            GetModuleFileNameEx(process.Handle, nint.Zero, applicationName, nChars);

            return System.IO.Path.GetFileName(applicationName.ToString());
        }

        public string? GetActiveTab(uint processId)
        {
            var process = Process.GetProcessById((int)processId);
            return process.MainWindowHandle == nint.Zero ? null : BrowserHelper.GetUrl(process);
        }

        public List<string> GetBackgroundApplicationsName()
        {
            var windows = new List<nint>();
            EnumWindows((hWnd, _) =>
            {
                if (IsWindowVisible(hWnd))
                    windows.Add(hWnd);

                return true;
            }, nint.Zero);

            var names = new HashSet<string>();
            var browsers = new Dictionary<string, List<uint>>();
            foreach (var window in windows)
            {
                var processName = GetProcessWithId(window, out var id);
                if (BrowserHelper.BrowserExecutables.Contains(processName))
                {
                    if (browsers.TryGetValue(processName, out var pids))
                        pids.Add(id);
                    else
                        browsers[processName] = new List<uint> { id };
                }

                names.Add(processName);
            }

            _browserMap = browsers;
            return names.ToList();
        }

        public List<string> GetActiveBackgroundTabs()
        {
            var urls = new HashSet<string>();
            foreach (var pid in _browserMap.Values.SelectMany(pids => pids))
            {
                var url = GetActiveTab(pid);
                if (!string.IsNullOrEmpty(url))
                    urls.Add(url);
            }

            return urls.ToList();
        }

        public Vector2 GetMousePos()
        {
            GetCursorPos(out var point);
            return new Vector2(point.X, point.Y);
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(nint hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(nint hProcess, nint hModule, StringBuilder lpBaseName, int nSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
