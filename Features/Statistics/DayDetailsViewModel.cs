using CommunityToolkit.Mvvm.ComponentModel;
using WorkLifeBalance.Shared.Data;
using WorkLifeBalance.Shared.Navigation;

namespace WorkLifeBalance.Features.Statistics
{
    public partial class DayDetailsViewModel : SecondWindowPageBase
    {
        private readonly StatisticsStore _store;

        [ObservableProperty]
        private ProcessActivityData[]? _processActivities;

        [ObservableProperty]
        private PageActivityData[]? _pageActivities;

        [ObservableProperty]
        private DayData? _loadedDayData;

        public DayDetailsViewModel(StatisticsStore store)
        {
            _store = store;
            PageWidth = 640;
            PageHeight = 450;
            PageName = "View Day Details";
        }

        public override Task OnPageOpeningAsync(object? args = null)
        {
            if (args is DayData day)
            {
                LoadedDayData = day;
                PageName = $"{day.DateC:MM/dd/yyyy} Activity";
                _ = RequestData(day);
            }

            return Task.CompletedTask;
        }

        private async Task RequestData(DayData day)
        {
            var processes = await _store.ReadProcessDayActivity(day.Date);
            var pages = await _store.ReadUrlDayActivity(day.Date);
            ProcessActivities = processes.OrderByDescending(activity => activity.TimeSpentC).ToArray();
            PageActivities = pages.OrderByDescending(activity => activity.TimeSpentC).ToArray();
        }
    }
}
