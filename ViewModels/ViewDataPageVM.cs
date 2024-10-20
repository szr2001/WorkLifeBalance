using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Numerics;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WorkLifeBalance.ViewModels
{
    public partial class ViewDataPageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        private float recordWorkRestRatio = 0;
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
            RequiredWindowSize = new Vector2(750, 580);
            WindowPageName = "View Data";
            this.secondWindowService = secondWindowService;
            this.databaseHandler = databaseHandler;
            this.dataStorageFeature = dataStorageFeature;

            _ = CalculateData();
        }

        private async Task CalculateData() //break code up
        {
            DateOnly currentDate = dataStorageFeature.TodayData.DateC;

            DayData TempDay;

            //calculate current month records
            TempDay = await databaseHandler.GetMaxValue("WorkedAmmount");

            RecordMostWorked = TempDay.WorkedAmmountC;
            RecordMostWorkedDate = TempDay.DateC;

            TempDay = await databaseHandler.GetMaxValue("RestedAmmount");

            RecordMostRested = TempDay.RestedAmmountC;
            RecordMostRestedDate = TempDay.DateC;

            TempDay = await databaseHandler.GetMaxValue("WorkedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));

            CurrentMonthMostWorked = TempDay.WorkedAmmountC;
            CurrentMonthMostWorkedDate = TempDay.DateC;

            TempDay = await databaseHandler.GetMaxValue("RestedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));

            CurrentMonthMostRested = TempDay.RestedAmmountC;
            CurrentMonthMostRestedDate = TempDay.DateC;



            //calculate previous month records
            DateTime previousMonthDateTime = currentDate.ToDateTime(new TimeOnly(0, 0, 0)).AddMonths(-1);
            DateOnly previousDate = DateOnly.FromDateTime(previousMonthDateTime);

            TempDay = await databaseHandler.GetMaxValue("WorkedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));

            PreviousMonthMostWorked = TempDay.WorkedAmmountC;
            PreviousMonthMostWorkedDate = TempDay.DateC;

            TempDay = await databaseHandler.GetMaxValue("RestedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));

            PreviousMonthMostRested = TempDay.RestedAmmountC;
            PreviousMonthMostRestedDate = TempDay.DateC;


            //calculate total days in current and previous months
            CurrentMonthTotalDays = await databaseHandler.ReadCountInMonth(currentDate.ToString("MM"));
            PreviousMonthTotalDays = await databaseHandler.ReadCountInMonth(previousDate.ToString("MM"));

            //calculate rest/work ratio in current and previous months
            float MonthToporkedSeconds = ConvertTimeOnlyToSeconds(PreviousMonthMostWorked);

            PreviousMonthWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;
            PreviousMonthWorkRestRatio = (float)Math.Round(PreviousMonthWorkRestRatio, 2);

            MonthToporkedSeconds = ConvertTimeOnlyToSeconds(CurrentMonthMostWorked);

            CurrentMonthWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;
            CurrentMonthWorkRestRatio = (float)Math.Round(CurrentMonthWorkRestRatio, 2);

            MonthToporkedSeconds = ConvertTimeOnlyToSeconds(RecordMostWorked);

            RecordWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;
            RecordWorkRestRatio = (float)Math.Round(RecordWorkRestRatio, 2);
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

        private int ConvertTimeOnlyToSeconds(TimeOnly time)
        {
            int seconds = 0;
            seconds += time.Second;
            seconds += time.Minute * 60;
            seconds += (time.Hour * 60) * 60;

            return seconds;
        }
    }
}
