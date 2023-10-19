using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkLifeBalance.HandlerClasses;
using static Dapper.SqlMapper;

namespace WorkLifeBalance.Handlers
{
    public class AutomaticStateChangerHandler
    {
        private static AutomaticStateChangerHandler? _instance;
        public static AutomaticStateChangerHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AutomaticStateChangerHandler();
                }
                return _instance;
            }
        }

        public TimeOnly CurrentTime = new();
        public string ActiveWindow = "";

        private TimeSpan OneSec = new TimeSpan(0, 0, 1);

        private bool IsAutoWorkDetectionTriggered = false;

        public void TickTime()
        {
            CurrentTime  = CurrentTime.Add(OneSec);
        }
        public async void TriggerWorkDetect()
        {
            if (IsAutoWorkDetectionTriggered) return;

            IsAutoWorkDetectionTriggered = true;

            await Task.Delay(DataHandler.Instance.Settings.AutoDetectInterval * 1000);

            try
            {
                IntPtr foregroundWindowHandle = WindowOptionsHelper.GetForegroundWindow();
                ActiveWindow = WindowOptionsHelper.GetProcessname(foregroundWindowHandle);

                CalculateAutomaticDetection();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            IsAutoWorkDetectionTriggered = false;
        }

        private void CalculateAutomaticDetection()
        {
            bool IsFocusingOnWorkingWindow = DataHandler.Instance.AutoChangeData.WorkingStateWindows.Contains(ActiveWindow);

            Console.WriteLine(ActiveWindow);

            switch (TimeHandler.Instance.AppTimmerState)
            {
                case TimmerState.Working:
                    if (IsFocusingOnWorkingWindow)
                    {
                        return;
                    }
                    else
                    {
                        MainWindow.instance.SetAppState(TimmerState.Resting);
                    }

                    break;

                case TimmerState.Resting:
                    if (!IsFocusingOnWorkingWindow)
                    {
                        return;
                    }
                    else
                    {
                        MainWindow.instance.SetAppState(TimmerState.Working);
                    }
                    break;
            }
        }
    }
}
