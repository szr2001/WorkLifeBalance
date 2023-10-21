using System;
using System.Collections.Generic;
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
using WorkLifeBalance.Handlers;
using WorkLifeBalance.Windows;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for ViewDayDetailsPage.xaml
    /// </summary>
    public partial class ViewDayDetailsPage : SecondWindowPageBase
    {
        public ProcessActivity[] activities { get; set; } = new ProcessActivity[0];

        //use args to pass date
        DateOnly TodayDate = DataHandler.Instance.TodayData.DateC;
        private int LoadedPageType = 0;
        private DayData LoadedDayData = new();
        public ViewDayDetailsPage(object? args) : base(args)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(430, 440);
            if (args != null)
            {
                if (args is (int loadedpagetype, DayData day))
                {
                    LoadedPageType = loadedpagetype;
                    LoadedDayData = day;
                    pageNme = $"{LoadedDayData.DateC.ToString("MM/dd/yyyy")} Activity";
                    _ = RequiestData();
                    return;
                }
            }

            MainWindow.ShowErrorBox("Error ViewDayDetails", "Requested ViewDayDetails Page with no/wrong arguments", true);
        }

        private async Task RequiestData()
        {
            activities = (await DataBaseHandler.ReadDayActivity(LoadedDayData.Date)).ToArray();

            WorkedT.Text = LoadedDayData.WorkedAmmountC.ToString("HH:mm:ss");
            RestedT.Text = LoadedDayData.RestedAmmountC.ToString("HH:mm:ss");
            DataContext = this;
        }

        private void BackToViewDaysPage(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.ViewDays, LoadedPageType);
        }
    }
}
