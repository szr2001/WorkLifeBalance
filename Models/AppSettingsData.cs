using Serilog;
using System;

namespace WorkLifeBalance.Models
{
    public class AppSettingsData
    {
        public readonly string Version = "2.0.1";
        public string AppName;
        public string AppDirectory;
        public string AppExePath;
        public string LastTimeOpened { get; set; } = "";
        public int SaveInterval { get; set; } = 5;
        public int AutoDetectInterval { get; set; } = 1;
        public int AutoDetectIdleInterval { get; set; } = 1;
        public int StartWithWindows { get; set; } = 0;

        public DateTime LastTimeOpenedC = new();
        public bool StartWithWindowsC = false;

        public Action OnSettingsChanged = new(() => { });

        public AppSettingsData()
        {
            AppName = "WorkLifeBalance";
            AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AppExePath = $"{AppDirectory}/{AppName}.exe";
        }

        public void ConvertSaveDataToUsableData()
        {
            try
            {
                if (!string.IsNullOrEmpty(LastTimeOpened))
                {
                    LastTimeOpenedC = new DateTime
                        (
                            int.Parse(LastTimeOpened.Substring(8, 4)),
                            int.Parse(LastTimeOpened.Substring(4, 2)),
                            int.Parse(LastTimeOpened.Substring(6, 2)),
                            int.Parse(LastTimeOpened.Substring(0, 2)),
                            int.Parse(LastTimeOpened.Substring(2, 2)),
                            0
                        );
                }
                StartWithWindowsC = StartWithWindows == 1;

            }
            catch (Exception ex)
            {
                Log.Error("AppSettings Error: Failed to convert data to usable data", ex);
            }

        }

        public void ConvertUsableDataToSaveData()
        {
            try
            {
                LastTimeOpenedC = DateTime.Now;
                LastTimeOpened = LastTimeOpenedC.ToString("HHmmMMddyyyy");

                StartWithWindows = StartWithWindowsC ? 1 : 0;
            }
            catch (Exception ex)
            {
                Log.Error("AppSettings Error: Failed to convert usable data to save data", ex);
            }
        }
    }
}
