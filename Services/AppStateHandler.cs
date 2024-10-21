using Serilog;
using System;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.Services
{
    public class AppStateHandler
    {
        public event Action<AppState>? OnStateChanges;
        
        private AppState appTimerState = AppState.Resting;
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

        public void SetAppState(AppState state)
        {
            if (AppTimerState == state) return;

            AppTimerState = state;
            Log.Information($"App state changed to {state}"); // probl?
        }
    }
}
