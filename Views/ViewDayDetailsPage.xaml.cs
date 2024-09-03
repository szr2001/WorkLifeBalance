using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WorkLifeBalance.Models;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Views
{
    /// <summary>
    /// Interaction logic for ViewDayDetailsPage.xaml
    /// </summary>
    public partial class ViewDayDetailsPage : Page
    {
        public ProcessActivityData[] activities { get; set; } = new ProcessActivityData[0];

        //use args to pass date
        private int LoadedPageType = 0;
        private DayData LoadedDayData = new();
        private ViewDaysDetailsPageVM viewDaysDetailsPageVM;
        public ViewDayDetailsPage(ViewDaysDetailsPageVM viewDaysDetailsPageVM)
        {
            InitializeComponent();
            //RequiredWindowSize = new Vector2(430, 440);
            //if (args != null)
            //{
            //    if (args is (int loadedpagetype, DayData day))
            //    {
            //        LoadedPageType = loadedpagetype;
            //        LoadedDayData = day;
            //        pageNme = $"{LoadedDayData.DateC.ToString("MM/dd/yyyy")} Activity";
            //        _ = RequiestData();
            //        return;
            //    }
            //}

            //MainWindow.ShowErrorBox("Error ViewDayDetails", "Requested ViewDayDetails Page with no/wrong arguments");
            this.viewDaysDetailsPageVM = viewDaysDetailsPageVM;
        }

        private async Task RequiestData()
        {
            //List<ProcessActivityData> RequestedActivity = (await DataBaseHandler.ReadDayActivity(LoadedDayData.Date));
            //activities = RequestedActivity.OrderByDescending(data => data.TimeSpentC).ToArray();

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
