using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.Services
{
    //low level handling of windows and mouse
    public class LowLevelHandler
    {
        private readonly DataStorageFeature dataStorageFeature;
        public LowLevelHandler(DataStorageFeature dataStorageFeature)
        {
            this.dataStorageFeature = dataStorageFeature;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);
        private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(nint hWnd);

        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow();

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

        public void OpenLink(string link)
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }

        public void SetForeground(string process)
        {
            nint window = -1;
            bool EnumWindowsCallback(nint hWnd, nint lParam)
            {
                // Get the process name associated with the window.
                string processName = GetProcessname(hWnd);

                // Check if they are the same process
                if (processName.Equals(process, StringComparison.OrdinalIgnoreCase) && IsWindowVisible(hWnd))
                {
                    window = hWnd;
                    return false;
                }

                return true;
            }

            // Enumerate through all windows.
            EnumWindows(EnumWindowsCallback, nint.Zero);
            if(window != -1)
            {
                SetForegroundWindow(window);
            }
        }

        public void RestartApplicationWithAdmin()
        {
            var psi = new ProcessStartInfo
            {
                FileName = dataStorageFeature.Settings.AppExePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(psi);
            App.Current.Shutdown();
        }

        public void MinimizeWindow(string process)
        {
            List<nint> Windows = new();

            // Callback function to find the window associated with the process name.
            bool EnumWindowsCallback(nint hWnd, nint lParam)
            {
                // Get the process name associated with the window.
                string processName = GetProcessname(hWnd);

                // Check if they are the same process
                if (processName.Equals(process, StringComparison.OrdinalIgnoreCase) && IsWindowVisible(hWnd))
                {
                    Windows.Add(hWnd);
                }

                return true;
            }

            // Enumerate through all windows.
            EnumWindows(EnumWindowsCallback, nint.Zero);

            //hide all windows found with that process
            foreach(nint window in Windows)
            {
                ShowWindow(window, 2);
            }
        }

        public nint ReadForegroundWindow()
        {
            return GetForegroundWindow();
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

            bool EnumWindowsCallback(nint hWnd, nint lParam)
            {
                if (IsWindowVisible(hWnd))
                {
                    windows.Add(hWnd);
                }

                return true;
            }

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
    }
}