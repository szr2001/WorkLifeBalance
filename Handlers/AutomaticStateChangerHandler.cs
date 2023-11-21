using System;
using System.Linq;
using System.Threading.Tasks;
using WorkLifeBalance.HandlerClasses;

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
        public event ActiveProcess? OnWindowChange;

        public bool IsFocusingOnWorkingWindow = false;

        public string ActiveWindow = "";

        private TimeSpan OneSec = new TimeSpan(0, 0, 1);


        public void TriggerRecordActivity()
        {
            try
            {
                IntPtr foregroundWindowHandle = LowLevelHandler.GetForegroundWindow();
                
                ActiveWindow = LowLevelHandler.GetProcessname(foregroundWindowHandle);
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

        private bool IsAutoWorkDetectionTriggered = false;
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

            IsFocusingOnWorkingWindow = DataHandler.Instance.AutoChangeData.WorkingStateWindows.Contains(ActiveWindow);

            switch (TimeHandler.Instance.AppTimmerState)
            {
                case AppState.Working:
                    if (IsFocusingOnWorkingWindow)
                    {
                        return;
                    }
                    else
                    {
                        MainWindow.instance.SetAppState(AppState.Resting);
                    }

                    break;

                case AppState.Resting:
                    if (!IsFocusingOnWorkingWindow)
                    {
                        return;
                    }
                    else
                    {
                        MainWindow.instance.SetAppState(AppState.Working);
                    }
                    break;
            }
        }
    }
}
