using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace WorkLifeBalance.Services
{
    //low level handling of windows and mouse
    public class LowLevelHandler
    {
        // Constants for the SetWindowLoc functions
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        //read mouse pos
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);
        // Delegate for EnumWindows for callback
        private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(nint hWnd);

        [DllImport("user32.dll")]
        private static extern nint FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow();


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);


        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);


        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(nint hProcess, nint hModule, StringBuilder lpBaseName, int nSize);

        //create a point to store thelocation of a point on the screen
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        public void EnableConsole()
        {
            AllocConsole();
        }

        public nint ReadForegroundWindow()
        {
            return GetForegroundWindow();
        }

        public void SetWindowLocation(nint windowHandle, int x, int y)
        {
            // Call SetWindowPos with the SWP_NOSIZE and SWP_NOZORDER flags
            SetWindowPos(windowHandle, nint.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        public nint GetWindow(string? lpClassName, string lpWindowName)
        {
            return FindWindow(lpClassName, lpWindowName);
        }

        public string GetWindowTitle(nint hWnd)
        {
            const int nChars = 256;
            StringBuilder windowTitle = new StringBuilder(nChars);
            GetWindowText(hWnd, windowTitle, nChars);
            return windowTitle.ToString();
        }

        public string GetProcessname(nint hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            Process process = Process.GetProcessById((int)processId);

            const int nChars = 1024;
            StringBuilder applicationName = new StringBuilder(nChars);
            GetModuleFileNameEx(process.Handle, nint.Zero, applicationName, nChars);

            // Get only the executable file name
            string fileName = System.IO.Path.GetFileName(applicationName.ToString());

            return fileName;
        }

        public List<string> GetBackgroundApplicationsName()
        {
            HashSet<string> Appnames = new();

            List<nint> windows = new List<nint>();
            EnumWindows(EnumWindowsCallback, GCHandle.ToIntPtr(GCHandle.Alloc(windows)));

            foreach (nint windowId in windows)
            {
                Appnames.Add(GetProcessname(windowId));
            }

            return Appnames.ToList();
        }

        public Vector2 GetMousePos()
        {
            GetCursorPos(out POINT p);
            Vector2 pos = new Vector2(p.X, p.Y);

            return pos;
        }

        public bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private bool EnumWindowsCallback(nint hWnd, nint lParam)
        {
            List<nint> windows = GCHandle.FromIntPtr(lParam).Target as List<nint>;

            if (IsWindowVisible(hWnd))
            {
                windows.Add(hWnd);
            }

            return true;
        }
    }
}