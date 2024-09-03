using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.Services
{
    //Main timer that runs once a second, other features can subscribe to it and have their own run interval
    public class AppTimer
    {
        public event Action<AppState> OnStateChanges = delegate { };
        public AppState AppTimerState 
        {
            get 
            {
                return appTimerState;
            }
            set
            {
                appTimerState = value;
                OnStateChanges?.Invoke(appTimerState);
            }
        }

        private AppState appTimerState = AppState.Resting;
        private event Action OnTimerTick = delegate { };
        private CancellationTokenSource CancelTick = new();

        public void StartTick()
        {
            CancelTick.Cancel();

            CancelTick = new();

            _ = TimerLoop(CancelTick.Token);
        }

        public void Subscribe(Action eventname)
        {
            if (!OnTimerTick.GetInvocationList().Contains(eventname))
            {
                OnTimerTick += eventname;
                Log.Information($"{eventname.Method.Name} Subscribed to Main Timer");
            }
        }

        public void UnSubscribe(Action eventname)
        {
            if (OnTimerTick.GetInvocationList().Contains(eventname))
            {
                OnTimerTick -= eventname;
                Log.Information($"{eventname.Method.Name} UnSubscribed from Main Timer");
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
                if (!DataStorageFeature.Instance.IsAppReady && DataStorageFeature.Instance.IsClosingApp)
                {
                    Stop();
                }

                try
                {
                    //Delay the triggering of the main event to pause every feature from being
                    //triggered while saving, so data is not updated while is saving
                    if (DataStorageFeature.Instance.IsAppSaving)
                    {
                        await Task.Delay(500);
                        continue;
                    }

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "TimeHandler timer loop");
                }

                OnTimerTick?.Invoke();
            }
        }
    }
}
