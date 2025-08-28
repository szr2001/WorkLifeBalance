﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Win32.TaskScheduler;
using System.Text;
using System.Windows.Automation;
using WorkLifeBalance.Services.Feature;
using Serilog;
using WorkLifeBalance.Helpers;

namespace WorkLifeBalance.Services
{
    //low level handling of windows and mouse
    public class LowLevelHandler
    {
        private Dictionary<string, List<uint>?> browserMap;
        private readonly DataStorageFeature dataStorageFeature;
        public LowLevelHandler(DataStorageFeature dataStorageFeature)
        {
            this.browserMap = new Dictionary<string, List<uint>?>();
            this.dataStorageFeature = dataStorageFeature;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);
        private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(nint hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        const byte VK_LWIN = 0x5B; // Virtual Key for Left Windows key
        const byte VK_D = 0x44;    // Virtual Key for 'D' key
        const uint KEYEVENTF_KEYUP = 0x0002; // Flag for key release

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

        public void MinimizeAllApps()
        {
            // win down
            keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
            // D down
            keybd_event(VK_D, 0, 0, UIntPtr.Zero);

            // d up
            keybd_event(VK_D, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            // win up
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public void CreateStartupShortcut()
        {
            try
            {
                using (TaskService taskService = new TaskService())
                {
                    DeleteStartupShortcut();

                    TaskDefinition taskDefinition = taskService.NewTask();
                    taskDefinition.RegistrationInfo.Description = $"Run {dataStorageFeature.Settings.AppName} at windows startup as administrator to start recording activity.";

                    taskDefinition.Triggers.Add(new LogonTrigger());

                    taskDefinition.Actions.Add(new ExecAction(dataStorageFeature.Settings.AppExePath, null, dataStorageFeature.Settings.AppDirectory));

                    taskDefinition.Settings.StopIfGoingOnBatteries = false;
                    taskDefinition.Settings.StartWhenAvailable = true;
                    taskDefinition.Settings.RestartInterval = TimeSpan.FromMinutes(1);
                    taskDefinition.Settings.RestartCount = 3;
                    taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero;

                    taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;
                    taskService.RootFolder.RegisterTaskDefinition(dataStorageFeature.Settings.AppName, taskDefinition);
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex,"");
            }
        }

        public void RegenerateStartupShortcut()
        {
            using (TaskService taskService = new TaskService())
            {
                Task existingTask = taskService.FindTask(dataStorageFeature.Settings.AppName);
                if (existingTask != null)
                {
                    CreateStartupShortcut();
                }
            }
        }

        public void DeleteStartupShortcut()
        {
            using (TaskService taskService = new TaskService())
            {
                Task existingTask = taskService.FindTask(dataStorageFeature.Settings.AppName);
                if (existingTask != null)
                {
                    taskService.RootFolder.DeleteTask(dataStorageFeature.Settings.AppName);
                }
            }
        }

        public nint ReadForegroundWindow()
        {
            return GetForegroundWindow();
        }

        public string GetProcessWithId(nint hWnd, out uint id)
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            Process process = Process.GetProcessById((int)processId);
            id = processId;
            
            const int nChars = 1024;
            StringBuilder applicationName = new StringBuilder(nChars);
            GetModuleFileNameEx(process.Handle, nint.Zero, applicationName, nChars);

            // Get only the executable file name
            string fileName = System.IO.Path.GetFileName(applicationName.ToString());

            return fileName;
        }

        public string? GetActiveTab(uint processId)
        {
            Process proc = Process.GetProcessById((int)processId);
            
            if (proc.MainWindowHandle == IntPtr.Zero)
            {
                return null;
            }

            return BrowserHelper.GetUrl(proc);
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

            Dictionary<string, List<uint>?> namePidMap = new();
            foreach (nint windowId in windows)
            {
                string processName = GetProcessWithId(windowId, out uint id);

                if (Constants.BrowserExecutables.Contains(processName))
                {
                    if(namePidMap.TryGetValue(processName, out List<uint>? pids))
                    {
                        pids.Add(id);
                    } else
                    {
                        namePidMap[processName] = new List<uint>() { id };
                    }
                }
                
                Appnames.Add(processName);
            }
            browserMap = namePidMap;
            
            return Appnames.ToList();
        }

        public List<string> GetActiveBackgroundTabs()
        {
            List<uint> pids = browserMap.Values.SelectMany(x => x).ToList();
            HashSet<string> result = new();
            
            foreach (var pid in pids)
            {
                string? url = GetActiveTab(pid);
                
                if (!string.IsNullOrEmpty(url))
                {
                    result.Add(url);
                }
            }

            return result.ToList();
        }
        
        public Vector2 GetMousePos()
        {
            GetCursorPos(out POINT p);
            Vector2 pos = new Vector2(p.X, p.Y);

            return pos;
        }

    }
}