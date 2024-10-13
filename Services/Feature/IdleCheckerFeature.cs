using Serilog;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace WorkLifeBalance.Services.Feature
{
    public class IdleCheckerFeature : FeatureBase
    {
        private Vector2 _oldmousePosition = new Vector2(-1, -1);
        private readonly AppStateHandler appStateHandler;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly LowLevelHandler lowLevelHandler;
        public IdleCheckerFeature(DataStorageFeature dataStorageFeature, LowLevelHandler lowLevelHandler, AppStateHandler appStateHandler)
        {
            this.dataStorageFeature = dataStorageFeature;
            this.lowLevelHandler = lowLevelHandler;
            this.appStateHandler = appStateHandler;
        }

        protected override Action ReturnFeatureMethod()
        {
            return TriggerCheckIdle;
        }

        private bool IsCheckingIdleTriggered = false;
        private async void TriggerCheckIdle()
        {
            if (IsCheckingIdleTriggered) return;

            int delay = 0;

            if (appStateHandler.AppTimerState == AppState.Idle)
            {
                delay = 3000;
            }
            else
            {
                delay = dataStorageFeature.Settings.AutoDetectIdle * 60000 / 2;
            }

            IsCheckingIdleTriggered = true;

            try
            {
                await Task.Delay(delay, CancelTokenS.Token);
                CheckIdle();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"IdleCheckerFeature timer loop");
            }
            finally
            {
                IsCheckingIdleTriggered = false;
            }
        }

        private void CheckIdle()
        {
            Vector2 newpos = Vector2.Zero;

            try
            {
                newpos = lowLevelHandler.GetMousePos();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "IdleChecker failed to get mouse pos");
            }

            if (_oldmousePosition == new Vector2(-1, -1))
            {
                _oldmousePosition = newpos;
                return;
            }

            if (newpos == _oldmousePosition)
            {
                appStateHandler.SetAppState(AppState.Idle);
                Log.Information($"Mouse not moving, Old: {_oldmousePosition} New: {newpos}");
            }
            else
            {
                appStateHandler.SetAppState(AppState.Working);
                Log.Information($"Mouse Moved, Old: {_oldmousePosition} New: {newpos}");
            }

            _oldmousePosition = newpos;
        }
    }
}
