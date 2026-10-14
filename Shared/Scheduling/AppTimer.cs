using Serilog;
using WorkLifeBalance.Shared.Data;

namespace WorkLifeBalance.Shared.Scheduling
{
    // Main timer that runs once a second, other features can subscribe to it and have their own run interval
    public class AppTimer
    {
        private readonly DataStorageFeature _dataStorage;
        private CancellationTokenSource _cancelTick = new();

        public AppTimer(DataStorageFeature dataStorage)
        {
            _dataStorage = dataStorage;
        }

        private event Func<Task>? OnTimerTick;

        public void StartTick()
        {
            _cancelTick.Cancel();
            _cancelTick = new();
            _ = TimerLoop(_cancelTick.Token);
        }

        public void Stop() => _cancelTick.Cancel();

        public bool IsFeaturePresent(Func<Task> feature) => OnTimerTick?.GetInvocationList().Contains(feature) == true;

        public void Subscribe(Func<Task> feature)
        {
            if (IsFeaturePresent(feature))
                return;

            OnTimerTick += feature;
            Log.Information("{Feature} subscribed to the main timer", feature.Method.Name);
        }

        public void UnSubscribe(Func<Task> feature)
        {
            if (!IsFeaturePresent(feature))
                return;

            OnTimerTick -= feature;
            Log.Information("{Feature} unsubscribed from the main timer", feature.Method.Name);
        }

        private async Task TimerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // stop the timer if the app is not ready or is closing
                if (!_dataStorage.IsAppReady && _dataStorage.IsClosingApp)
                {
                    Stop();
                    return;
                }

                try
                {
                    // Delay the triggering of the main event to pause every feature from being
                    // triggered while saving, so data is not updated while is saving
                    if (_dataStorage.IsAppSaving)
                    {
                        await Task.Delay(500, token);
                        continue;
                    }

                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException taskCancel)
                {
                    Log.Information("App Timer: {Message}", taskCancel.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "App Timer");
                }

                OnTimerTick?.Invoke();
            }
        }
    }
}
