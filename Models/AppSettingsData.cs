using Serilog;
using System;

namespace WorkLifeBalance.Models
{
    public class AppSettingsData
    {
        public string LastTimeOpened { get; set; } = "";
        public int SaveInterval { get; set; } = 5;
        public int AutoDetectInterval { get; set; } = 1;
        public int AutoDetectIdleInterval { get; set; } = 1;
        public int StartWithWindows { get; set; } = 0;
        public int AutoDetectWorking { get; set; } = 0;
        public int AutoDetectIdle { get; set; } = 0;
        public int StartUpCorner { get; set; } = 0;

        public DateTime LastTimeOpenedC = new();
        public bool StartWithWindowsC = false;
        public bool AutoDetectWorkingC = false;
        public bool AutoDetectIdleC = false;
        public AnchorCorner StartUpCornerC = AnchorCorner.BootomLeft;

        public Action OnSettingsChanged = new(() => { });

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

                AutoDetectWorkingC = AutoDetectWorking == 1;

                AutoDetectIdleC = AutoDetectIdle == 1;

                StartUpCornerC = (AnchorCorner)StartUpCorner;
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

                AutoDetectWorking = AutoDetectWorkingC ? 1 : 0;

                AutoDetectIdle = AutoDetectIdleC ? 1 : 0;

                StartUpCorner = (int)StartUpCornerC;
            }
            catch (Exception ex)
            {
                Log.Error("AppSettings Error: Failed to convert usable data to save data", ex);
            }
        }
    }
    public enum AnchorCorner
    {
        TopLeft,
        TopRight,
        BootomLeft,
        BottomRight
    }
}
