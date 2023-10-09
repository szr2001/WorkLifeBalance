using System;
using System.Collections.Generic;
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
    /// Interaction logic for ViewDaysPage.xaml
    /// </summary>
    public partial class ViewDaysPage : SecondWindowPageBase
    {
        public DayData[] LoadedData { get; set; }
        public ViewDaysPage(SecondWindow secondwindow, object? args) : base(secondwindow, args)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(710, 570);

            if(args != null)
            {
                if(args is int loadedpagetype)
                {
                    _ = RequiestData(loadedpagetype);

                    DataContext = this;
                    return;
                }
            }

            ParentWindow.Close();
        }

        private async Task RequiestData(int requiestedDataType)
        {
            DateOnly currentDate = ParentWindow.MainWindowParent.TodayData.DateC;
            DateTime previousMonthDateTime = currentDate.ToDateTime(new TimeOnly(0, 0, 0)).AddMonths(-1);
            DateOnly previousDate = DateOnly.FromDateTime(previousMonthDateTime);

            List<DayData> Days = new();

            switch (requiestedDataType)
            {
                //call database
                case 0:
                    Days = await DataBaseHandler.ReadMonth();
                    pageNme = "All Months Days";
                    break;
                case 1:
                    Days = await DataBaseHandler.ReadMonth(currentDate.ToString("MM"), currentDate.ToString("yyyy"));
                    pageNme = "Current Month Days";
                    break;
                case 2:
                    Days = await DataBaseHandler.ReadMonth(previousDate.ToString("MM"), previousDate.ToString("yyyy"));
                    pageNme = "Previous Month Days";
                    break;
            }
            LoadedData = Days.ToArray();
        }

        private void ReturnToPreviousPage(object sender, RoutedEventArgs e)
        {
            ParentWindow.MainWindowParent.OpenSecondWindow(SecondWindowType.ViewData);
        }
    }
}
