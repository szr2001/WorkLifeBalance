using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for ViewDataPage.xaml
    /// </summary>
    public partial class ViewDataPage : Page
    {
        float RecordWorkRestRatio = 0;
        TimeOnly RecordMostWorked;
        DateOnly RecordMostWorkedDate;

        TimeOnly RecordMostRested;
        DateOnly RecordMostRestedDate;

        float CurrentMonthWorkRestRatio = 0;
        int CurrentMonthTotalDays = 0;
        TimeOnly CurrentMonthMostWorked;
        DateOnly CurrentMonthMostWorkedDate;

        TimeOnly CurrentMonthMostRested;
        DateOnly CurrentMonthMostRestedDate;

        float PreviousMonthWorkRestRatio = 0;
        int PreviousMonthTotalDays = 0;
        TimeOnly PreviousMonthMostWorked;
        DateOnly PreviousMonthMostWorkedDate;

        TimeOnly PreviousMonthMostRested;
        DateOnly PreviousMonthMostRestedDate;

        private ViewDataPageVM viewDataPageVM;
        public ViewDataPage(ViewDataPageVM viewDataPageVM)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(750, 580);
            pageNme = "View Data";
            _ = CalculateData();
            this.viewDataPageVM = viewDataPageVM;
        }

        private async Task CalculateData()
        {
            //get today date
            DateOnly currentDate = DataStorageFeature.Instance.TodayData.DateC;

            //create temporary daydata to load each day and reuse it
            DayData TempDay;


            //calculate current month records

            TempDay = await DataBaseHandler.GetMaxValue("WorkedAmmount");

            RecordMostWorked = TempDay.WorkedAmmountC;
            RecordMostWorkedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("RestedAmmount");

            RecordMostRested = TempDay.RestedAmmountC;
            RecordMostRestedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("WorkedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));

            CurrentMonthMostWorked = TempDay.WorkedAmmountC;
            CurrentMonthMostWorkedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("RestedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));

            CurrentMonthMostRested = TempDay.RestedAmmountC;
            CurrentMonthMostRestedDate = TempDay.DateC;

            //calculate previous month records

            DateTime previousMonthDateTime = currentDate.ToDateTime(new TimeOnly(0, 0, 0)).AddMonths(-1);
            DateOnly previousDate = DateOnly.FromDateTime(previousMonthDateTime);

            TempDay = await DataBaseHandler.GetMaxValue("WorkedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));

            PreviousMonthMostWorked = TempDay.WorkedAmmountC;
            PreviousMonthMostWorkedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("RestedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));

            PreviousMonthMostRested = TempDay.RestedAmmountC;
            PreviousMonthMostRestedDate = TempDay.DateC;


            //calculate total days in current and previous months
            CurrentMonthTotalDays = await DataBaseHandler.ReadCountInMonth(currentDate.ToString("MM"));
            PreviousMonthTotalDays = await DataBaseHandler.ReadCountInMonth(previousDate.ToString("MM"));

            //calculate rest/work ratio in current and previous months
            float MonthToporkedSeconds = ConvertTimeOnlyToSeconds(PreviousMonthMostWorked);

            PreviousMonthWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;

            MonthToporkedSeconds = ConvertTimeOnlyToSeconds(CurrentMonthMostWorked);

            CurrentMonthWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;

            MonthToporkedSeconds = ConvertTimeOnlyToSeconds(RecordMostWorked);

            RecordWorkRestRatio = MonthToporkedSeconds == 0 ? 0 : MonthToporkedSeconds / 86400;
            UpdateUi();
        }
        private void UpdateUi()
        {
            PRMWAmount.Text = RecordMostWorked.ToString("HH:mm:ss");
            PRMWDate.Text = RecordMostWorkedDate.ToString("MM/dd/yyy");

            PRMRAmount.Text = RecordMostRested.ToString("HH:mm:ss");
            PRMRDate.Text = RecordMostRestedDate.ToString("MM/dd/yyy");

            PMMWAmount.Text = PreviousMonthMostWorked.ToString("HH:mm:ss");
            PMMWDate.Text = PreviousMonthMostWorkedDate.ToString("MM/dd/yyy");

            PMMRAmount.Text = PreviousMonthMostRested.ToString("HH:mm:ss");
            PMMRDate.Text = PreviousMonthMostRestedDate.ToString("MM/dd/yyy");

            CMMWAmount.Text = CurrentMonthMostWorked.ToString("HH:mm:ss");
            CMMWDate.Text = CurrentMonthMostWorkedDate.ToString("MM/dd/yyy");

            CMMRAmount.Text = CurrentMonthMostRested.ToString("HH:mm:ss");
            CMMRDate.Text = CurrentMonthMostRestedDate.ToString("MM/dd/yyy");

            PRWRRatio.Text = RecordWorkRestRatio.ToString("0.00");

            PMMRTRatio.Text = PreviousMonthWorkRestRatio.ToString("0.00");

            PMRDays.Text = PreviousMonthTotalDays.ToString();

            CMMRTRatio.Text = CurrentMonthWorkRestRatio.ToString("0.00");

            CMRDays.Text = CurrentMonthTotalDays.ToString();
        }

        private void SeePreviousMonth(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.ViewDays, 2);
        }

        private void SeeCurrentMonth(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.ViewDays, 1);
        }

        private void SeeAllDays(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.ViewDays, 0);
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
