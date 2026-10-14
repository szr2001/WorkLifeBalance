using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Features.Statistics
{
    public partial class StatisticsViewModel : SecondWindowPageBase
    {
        private const float SecondsInADay = 86400;

        private readonly StatisticsStore _store;
        private readonly DataStorageFeature _dataStorage;
        private readonly IWindowService<SecondWindowPageBase> _secondWindowService;

        [ObservableProperty]
        private TimeOnly _recordMostWorked;

        [ObservableProperty]
        private DateOnly _recordMostWorkedDate;

        [ObservableProperty]
        private TimeOnly _recordMostRested;

        [ObservableProperty]
        private DateOnly _recordMostRestedDate;

        [ObservableProperty]
        private float _currentMonthWorkRestRatio;

        [ObservableProperty]
        private int _currentMonthTotalDays;

        [ObservableProperty]
        private TimeOnly _currentMonthMostWorked;

        [ObservableProperty]
        private TimeOnly _currentMonthAverageWorked;

        [ObservableProperty]
        private DateOnly _currentMonthMostWorkedDate;

        [ObservableProperty]
        private TimeOnly _currentMonthMostRested;

        [ObservableProperty]
        private DateOnly _currentMonthMostRestedDate;

        [ObservableProperty]
        private float _previousMonthWorkRestRatio;

        [ObservableProperty]
        private int _previousMonthTotalDays;

        [ObservableProperty]
        private TimeOnly _previousMonthMostWorked;

        [ObservableProperty]
        private TimeOnly _previousMonthAverageWorked;

        [ObservableProperty]
        private DateOnly _previousMonthMostWorkedDate;

        [ObservableProperty]
        private TimeOnly _previousMonthMostRested;

        [ObservableProperty]
        private DateOnly _previousMonthMostRestedDate;

        public StatisticsViewModel(StatisticsStore store, DataStorageFeature dataStorage, IWindowService<SecondWindowPageBase> secondWindowService)
        {
            _store = store;
            _dataStorage = dataStorage;
            _secondWindowService = secondWindowService;
            PageWidth = 640;
            PageHeight = 440;
            PageName = "View Data";
            _ = CalculateData();
        }

        public override async Task OnPageOpeningAsync(object? args = null) => await CalculateData();

        private async Task CalculateData()
        {
            var currentDate = _dataStorage.TodayData.DateC;
            var previousDate = currentDate.AddMonths(-1);

            await CalculateCurrentMonth(currentDate);
            await CalculatePreviousMonth(previousDate);
            await CalculateRecord();
            CalculateWorkRatios();
        }

        private async Task CalculateRecord()
        {
            var day = await _store.GetMaxValue("WorkedAmmount");
            RecordMostWorked = day.WorkedAmmountC;
            RecordMostWorkedDate = day.DateC;

            day = await _store.GetMaxValue("RestedAmmount");
            RecordMostRested = day.RestedAmmountC;
            RecordMostRestedDate = day.DateC;
        }

        private async Task CalculateCurrentMonth(DateOnly date)
        {
            try
            {
                var (month, year) = (date.ToString("MM"), date.ToString("yyyy"));

                CurrentMonthAverageWorked = ConvertSecondsToTime(await _store.GetAvgSecondsTimeOnly("WorkedAmmount", month));

                var day = await _store.GetMaxValue("WorkedAmmount", month, year);
                CurrentMonthMostWorked = day.WorkedAmmountC;
                CurrentMonthMostWorkedDate = day.DateC;

                day = await _store.GetMaxValue("RestedAmmount", month, year);
                CurrentMonthMostRested = day.RestedAmmountC;
                CurrentMonthMostRestedDate = day.DateC;

                CurrentMonthTotalDays = await _store.ReadCountInMonth(month, year);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to calculate the current month");
            }
        }

        private async Task CalculatePreviousMonth(DateOnly date)
        {
            var (month, year) = (date.ToString("MM"), date.ToString("yyyy"));

            PreviousMonthAverageWorked = ConvertSecondsToTime(await _store.GetAvgSecondsTimeOnly("WorkedAmmount", month));

            var day = await _store.GetMaxValue("WorkedAmmount", month, year);
            PreviousMonthMostWorked = day.WorkedAmmountC;
            PreviousMonthMostWorkedDate = day.DateC;

            day = await _store.GetMaxValue("RestedAmmount", month, year);
            PreviousMonthMostRested = day.RestedAmmountC;
            PreviousMonthMostRestedDate = day.DateC;

            PreviousMonthTotalDays = await _store.ReadCountInMonth(month, year);
        }

        // Average work divided by 24 hours
        private void CalculateWorkRatios()
        {
            PreviousMonthWorkRestRatio = (float)Math.Round(ConvertTimeToSeconds(PreviousMonthAverageWorked) / SecondsInADay, 2);
            CurrentMonthWorkRestRatio = (float)Math.Round(ConvertTimeToSeconds(CurrentMonthAverageWorked) / SecondsInADay, 2);
        }

        private static TimeOnly ConvertSecondsToTime(int seconds) => new(seconds / 3600, seconds / 60 % 60, seconds % 60);

        private static int ConvertTimeToSeconds(TimeOnly time) => time.Hour * 3600 + time.Minute * 60 + time.Second;

        [RelayCommand]
        private void SeePreviousMonth() => _secondWindowService.OpenWith<DaysViewModel>(DaysRange.PreviousMonth);

        [RelayCommand]
        private void SeeCurrentMonth() => _secondWindowService.OpenWith<DaysViewModel>(DaysRange.CurrentMonth);

        [RelayCommand]
        private void SeeAllDays() => _secondWindowService.OpenWith<DaysViewModel>(DaysRange.All);
    }
}
