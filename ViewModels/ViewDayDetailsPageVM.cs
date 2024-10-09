using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.Models;
using WorkLifeBalance.Services;

namespace WorkLifeBalance.ViewModels
{
    public partial class ViewDayDetailsPageVM : SecondWindowPageVMBase
    {
        [ObservableProperty]
        public ProcessActivityData[]? activities;

        [ObservableProperty]
        private DayData? loadedDayData;

        private int LoadedPageType = 0;
        private ISecondWindowService secondWindowService;
        private DataBaseHandler database;
        public ViewDayDetailsPageVM(ISecondWindowService secondWindowService, DataBaseHandler database)
        {
            RequiredWindowSize = new Vector2(430, 440);
            WindowPageName = "View Day Details";
            this.secondWindowService = secondWindowService;
            this.database = database;
        }

        private async Task RequiestData()
        {
            List<ProcessActivityData> RequestedActivity = (await database.ReadDayActivity(LoadedDayData!.Date));
            Activities = RequestedActivity.OrderByDescending(data => data.TimeSpentC).ToArray();
        }

        public override Task OnPageOppeningAsync(object? args)
        {
            if (args != null)
            {
                if (args is (int loadedpagetype, DayData day))
                {
                    LoadedPageType = loadedpagetype;
                    LoadedDayData = day;
                    WindowPageName = $"{LoadedDayData.DateC.ToString("MM/dd/yyyy")} Activity";
                    _ = RequiestData();
                    return base.OnPageOppeningAsync(args);
                }
            }

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
