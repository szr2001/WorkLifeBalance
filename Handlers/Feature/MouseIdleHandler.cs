using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using WorkLifeBalance.Handlers;
using static WorkLifeBalance.Handlers.TimeHandler;

namespace WorkLifeBalance.Handlers.Feature
{
    public class MouseIdleHandler : FeatureBase
    {
        private static MouseIdleHandler? _instance;
        private Vector2 _oldmousePosition = new Vector2(-1,-1);
        public static MouseIdleHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MouseIdleHandler();
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

            if(TimeHandler.Instance.AppTimmerState == AppState.Idle)
            {
                delay = 1500;
            }
            else
            {
                delay = (DataHandler.Instance.Settings.AutoDetectIdle * 60000)/2;
            }

            IsCheckingIdleTriggered = true;

            try
            {
                await Task.Delay(delay, CancelTokenS.Token);
                CheckIdle();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsCheckingIdleTriggered = false;
            }
        }

        private void CheckIdle()
        {
            Vector2 newpos = LowLevelHandler.GetMousePos();

            if(_oldmousePosition == new Vector2(-1,-1))
            {
                _oldmousePosition = newpos;
                return;
            }

            Console.WriteLine($"Old: {_oldmousePosition} New: {newpos}");

            if(newpos == _oldmousePosition)
            {
                MainWindow.instance.SetAppState(AppState.Idle);
                Console.WriteLine("Mouse not moving");
            }
            else
            {
                MainWindow.instance.SetAppState(AppState.Working);
                Console.WriteLine("Mouse Moved");
            }

            _oldmousePosition = newpos;
        }
    }
}
