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
        public ViewDayDetailsPage(object? args) : base(args)
        {
            InitializeComponent();
            pageNme = $"{TodayDate.ToString("MM/dd/yyyy")} Activity";
            _ = RequiestData();
            RequiredWindowSize = new Vector2(330, 440);

            if (args != null)
            {
                if (args is int loadedpagetype)
                {
                    _ = RequiestData();
                    LoadedPageType = loadedpagetype;
                    return;
                }
            }

            MainWindow.ShowErrorBox("Error ViewDayDetails", "Requested ViewDayDetails Page with no/wrong arguments", true);
        }

        private async Task RequiestData()
        {
            activities = (await DataBaseHandler.ReadDayActivity(TodayDate.ToString("MMddyyyy"))).ToArray();
            DataContext = this;
        }

        private void BackToViewDaysPage(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.ViewDays, LoadedPageType);
        }
    }
}
