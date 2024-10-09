using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Models;

namespace WorkLifeBalance.ViewModels
{
    public partial class ViewDayDetailsPageVM : SecondWindowPageVMBase
    {
        public ProcessActivityData[] activities { get; set; } = new ProcessActivityData[0];

        private int LoadedPageType = 0;
        private DayData LoadedDayData = new();
        private ISecondWindowService secondWindowService;

        public ViewDayDetailsPageVM(ISecondWindowService secondWindowService)
        {
            RequiredWindowSize = new Vector2(430, 440);
            WindowPageName = "View Day Details";
            this.secondWindowService = secondWindowService;
        }

        private async Task RequiestData()
        {
            //List<ProcessActivityData> RequestedActivity = (await DataBaseHandler.ReadDayActivity(LoadedDayData.Date));
            //activities = RequestedActivity.OrderByDescending(data => data.TimeSpentC).ToArray();

            //WorkedT.Text = LoadedDayData.WorkedAmmountC.ToString("HH:mm:ss");
            //RestedT.Text = LoadedDayData.RestedAmmountC.ToString("HH:mm:ss");
            //DataContext = this;
        }

        public override Task OnPageOppeningAsync(object args)
        {
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
            return base.OnPageOppeningAsync(args);
        }

        [RelayCommand]
        private void BackToViewDaysPage()
        {
            secondWindowService.OpenWindowWith<ViewDaysPageVM>(LoadedPageType);
        }
    }
}
