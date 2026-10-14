using Serilog;

namespace WorkLifeBalance.Features.Tracking
{
    public class AppStateHandler
    {
        private AppState _appTimerState = AppState.Resting;

        public event Action<AppState>? OnStateChanges;

        public AppState AppTimerState
        {
            get => _appTimerState;
            set
            {
                if (_appTimerState == value)
                    return;

                _appTimerState = value;
                OnStateChanges?.Invoke(value);
            }
        }

        public void SetAppState(AppState state)
        {
            if (AppTimerState == state)
                return;

            AppTimerState = state;
            Log.Information("App state changed to {State}", state);
        }
    }
}
