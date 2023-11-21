using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using WorkLifeBalance.HandlerClasses;

namespace WorkLifeBalance.Handlers
{
    public class MouseIdleHandler
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

        private bool IsCheckingIdleTriggered = false;

        public async void TriggerCheckIdle()
        {
            if (IsCheckingIdleTriggered) return;

            int delay;

            if(TimeHandler.Instance.AppTimmerState == AppState.Idle)
            {
                delay = 1500;
            }
            else
            {
                //delay = DataHandler.Instance.Settings.AutoDetectIdle * 1000;
                delay = 10000;
            }

            IsCheckingIdleTriggered = true;

            await Task.Delay(delay);

            CheckIdle();

            IsCheckingIdleTriggered = false;
        }

        private void CheckIdle()
        {
            Vector2 newpos = LowLevelHandler.GetMousePos();

            if(_oldmousePosition == new Vector2(-1,-1))
            {
                _oldmousePosition = newpos;
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
