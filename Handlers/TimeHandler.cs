using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkLifeBalance.Handlers.Feature;

namespace WorkLifeBalance.Handlers
{
    public class TimeHandler
    {
        //Main timer that runs once a second, other features can subscribe to it and have their own run interval
        private static TimeHandler? _instance;
        public static TimeHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimeHandler();
                }
                return _instance;
            }
        }

        public delegate void TickEvent();

        private event TickEvent OnTimerTick = delegate { };
        public AppState AppTimmerState = AppState.Resting;

        private CancellationTokenSource CancelTick = new();

        public void StartTick()
        {
            CancelTick.Cancel();

            CancelTick = new();

            _ = TimerLoop(CancelTick.Token);
        }

        public void Subscribe(TickEvent eventname)
        {
            if(!OnTimerTick.GetInvocationList().Contains(eventname))
            {
                OnTimerTick += eventname;
            }
        }

        public void UnSubscribe(TickEvent eventname)
        {
            if(OnTimerTick.GetInvocationList().Contains(eventname))
            {
                OnTimerTick -= eventname;
            }
        }

        public void Stop()
        {
            CancelTick.Cancel();
        }

        private async Task TimerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                //stop the timer if the app is not ready or is closing
                if (!DataHandler.Instance.IsAppReady && DataHandler.Instance.IsClosingApp) 
                {
                    Stop();
                }

                try
                {
                    //Delay the triggering of the main event to pause every feature from being
                    //triggered while saving, so data is not updated while is saving
                    if (DataHandler.Instance.IsAppSaving)
                    {
                        await Task.Delay(500);
                        continue;
                    }

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                OnTimerTick?.Invoke();
            }
        }
    }
}
