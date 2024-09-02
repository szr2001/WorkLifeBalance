using Serilog;
using System;
using System.Numerics;
using System.Threading.Tasks;
using static WorkLifeBalance.Services.TimeHandler;

namespace WorkLifeBalance.Services.Feature
{
    public class IdleCheckerFeature : FeatureBase
    {
        private static IdleCheckerFeature? _instance;
        private Vector2 _oldmousePosition = new Vector2(-1, -1);
        public static IdleCheckerFeature Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IdleCheckerFeature();
                }
                return _instance;
            }
        }

        protected override TickEvent ReturnFeatureMethod()
        {
            return TriggerCheckIdle;
        }

        private bool IsCheckingIdleTriggered = false;
        private async void TriggerCheckIdle()
        {
            if (IsCheckingIdleTriggered) return;

            int delay;

            if (AppTimmerState == AppState.Idle)
            {
                delay = 3000;
            }
            else
            {
                delay = DataStorageFeature.Instance.Settings.AutoDetectIdle * 60000 / 2;
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
                newpos = LowLevelHandler.GetMousePos();
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
                MainWindow.instance.SetAppState(AppState.Idle);
                Log.Information($"Mouse not moving, Old: {_oldmousePosition} New: {newpos}");
            }
            else
            {
                MainWindow.instance.SetAppState(AppState.Working);
                Log.Information($"Mouse Moved, Old: {_oldmousePosition} New: {newpos}");
            }

            _oldmousePosition = newpos;
        }
    }
}
