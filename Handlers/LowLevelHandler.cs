using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace WorkLifeBalance.Handlers
{
    public class LowLevelHandler
    {
        // Constants for the SetWindowLoc functions
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        //read mouse pos
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        // Delegate for EnumWindows for callback
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, int nSize);

        //create a point to store thelocation of a point on the screen
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
        public static void SetWindowLocation(IntPtr windowHandle, int x, int y)
        {
            // Call SetWindowPos with the SWP_NOSIZE and SWP_NOZORDER flags
            SetWindowPos(windowHandle, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        public static IntPtr GetWindow(string? lpClassName, string lpWindowName)
        {
            return FindWindow(lpClassName, lpWindowName);
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            const int nChars = 256;
            StringBuilder windowTitle = new StringBuilder(nChars);
            GetWindowText(hWnd, windowTitle, nChars);
            return windowTitle.ToString();
        }

        public static string GetProcessname(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            Process process = Process.GetProcessById((int)processId);

            const int nChars = 1024;
            StringBuilder applicationName = new StringBuilder(nChars);
            GetModuleFileNameEx(process.Handle, IntPtr.Zero, applicationName, nChars);

            // Get only the executable file name
            string fileName = System.IO.Path.GetFileName(applicationName.ToString());

            return fileName;
        }
        public static List<string> GetBackgroundApplicationsName()
        {
            HashSet<string> Appnames = new();

            List<IntPtr> windows = new List<IntPtr>();
            EnumWindows(EnumWindowsCallback, GCHandle.ToIntPtr(GCHandle.Alloc(windows)));

            foreach (IntPtr windowId in windows)
            {
                Appnames.Add(GetProcessname(windowId));
            }

            return Appnames.ToList();
        }

        public static Vector2 GetMousePos()
        {
            GetCursorPos(out POINT p);
            Vector2 pos = new Vector2(p.X, p.Y);

            return pos;
        }

        public static bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            List<IntPtr> windows = GCHandle.FromIntPtr(lParam).Target as List<IntPtr>;

            if (IsWindowVisible(hWnd))
            {
                windows.Add(hWnd);
            }

            return true;
        }
    }
}