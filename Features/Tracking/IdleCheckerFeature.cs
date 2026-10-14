using System.Numerics;
using Serilog;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Native;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Features.Tracking
{
    // AFK detection: the mouse in the same place between two checks means the user is idle
    public class IdleCheckerFeature : FeatureBase
    {
        private static readonly Vector2 NoPosition = new(-1, -1);

        private readonly DataStorageFeature _dataStorage;
        private readonly LowLevelHandler _lowLevelHandler;
        private readonly AppStateHandler _appStateHandler;
        private readonly IFeaturesService _featuresService;

        private Vector2 _oldMousePosition = NoPosition;

        public IdleCheckerFeature(DataStorageFeature dataStorage, LowLevelHandler lowLevelHandler, AppStateHandler appStateHandler, IFeaturesService featuresService)
        {
            _dataStorage = dataStorage;
            _lowLevelHandler = lowLevelHandler;
            _appStateHandler = appStateHandler;
            _featuresService = featuresService;
        }

        protected override Func<Task> ReturnFeatureMethod() => TriggerCheckIdle;

        private async Task TriggerCheckIdle()
        {
            if (IsFeatureRunning)
                return;

            try
            {
                IsFeatureRunning = true;
                var delay = _appStateHandler.AppTimerState == AppState.Idle
                    ? 2000
                    : _dataStorage.Settings.AutoDetectIdleInterval * 60000 / 2;

                await Task.Delay(delay, CancelTokenSource.Token);
                CheckIdle();
            }
            catch (TaskCanceledException taskCancel)
            {
                Log.Information("Idle Checker: {Message}", taskCancel.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Idle Checker");
            }
            finally
            {
                IsFeatureRunning = false;
            }
        }

        private void CheckIdle()
        {
            var newPosition = Vector2.Zero;
            try
            {
                newPosition = _lowLevelHandler.GetMousePos();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read the mouse position");
            }

            if (_oldMousePosition == NoPosition)
            {
                _oldMousePosition = newPosition;
                return;
            }

            if (newPosition == _oldMousePosition)
            {
                _featuresService.RemoveFeature<StateCheckerFeature>();
                _appStateHandler.SetAppState(AppState.Idle);
            }
            else
            {
                _featuresService.AddFeature<StateCheckerFeature>();
            }

            _oldMousePosition = newPosition;
        }
    }
}
