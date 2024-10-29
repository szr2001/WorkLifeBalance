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
        private DateOnly previousMonthMostWorkedDate;

        [ObservableProperty]
        private TimeOnly previousMonthMostRested;
        [ObservableProperty]
        private DateOnly previousMonthMostRestedDate;

        private DataBaseHandler databaseHandler;
        private DataStorageFeature dataStorageFeature;
        private ISecondWindowService secondWindowService;
        public ViewDataPageVM(DataBaseHandler databaseHandler, DataStorageFeature dataStorageFeature, ISecondWindowService secondWindowService)
        {
            PageHeight = 580;
            PageWidth = 750;
            PageName = "View Data";
            this.secondWindowService = secondWindowService;
            this.databaseHandler = databaseHandler;
            this.dataStorageFeature = dataStorageFeature;

            _ = CalculateData();
        }

        private async Task CalculateData()
        {
            DateOnly currentDate = dataStorageFeature.TodayData.DateC;
            DateOnly previousMonthDateTime = currentDate.AddMonths(-1);

            await CalculateCurrentMonth(currentDate);
            await CalculatePreviousMonth(previousMonthDateTime);
            await CalculateRecord();
            await CalculateWorkRatios(currentDate, previousMonthDateTime);
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
            DayData TempDay;

            TempDay = await databaseHandler.GetMaxValue("WorkedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));

            CurrentMonthMostWorked = TempDay.WorkedAmmountC;
            CurrentMonthMostWorkedDate = TempDay.DateC;

            TempDay = await databaseHandler.GetMaxValue("RestedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));

            CurrentMonthMostRested = TempDay.RestedAmmountC;
            CurrentMonthMostRestedDate = TempDay.DateC;
            CurrentMonthTotalDays = await databaseHandler.ReadCountInMonth(currentDate.ToString("MM"));
        }

        private async Task CalculatePreviousMonth(DateOnly previousDate)
        {
            DayData TempDay;

            TempDay = await databaseHandler.GetMaxValue("WorkedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));

            PreviousMonthMostWorked = TempDay.WorkedAmmountC;
            PreviousMonthMostWorkedDate = TempDay.DateC;

            TempDay = await databaseHandler.GetMaxValue("RestedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));

            PreviousMonthMostRested = TempDay.RestedAmmountC;
            PreviousMonthMostRestedDate = TempDay.DateC;
            PreviousMonthTotalDays = await databaseHandler.ReadCountInMonth(previousDate.ToString("MM"));
        }

        private async Task CalculateWorkRatios(DateOnly currentDate, DateOnly previousDate)
        {
            float MonthToporkedSeconds = await databaseHandler.GetAvgSecondsTimeOnly("WorkedAmmount", previousDate.ToString("MM"));

            PreviousMonthWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;
            PreviousMonthWorkRestRatio = (float)Math.Round(PreviousMonthWorkRestRatio, 2);

            MonthToporkedSeconds = await databaseHandler.GetAvgSecondsTimeOnly("WorkedAmmount", currentDate.ToString("MM"));

            CurrentMonthWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;
            CurrentMonthWorkRestRatio = (float)Math.Round(CurrentMonthWorkRestRatio, 2);
        }

        [RelayCommand]
        private void SeePreviousMonth()
        {
            secondWindowService.OpenWindowWith<ViewDaysPageVM>(2);
        }

        [RelayCommand]
        private void SeeCurrentMonth()
        {
            secondWindowService.OpenWindowWith<ViewDaysPageVM>(1);
        }

        [RelayCommand]
        private void SeeAllDays()
        {
            secondWindowService.OpenWindowWith<ViewDaysPageVM>(0);
        }
    }
}
