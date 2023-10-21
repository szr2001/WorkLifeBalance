using System;
using System.Buffers;
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

        public delegate void ActiveProcess(string ActiveWindow);
        public event ActiveProcess OnWindowChange;


        public string ActiveWindow = "";

        private TimeSpan OneSec = new TimeSpan(0, 0, 1);

        private bool IsAutoWorkDetectionTriggered = false;

        public void RecordActivity()
        {
            try
            {
                IntPtr foregroundWindowHandle = WindowOptionsHelper.GetForegroundWindow();
                
                ActiveWindow = WindowOptionsHelper.GetProcessname(foregroundWindowHandle);
                OnWindowChange?.Invoke(ActiveWindow);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                TimeOnly IncreasedTimeSpan = DataHandler.Instance.AutoChangeData.ActivitiesC[ActiveWindow].Add(OneSec);
                DataHandler.Instance.AutoChangeData.ActivitiesC[ActiveWindow] = IncreasedTimeSpan;
            }
            catch
            {
                DataHandler.Instance.AutoChangeData.ActivitiesC.Add(ActiveWindow,new TimeOnly());
            }
        }
        public async void TriggerWorkDetect()
        {
            if (IsAutoWorkDetectionTriggered) return;

            IsAutoWorkDetectionTriggered = true;

            await Task.Delay(DataHandler.Instance.Settings.AutoDetectInterval * 1000);

            CheckStateChange();

            IsAutoWorkDetectionTriggered = false;
        }

        private void CheckStateChange()
        {
            if (string.IsNullOrEmpty(ActiveWindow)) return;

            bool IsFocusingOnWorkingWindow = DataHandler.Instance.AutoChangeData.WorkingStateWindows.Contains(ActiveWindow);

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
