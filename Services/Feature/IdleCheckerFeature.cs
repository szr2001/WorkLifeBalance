using Serilog;
using System;
using System.IO.Pipes;
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

        private readonly int MinuteMiliseconds = 60000;
        private readonly int IdleDelay = 3000;
        private readonly int RestingDelay = 600000;
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

            switch (appStateHandler.AppTimerState)
            {
                case AppState.Working:
                    //delay = dataStorageFeature.Settings.AutoDetectIdle * MinuteMiliseconds / 2;
                    delay = 5000;
                    break;
                case AppState.Resting:
                    delay = 5000;
                    break;
                case AppState.Idle:
                    delay = 3000;
                    break;
            }

            IsCheckingIdleTriggered = true;

            Console.WriteLine(delay);

            await Task.Delay(delay, CancelTokenS.Token);
            CheckIdle();
         
            IsCheckingIdleTriggered = false;
        }

        private void CheckIdle()
        {
            Vector2 newpos = Vector2.Zero;

            newpos = lowLevelHandler.GetMousePos();

            if (_oldmousePosition == new Vector2(-1, -1))
            {
                _oldmousePosition = newpos;
                return;
            }

            if (newpos == _oldmousePosition)
            {
                appStateHandler.SetAppState(AppState.Idle);
            }
            else
            {
                appStateHandler.SetAppState(AppState.Working);
            }

            _oldmousePosition = newpos;
        }
    }
}
