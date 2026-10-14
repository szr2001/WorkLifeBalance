using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkLifeBalance.Shared.Navigation;
using WorkLifeBalance.Shared.Scheduling;

namespace WorkLifeBalance.Features.ForceWork
{
    public partial class ForceWorkViewModel : SecondWindowPageBase
    {
        private readonly ForceWorkFeature _forceWorkFeature;
        private readonly IWindowService<SecondWindowPageBase> _secondWindowService;
        private readonly IFeaturesService _featuresService;

        [ObservableProperty]
        private int _totalWorkHours = 2;

        [ObservableProperty]
        private int _totalWorkMinutes;

        [ObservableProperty]
        private int _workHours;

        [ObservableProperty]
        private int _workMinutes = 25;

        [ObservableProperty]
        private int _restHours;

        [ObservableProperty]
        private int _restMinutes = 5;

        [ObservableProperty]
        private int _longRestHours;

        [ObservableProperty]
        private int _longRestMinutes = 25;

        [ObservableProperty]
        private int _longRestInterval = 4;

        [ObservableProperty]
        private bool _isFeatureActive;

        [ObservableProperty]
        private int _maxWarnings = 3;

        [ObservableProperty]
        private TimeOnly _totalWorkTimeSetting;

        [ObservableProperty]
        private TimeOnly _workTimeSetting;

        [ObservableProperty]
        private TimeOnly _restTimeSetting;

        [ObservableProperty]
        private TimeOnly _longRestTimeSetting;

        [ObservableProperty]
        private int _longRestIntervalSetting;

        [ObservableProperty]
        private int _distractionCount;

        [ObservableProperty]
        private string[] _distractions = ["Process.exe", "Process.exe", "Process.exe"];

        public ForceWorkViewModel(ForceWorkFeature forceWorkFeature, IWindowService<SecondWindowPageBase> secondWindowService, IFeaturesService featuresService)
        {
            _forceWorkFeature = forceWorkFeature;
            _secondWindowService = secondWindowService;
            _featuresService = featuresService;
            PageWidth = 380;
            PageHeight = 330;
            PageName = "Force Work";
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            IsFeatureActive = _featuresService.IsFeaturePresent<ForceWorkFeature>();
            _forceWorkFeature.OnDataUpdated += UpdateDataFromForceWork;
            return Task.CompletedTask;
        }

        public override Task OnPageClosingAsync()
        {
            _forceWorkFeature.OnDataUpdated -= UpdateDataFromForceWork;
            return Task.CompletedTask;
        }

        private void UpdateDataFromForceWork()
        {
            Distractions = _forceWorkFeature.Distractions;
            DistractionCount = _forceWorkFeature.DistractionsCount;
            IsFeatureActive = _featuresService.IsFeaturePresent<ForceWorkFeature>();
        }

        private void GetForceWorkSettings()
        {
            TotalWorkTimeSetting = _forceWorkFeature.TotalWorkTimeSetting;
            WorkTimeSetting = _forceWorkFeature.WorkTimeSetting;
            RestTimeSetting = _forceWorkFeature.RestTimeSetting;
            LongRestTimeSetting = _forceWorkFeature.LongRestTimeSetting;
            LongRestIntervalSetting = _forceWorkFeature.LongRestIntervalSetting;
        }

        [RelayCommand]
        private void ToggleForceWork()
        {
            if (IsFeatureActive)
            {
                _featuresService.RemoveFeature<ForceWorkFeature>();
                IsFeatureActive = false;
                return;
            }

            _forceWorkFeature.SetWorkTime(WorkHours, WorkMinutes, MaxWarnings);
            _forceWorkFeature.SetRestTime(RestHours, RestMinutes);
            _forceWorkFeature.SetTotalWorkTime(TotalWorkHours, TotalWorkMinutes);
            _forceWorkFeature.SetLongRestTime(LongRestHours, LongRestMinutes, LongRestInterval);
            GetForceWorkSettings();
            _featuresService.AddFeature<ForceWorkFeature>();
            IsFeatureActive = true;
        }
    }
}
