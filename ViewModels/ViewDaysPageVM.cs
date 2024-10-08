using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using WorkLifeBalance.Models;

namespace WorkLifeBalance.ViewModels
{
    public class ViewDaysPageVM : SecondWindowPageVMBase
    {
        public ObservableCollection<DayData> LoadedData { get; set; } = new();

        private DayData[] backupdata;
        //use this to request the correct page when leaving the DayActivity page
        private int LoadedPageType = 0;
        private string FilterMonth = "00";
        private string FilterDay = "00";
        private string FilterYear = "0000";
        public ViewDaysPageVM()
        {
            //RequiredWindowSize = new Vector2(710, 570);
            //if (args != null)
            //{
            //    if (args is int loadedpagetype)
            //    {
            //        _ = RequiestData(loadedpagetype);
            //        LoadedPageType = loadedpagetype;
            //        return;
            //    }
            //}

            //MainWindow.ShowErrorBox("Error ViewDaysPage", "Requested ViewDays Page with no/wrong arguments");
        }

        private async Task RequiestData(int requiestedDataType)
        {
            //DateOnly currentDate = DataStorageFeature.Instance.TodayData.DateC;
            //DateTime previousMonthDateTime = currentDate.ToDateTime(new TimeOnly(0, 0, 0)).AddMonths(-1);
            //DateOnly previousDate = DateOnly.FromDateTime(previousMonthDateTime);

            List<DayData> Days = new();
            //switch (requiestedDataType)
            //{
            //    //call database
            //    case 0:
            //        Days = await DataBaseHandler.ReadMonth();
            //        pageNme = "All Months Days";
            //        break;
            //    case 1:
            //        Days = await DataBaseHandler.ReadMonth(currentDate.ToString("MM"), currentDate.ToString("yyyy"));
            //        pageNme = "Current Month Days";
            //        break;
            //    case 2:
            //        Days = await DataBaseHandler.ReadMonth(previousDate.ToString("MM"), previousDate.ToString("yyyy"));
            //        pageNme = "Previous Month Days";
            //        break;
            //}

            Days.Reverse();
            LoadedData = new ObservableCollection<DayData>(Days);

            backupdata = LoadedData.ToArray();
        }

        private void ReturnToPreviousPage(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.ViewData);
        }

        private void ViewDay(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DayData ClickedDay = new();

            StackPanel ButtonParent = FindParent<StackPanel>(button);

            TextBlock tempblock = (TextBlock)ButtonParent.Children[1];
            ClickedDay.WorkedAmmount = tempblock.Text.Replace(":", "");

            tempblock = (TextBlock)ButtonParent.Children[3];
            ClickedDay.RestedAmmount = tempblock.Text.Replace(":", "");

            tempblock = (TextBlock)ButtonParent.Children[4];
            ClickedDay.Date = tempblock.Text.Replace("/", "");

            ClickedDay.ConvertSaveDataToUsableData();

            SecondWindow.RequestSecondWindow(SecondWindowType.ViewDayActivity, (LoadedPageType, ClickedDay));
        }

        private void ApplyFilters(object sender, RoutedEventArgs e)
        {
            if (FilterMonth == "00" && FilterDay == "00" && FilterYear == "0000")
            {
                LoadedData.Clear();

                foreach (DayData day in backupdata)
                {
                    LoadedData.Add(day);
                }
                return;
            }

            DayData[] tempdata = backupdata;
            if (FilterMonth != "00")
            {
                tempdata = tempdata.Where(daydata => daydata.DateC.ToString("MM") == FilterMonth).ToArray();
            }
            if (FilterDay != "00")
            {
                tempdata = tempdata.Where(daydata => daydata.DateC.ToString("dd") == FilterDay).ToArray();
            }
            if (FilterYear != "0000")
            {
                tempdata = tempdata.Where(daydata => daydata.DateC.ToString("yyyy") == FilterYear).ToArray();
            }

            LoadedData.Clear();

            foreach (DayData day in tempdata)
            {
                LoadedData.Add(day);
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }

        private void UpdateFilterMonth(object sender, TextChangedEventArgs e)
        {
            //if (string.IsNullOrEmpty(MonthB.Text) || !int.TryParse(MonthB.Text, out _))
            //{
            //    MonthB.Text = "00";
            //    FilterMonth = "00";
            //    return;
            //}

            //FilterMonth = MonthB.Text;
        }

        private void UpdateFilterDay(object sender, TextChangedEventArgs e)
        {
            //if (string.IsNullOrEmpty(DayB.Text) || !int.TryParse(DayB.Text, out _))
            //{
            //    DayB.Text = "00";
            //    FilterDay = "00";
            //    return;
            //}

            //FilterDay = DayB.Text;
        }

        private void UpdateFilterYear(object sender, TextChangedEventArgs e)
        {
            //if (string.IsNullOrEmpty(YearB.Text) || !int.TryParse(YearB.Text, out _))
            //{
            //    YearB.Text = "0000";
            //    FilterYear = "0000";
            //    return;
            //}

            //FilterYear = YearB.Text;
        }
    }
}
