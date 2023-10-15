using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkLifeBalance.Handlers
{
    public class TimeHandler
    {
        private static TimeHandler? _instance;
        public static TimeHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimeHandler();
                }
                return _instance;
            }
        }

        public delegate void TickEvent();

        public event TickEvent? OnTimerTick;

        private CancellationTokenSource CancelTick = new();

        public void StartTick()
        {
            CancelTick.Cancel();

            CancelTick = new();

            _ = TimerLoop(CancelTick.Token);
        }

        public void Stop()
        {
            CancelTick.Cancel();
        }

        private async Task TimerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (DataHandler.Instance.IsAppReady && !DataHandler.Instance.IsClosingApp) 
                {
                    await Task.Delay(1000);

                    OnTimerTick?.Invoke();
                }
            }
        }
    }
}
