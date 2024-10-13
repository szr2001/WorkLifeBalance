using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.Services
{
    //Main timer that runs once a second, other features can subscribe to it and have their own run interval
    public class AppTimer
    {
        public event Action<AppState>? OnStateChanges;

        private DataStorageFeature dataStorageFeature;
        public AppTimer(DataStorageFeature dataStorageFeature)
        {
            this.dataStorageFeature = dataStorageFeature;
        }
        public AppState AppTimerState
        {
            get
            {
                return appTimerState;
            }
            set
            {
                if (appTimerState == value) return;
                appTimerState = value;
                OnStateChanges?.Invoke(appTimerState);
            }
        }

        private AppState appTimerState = AppState.Resting;
        private event Action? OnTimerTick;
        private CancellationTokenSource CancelTick = new();

        public void StartTick()
        {
            CancelTick.Cancel();

            CancelTick = new();

            _ = TimerLoop(CancelTick.Token);
        }

        public void Subscribe(Action eventname)
        {
            if (OnTimerTick != null)
            {
                if (OnTimerTick.GetInvocationList().Contains(eventname)) return;
            }
            OnTimerTick += eventname;
            Log.Information($"{eventname.Method.Name} Subscribed to Main Timer");
        }

        public void UnSubscribe(Action eventname)
        {
            if (OnTimerTick == null) return;

            if (OnTimerTick.GetInvocationList().Contains(eventname))
            {
                OnTimerTick -= eventname;
                Log.Information($"{eventname.Method.Name} UnSubscribed from Main Timer");
            }
        }

        public void SetAppState(AppState state)
        {
            if (AppTimerState == state) return;

            AppTimerState = state;
            Log.Information($"App state changed to {state}");
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
                if (!dataStorageFeature.IsAppReady && dataStorageFeature.IsClosingApp)
                {
                    Stop();
                }

                try
                {
                    //Delay the triggering of the main event to pause every feature from being
                    //triggered while saving, so data is not updated while is saving
                    if (dataStorageFeature.IsAppSaving)
                    {
                        await Task.Delay(500);
                        continue;
                    }

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Log.Information(ex, "TimeHandler timer loop");
                }

                OnTimerTick?.Invoke();
            }
        }
    }
}
