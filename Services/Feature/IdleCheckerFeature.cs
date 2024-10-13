using Serilog;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace WorkLifeBalance.Services.Feature
{
    public class IdleCheckerFeature : FeatureBase
    {
        private static IdleCheckerFeature? _instance;
        private Vector2 _oldmousePosition = new Vector2(-1, -1);
        private AppTimer appTimer;
        private DataStorageFeature dataStorageFeature;
        private LowLevelHandler lowLevelHandler;
        public IdleCheckerFeature(AppTimer appTimer, DataStorageFeature dataStorageFeature, LowLevelHandler lowLevelHandler)
        {
            this.appTimer = appTimer;
            this.dataStorageFeature = dataStorageFeature;
            this.lowLevelHandler = lowLevelHandler;
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

            if (appTimer.AppTimerState == AppState.Idle)
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
                appTimer.SetAppState(AppState.Idle);
                Log.Information($"Mouse not moving, Old: {_oldmousePosition} New: {newpos}");
            }
            else
            {
                appTimer.SetAppState(AppState.Working);
                Log.Information($"Mouse Moved, Old: {_oldmousePosition} New: {newpos}");
            }

            _oldmousePosition = newpos;
        }
    }
}
