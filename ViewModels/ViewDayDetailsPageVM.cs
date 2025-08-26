using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services;

namespace WorkLifeBalance.ViewModels
{
    public partial class ViewDayDetailsPageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        private ProcessActivityData[]? processActivities;

        [ObservableProperty]
        private PageActivityData[]? pageActivities;
        
        [ObservableProperty]
        private DayData? loadedDayData;

        private int LoadedPageType;
        private IWindowService<SecondWindowPageVMBase> secondWindowService;
        private DataBaseHandler database;
        public ViewDayDetailsPageVM(IWindowService<SecondWindowPageVMBase> secondWindowService, DataBaseHandler database)
        {
            PageHeight = 440;
            PageWidth = 630;
            PageName = "View Day Details";
            this.secondWindowService = secondWindowService;
            this.database = database;
        }

        private async Task RequestData()
        {
            List<ProcessActivityData> RequestedActivity = (await database.ReadProcessDayActivity(LoadedDayData!.Date));
            List<PageActivityData> RequestedPageActivity = (await database.ReadUrlDayActivity(LoadedDayData!.Date));
            
            ProcessActivities = RequestedActivity.OrderByDescending(data => data.TimeSpentC).ToArray();
            PageActivities = RequestedPageActivity.OrderByDescending(data => data.TimeSpentC).ToArray();
        }

        public override Task OnPageClosingAsync() => Task.CompletedTask;

        public override Task OnPageOpeningAsync(object? args)
        {
            if (args != null)
            {
                if (args is (int loadedpagetype, DayData day))
                {
                    LoadedPageType = loadedpagetype;
                    LoadedDayData = day;
                    PageName = $"{LoadedDayData.DateC.ToString("MM/dd/yyyy")} Activity";
                    _ = RequestData();
                    return Task.CompletedTask;
                }
            }

            //MainWindow.ShowErrorBox("Error ViewDayDetails", "Requested ViewDayDetails Page with no/wrong arguments");
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void BackToViewDaysPage()
        {
            secondWindowService.OpenWith<ViewDaysPageVM>(LoadedPageType);
        }
    }
}
