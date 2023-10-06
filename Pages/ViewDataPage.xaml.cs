using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkLifeBalance.Data;
using WorkLifeBalance.HandlerClasses;
using WorkLifeBalance.Windows;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for ViewDataPage.xaml
    /// </summary>
    public partial class ViewDataPage : SecondWindowPageBase
    {
        int RecordWorkRestRatio = 0;
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
        public ViewDataPage(SecondWindow secondwindow, object? args) : base(secondwindow, args)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(750, 580);
            pageNme = "View Data";
            _ = CalculateData();
        }

        private async Task CalculateData()
        {
            //get today date
            DateOnly currentDate = ParentWindow.MainWindowParent.TodayData.DateC;

            //create temporary daydata to load each day and reuse it
            DayData TempDay;


            //calculate current month records

            TempDay = await DataBaseHandler.GetMaxValue("WorkedAmmount");
            TempDay.ConvertSaveDataToUsableData();

            RecordMostWorked = TempDay.WorkedAmmountC;
            RecordMostWorkedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("RestedAmmount");
            TempDay.ConvertSaveDataToUsableData();

            RecordMostRested = TempDay.RestedAmmountC;
            RecordMostRestedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("WorkedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));
            TempDay.ConvertSaveDataToUsableData();

            CurrentMonthMostWorked = TempDay.WorkedAmmountC;
            CurrentMonthMostWorkedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("RestedAmmount", currentDate.ToString("MM"), currentDate.ToString("yyyy"));
            TempDay.ConvertSaveDataToUsableData();

            CurrentMonthMostRested= TempDay.RestedAmmountC;
            CurrentMonthMostRestedDate = TempDay.DateC;

            //calculate previous month records

            DateTime previousMonthDateTime = currentDate.ToDateTime(new TimeOnly(0,0,0)).AddMonths(-1);
            DateOnly previousDate = DateOnly.FromDateTime(previousMonthDateTime);

            TempDay = await DataBaseHandler.GetMaxValue("WorkedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));
            TempDay.ConvertSaveDataToUsableData();

            PreviousMonthMostWorked = TempDay.WorkedAmmountC;
            PreviousMonthMostWorkedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("RestedAmmount", previousDate.ToString("MM"), previousDate.ToString("yyyy"));
            TempDay.ConvertSaveDataToUsableData();

            PreviousMonthMostRested= TempDay.RestedAmmountC;
            PreviousMonthMostRestedDate = TempDay.DateC;


            //calculate total days in current and previous months
            CurrentMonthTotalDays = await DataBaseHandler.ReadCountInMonth(currentDate.ToString("MM"));
            PreviousMonthTotalDays = await DataBaseHandler.ReadCountInMonth(previousDate.ToString("MM"));

            //calculate rest/work ratio in current and previous months
            float MonthTotalWorkedSeconds = 0;
            float MonthTotalRestedSeconds = 0;

            MonthTotalWorkedSeconds += PreviousMonthMostWorked.Second;
            MonthTotalWorkedSeconds += PreviousMonthMostWorked.Minute * 60;
            MonthTotalWorkedSeconds += (PreviousMonthMostWorked.Hour * 60) * 60;

            MonthTotalRestedSeconds += PreviousMonthMostRested.Second;
            MonthTotalRestedSeconds += PreviousMonthMostRested.Minute * 60;
            MonthTotalRestedSeconds += (PreviousMonthMostRested.Hour * 60) * 60;

            PreviousMonthWorkRestRatio = MonthTotalRestedSeconds / MonthTotalWorkedSeconds;

            MonthTotalWorkedSeconds = 0;
            MonthTotalRestedSeconds = 0;
            MonthTotalWorkedSeconds += CurrentMonthMostWorked.Second;
            MonthTotalWorkedSeconds += CurrentMonthMostWorked.Minute * 60;
            MonthTotalWorkedSeconds += (CurrentMonthMostWorked.Hour * 60) * 60;

            MonthTotalRestedSeconds += CurrentMonthMostRested.Second;
            MonthTotalRestedSeconds += CurrentMonthMostRested.Minute * 60;
            MonthTotalRestedSeconds += (CurrentMonthMostRested.Hour * 60) * 60;

            CurrentMonthWorkRestRatio = MonthTotalRestedSeconds / MonthTotalWorkedSeconds;

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

            PRWRRatio.Text = RecordWorkRestRatio.ToString();

            PMMRTRatio.Text = PreviousMonthWorkRestRatio.ToString("0.00");

            PMRDays.Text = PreviousMonthTotalDays.ToString();

            CMMRTRatio.Text = CurrentMonthWorkRestRatio.ToString("0.00");

            CMRDays.Text = CurrentMonthTotalDays.ToString();
        }

        private void SeePreviousMonth(object sender, RoutedEventArgs e)
        {
            ParentWindow.MainWindowParent.OpenSecondWindow(SecondWindowType.ViewDays,0);
        }

        private void SeeCurrentMonth(object sender, RoutedEventArgs e)
        {
            ParentWindow.MainWindowParent.OpenSecondWindow(SecondWindowType.ViewDays, 1);
        }

        private void SeeAllDays(object sender, RoutedEventArgs e)
        {
            ParentWindow.MainWindowParent.OpenSecondWindow(SecondWindowType.ViewDays, 2);
        }
    }
}
