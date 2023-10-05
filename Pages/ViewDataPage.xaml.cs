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

        int CurrentMonthWorkRestRatio = 0;
        int CurrentMonthTotalDays = 0;
        TimeOnly CurrentMonthMostWorked;
        DateOnly CurrentMonthMostWorkedDate;

        TimeOnly CurrentMonthMostRested;
        DateOnly CurrentMonthMostRestedDate;

        int PreviousMonthWorkRestRatio = 0;
        int PreviousMonthTotalDays = 0;
        TimeOnly PreviousMonthMostWorked;
        DateOnly PreviousMonthMostWorkedDate;

        TimeOnly PreviousMonthMostRested;
        DateOnly PreviousMonthMostRestedDate;
        public ViewDataPage(SecondWindow secondwindow) : base(secondwindow)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(750, 580);
            pageNme = "View Data";
            _ = CalculateData();
        }

        private async Task CalculateData()
        {
            //using int will not allow numbers like 09,use strings
            int CurrentMonth = ParentWindow.MainWindowParent.TodayData.DateC.Month;
            int CurrentYear = ParentWindow.MainWindowParent.TodayData.DateC.Year;

            DayData TempDay;

            TempDay = await DataBaseHandler.GetMaxValue("WorkedAmmount");
            TempDay.ConvertSaveDataToUsableData();

            RecordMostWorked = TempDay.WorkedAmmountC;
            RecordMostWorkedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("RestedAmmount");
            TempDay.ConvertSaveDataToUsableData();

            RecordMostRested = TempDay.RestedAmmountC;
            RecordMostRestedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("WorkedAmmount", CurrentMonth, CurrentYear);
            TempDay.ConvertSaveDataToUsableData();

            CurrentMonthMostWorked = TempDay.WorkedAmmountC;
            CurrentMonthMostWorkedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("RestedAmmount", CurrentMonth, CurrentYear);
            TempDay.ConvertSaveDataToUsableData();

            CurrentMonthMostRested= TempDay.RestedAmmountC;
            CurrentMonthMostRestedDate = TempDay.DateC;

            int PreviousMonth = -1;
            int PreviousYear = -1;

            if (CurrentMonth == 1)
            {
                PreviousMonth = 12;
                PreviousYear = CurrentYear - 1;
            }
            else
            {
                PreviousMonth = CurrentMonth - 1;
                PreviousYear = CurrentYear;
            }

            TempDay = await DataBaseHandler.GetMaxValue("WorkedAmmount", PreviousMonth,PreviousYear);
            TempDay.ConvertSaveDataToUsableData();

            PreviousMonthMostWorked = TempDay.WorkedAmmountC;
            PreviousMonthMostWorkedDate = TempDay.DateC;

            TempDay = await DataBaseHandler.GetMaxValue("RestedAmmount", PreviousMonth, PreviousYear);
            TempDay.ConvertSaveDataToUsableData();

            PreviousMonthMostRested= TempDay.RestedAmmountC;
            PreviousMonthMostRestedDate = TempDay.DateC;

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

            PMMRTRatio.Text = PreviousMonthWorkRestRatio.ToString();

            PMRDays.Text = PreviousMonthTotalDays.ToString();

            CMMRTRatio.Text = CurrentMonthWorkRestRatio.ToString();

            CMRDays.Text = PreviousMonthTotalDays.ToString();
        }
    }
}
