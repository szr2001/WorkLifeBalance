using Serilog;
using System;
using System.IO.Pipes;
using System.Numerics;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using static Dapper.SqlMapper;

namespace WorkLifeBalance.Services.Feature
{
    public class IdleCheckerFeature : FeatureBase
    {
        private Vector2 _oldmousePosition = new(-1, -1);
        private readonly AppStateHandler appStateHandler;
        private readonly DataStorageFeature dataStorageFeature;
        private readonly LowLevelHandler lowLevelHandler;
        private readonly IFeaturesServices featuresServices;

        private readonly int MinuteMiliseconds = 60000;
        private readonly int IdleDelay = 3000;
        private readonly int RestingDelay = 600000;
        public IdleCheckerFeature(DataStorageFeature dataStorageFeature, LowLevelHandler lowLevelHandler, AppStateHandler appStateHandler, IFeaturesServices featuresServices)
        {
            this.dataStorageFeature = dataStorageFeature;
            this.lowLevelHandler = lowLevelHandler;
            this.appStateHandler = appStateHandler;
            this.featuresServices = featuresServices;
        }

        protected override Func<Task> ReturnFeatureMethod()
        {
            return TriggerCheckIdle;
        }

        private async Task TriggerCheckIdle()
        {
            if (IsFeatureRuning) return;

            try
            {
                IsFeatureRuning = true;
                int delay;

                if (appStateHandler.AppTimerState == AppState.Idle)
                {
                    delay = 3000;
                }
                else
                {
                    //delay = (dataStorageFeature.Settings.AutoDetectIdle * 60000) / 2;
                    delay = 10000;
                }

                await Task.Delay(delay, CancelTokenS.Token);
                CheckIdle();
            }
            catch (TaskCanceledException taskCancel)
            {
                Log.Information($"Idle Checker: {taskCancel.Message}");
            }
            catch(Exception ex)
            {
                Log.Error(ex,"Idle Checker");
            }
            finally
            {
                IsFeatureRuning = false;
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
                Log.Error(ex.Message);
            }

            if (_oldmousePosition == new Vector2(-1, -1))
            {
                _oldmousePosition = newpos;
                return;
            }

            if (newpos == _oldmousePosition)
            {
                featuresServices.RemoveFeature<StateCheckerFeature>();
                appStateHandler.SetAppState(AppState.Idle);
            }
            else
            {
                featuresServices.AddFeature<StateCheckerFeature>();
            }

            _oldmousePosition = newpos;
        }
    }
}
