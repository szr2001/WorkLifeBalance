using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    public partial class ViewDataPageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        private TimeOnly recordMostWorked;
        [ObservableProperty]
        private DateOnly recordMostWorkedDate;

        [ObservableProperty]
        private TimeOnly recordMostRested;
        [ObservableProperty]
        private DateOnly recordMostRestedDate;

        [ObservableProperty]
        private float currentMonthWorkRestRatio = 0;
        [ObservableProperty]
        private int currentMonthTotalDays = 0;
        [ObservableProperty]
        private TimeOnly currentMonthMostWorked;
        [ObservableProperty]
        private TimeOnly currentMonthAverageWorked;
        [ObservableProperty]
        private DateOnly currentMonthMostWorkedDate;

        [ObservableProperty]
        private TimeOnly currentMonthMostRested;
        [ObservableProperty]
        private DateOnly currentMonthMostRestedDate;

        [ObservableProperty]
        private float previousMonthWorkRestRatio = 0;
        [ObservableProperty]
        private int previousMonthTotalDays = 0;
        [ObservableProperty]
        private TimeOnly previousMonthMostWorked;
        [ObservableProperty]
        private TimeOnly previousMonthAverageWorked;
        [ObservableProperty]
        private DateOnly previousMonthMostWorkedDate;

        [ObservableProperty]
        private TimeOnly previousMonthMostRested;
        [ObservableProperty]
        private DateOnly previousMonthMostRestedDate;

        private DataBaseHandler databaseHandler;
        private DataStorageFeature dataStorageFeature;
        private IWindowService<SecondWindowPageVMBase> secondWindowService;
        public ViewDataPageVM(DataBaseHandler databaseHandler, DataStorageFeature dataStorageFeature, IWindowService<SecondWindowPageVMBase> secondWindowService)
        {
            PageHeight = 580;
            PageWidth = 750;
            PageName = "View Data";
            this.secondWindowService = secondWindowService;
            this.databaseHandler = databaseHandler;
            this.dataStorageFeature = dataStorageFeature;

            _ = CalculateData();
        }

        public override Task OnPageClosingAsync() => Task.CompletedTask;

        public override async Task OnPageOpeningAsync(object? args = null)
        {
            await CalculateData();
        }

        private async Task CalculateData()
        {
            DateOnly currentDate = dataStorageFeature.TodayData.DateC;
            DateOnly previousMonthDateTime = currentDate.AddMonths(-1);

            await CalculateCurrentMonth(currentDate);
            await CalculatePreviousMonth(previousMonthDateTime);
            await CalculateRecord();
            CalculateWorkRatios(currentDate, previousMonthDateTime);
        }

        private async Task CalculateRecord()
        {
            DayData TempDay;

            TempDay = await databaseHandler.GetMaxValue("WorkedAmmount");

            RecordMostWorked = TempDay.WorkedAmmountC;
            RecordMostWorkedDate = TempDay.DateC;

            TempDay = await databaseHandler.GetMaxValue("RestedAmmount");

            RecordMostRested = TempDay.RestedAmmountC;
            RecordMostRestedDate = TempDay.DateC;
        }
        private async Task CalculateCurrentMonth(DateOnly currentDate)
        {
            try
            {

                DayData TempDay;

                int MonthToporkedSeconds = await databaseHandler.GetAvgSecondsTimeOnly("WorkedAmmount", currentDate.ToString("MM"));
                CurrentMonthAverageWorked = ConvertSecondsToTime(MonthToporkedSeconds);

                TempDay = await databaseHandler.GetMaxValue("WorkedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));
                CurrentMonthMostWorked = TempDay.WorkedAmmountC;
                CurrentMonthMostWorkedDate = TempDay.DateC;

                TempDay = await databaseHandler.GetMaxValue("RestedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));

                CurrentMonthMostRested = TempDay.RestedAmmountC;
                CurrentMonthMostRestedDate = TempDay.DateC;
                CurrentMonthTotalDays = await databaseHandler.ReadCountInMonth
                    (
                        currentDate.ToString("MM"), 
                        currentDate.ToString("yyyy")
                    );
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task CalculatePreviousMonth(DateOnly previousDate)
        {
            DayData TempDay;

            int MonthToporkedSeconds = await databaseHandler.GetAvgSecondsTimeOnly("WorkedAmmount", previousDate.ToString("MM"));
            PreviousMonthAverageWorked = ConvertSecondsToTime(MonthToporkedSeconds);

            TempDay = await databaseHandler.GetMaxValue("WorkedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));
            PreviousMonthMostWorked = TempDay.WorkedAmmountC;
            PreviousMonthMostWorkedDate = TempDay.DateC;

            TempDay = await databaseHandler.GetMaxValue("RestedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));

            PreviousMonthMostRested = TempDay.RestedAmmountC;
            PreviousMonthMostRestedDate = TempDay.DateC;
            PreviousMonthTotalDays = await databaseHandler.ReadCountInMonth
                    (
                        previousDate.ToString("MM"),
                        previousDate.ToString("yyyy")
                    );
        }

        private void CalculateWorkRatios(DateOnly currentDate, DateOnly previousDate)
        {
            float MonthToporkedSeconds = ConvertTimeToSeconds(PreviousMonthAverageWorked);

            PreviousMonthWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;
            PreviousMonthWorkRestRatio = (float)Math.Round(PreviousMonthWorkRestRatio, 2);

            MonthToporkedSeconds = ConvertTimeToSeconds(CurrentMonthAverageWorked);

            CurrentMonthWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;
            CurrentMonthWorkRestRatio = (float)Math.Round(CurrentMonthWorkRestRatio, 2);
        }

        private TimeOnly ConvertSecondsToTime(int seconds)
        {
            int minutes = 0;
            int hours = 0;
            if (seconds != 0)
            {
                minutes = seconds / 60;
                seconds = seconds % 60;

                hours = minutes == 0 ? 0 : minutes / 60;
                minutes = minutes % 60;
            }
            return new TimeOnly(hours,minutes,seconds);
        }

        private int ConvertTimeToSeconds(TimeOnly time)
        {
            int seconds = 0;

            seconds += time.Second;
            seconds += time.Minute * 60;
            seconds += time.Hour * 60 * 60;

            return seconds;
        }

        [RelayCommand]
        private void SeePreviousMonth()
        {
            secondWindowService.OpenWith<ViewDaysPageVM>(2);
        }

        [RelayCommand]
        private void SeeCurrentMonth()
        {
            secondWindowService.OpenWith<ViewDaysPageVM>(1);
        }

        [RelayCommand]
        private void SeeAllDays()
        {
            secondWindowService.OpenWith<ViewDaysPageVM>(0);
        }
    }
}
