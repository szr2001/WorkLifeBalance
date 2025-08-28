﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WorkLifeBalance.Models;
using WorkLifeBalance.Interfaces;
using CommunityToolkit.Mvvm.Input;
using WorkLifeBalance.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using WorkLifeBalance.Services.Feature;

namespace WorkLifeBalance.ViewModels
{
    public partial class ViewDaysPageVM : SecondWindowPageVMBase
    {
        public ObservableCollection<DayData> LoadedData { get; set; } = new();

        [ObservableProperty]
        private int[]? filterDays;

        [ObservableProperty]
        private int selectedDay = 0;
        
        [ObservableProperty]
        private int[]? filterMonths;

        [ObservableProperty]
        private int selectedMonth = 0;
        
        [ObservableProperty]
        private int[]? filterYears;

        [ObservableProperty]
        private int selectedYear = 0;

        private DayData[]? backupdata;
        //use this to request the correct page when leaving the DayActivity page
        private int LoadedPageType;
        private IWindowService<SecondWindowPageVMBase> secondWindowService;
        private DataBaseHandler database;
        private DataStorageFeature dataStorage;
        public ViewDaysPageVM(IWindowService<SecondWindowPageVMBase> secondWindowService, DataBaseHandler database, DataStorageFeature dataStorage)
        {
            PageHeight = 570;
            PageWidth = 710;
            this.secondWindowService = secondWindowService;
            this.database = database;
            this.dataStorage = dataStorage;
            SetFilterValues();
        }

        private void SetFilterValues()
        {
            //use the database to choose what days/month/years should the filters contain
            DateOnly Today = dataStorage.TodayData.DateC;
            List<int> Days = new();
            List<int> Months = new();
            List<int> Years = new();
            for (int x = 0; x < 31; x++)
            {
                Days.Add(x);
                FilterDays = Days.ToArray();
            }
            for (int x = 0; x < 13; x++)
            {
                Months.Add(x);
                FilterMonths = Months.ToArray();
            }
            for (int x = Today.Year; x > 2020 ; x--)
            {
                Years.Add(x);
            }
            Years.Add(0);
            FilterYears = Years.ToArray();
        }

        private async Task RequiestData(int requiestedDataType = 0)
        {
            DateOnly currentDate = dataStorage.TodayData.DateC;
            DateTime previousMonthDateTime = currentDate.ToDateTime(new TimeOnly(0, 0, 0)).AddMonths(-1);
            DateOnly previousDate = DateOnly.FromDateTime(previousMonthDateTime);

            List<DayData> Days = new();
            switch (requiestedDataType)
            {
                case 0:
                    Days = await database.ReadMonth();
                    PageName = "All Months Days";
                    break;
                case 1:
                    Days = await database.ReadMonth(currentDate.ToString("MM"), currentDate.ToString("yyyy"));
                    PageName = "Current Month Days";
                    break;
                case 2:
                    Days = await database.ReadMonth(previousDate.ToString("MM"), previousDate.ToString("yyyy"));
                    PageName = "Previous Month Days";
                    break;
            }

            SelectedMonth = 0;
            SelectedDay = 0;
            SelectedYear = 0;

            Days.Reverse();
            LoadedData = new ObservableCollection<DayData>(Days);

            backupdata = LoadedData.ToArray();
        }

        public override Task OnPageClosingAsync() => Task.CompletedTask;

        public override async Task OnPageOpeningAsync(object? args = null)
        {
            if (args != null)
            {
                if (args is int loadedpagetype)
                {
                    await RequiestData(loadedpagetype);
                    LoadedPageType = loadedpagetype;
                }
            }
        }

        [RelayCommand]
        private void ReturnToPreviousPage()
        {
            secondWindowService.OpenWith<ViewDataPageVM>();
        }

        [RelayCommand]
        private void ViewDay(DayData data)
        {
            data.ConvertSaveDataToUsableData();

            secondWindowService.OpenWith<ViewDayDetailsPageVM>((LoadedPageType, data));
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            if (SelectedMonth == 0 && SelectedDay == 0 && SelectedYear == 0)
            {
                LoadedData.Clear();

                foreach (DayData day in backupdata!)
                {
                    LoadedData.Add(day);
                }
                return;
            }

            DayData[] tempdata = backupdata!;
            if (SelectedMonth != 0)
            {
                tempdata = tempdata.Where(daydata => daydata.DateC.Month == SelectedMonth).ToArray();
            }
            if (SelectedDay != 0)
            {
                tempdata = tempdata.Where(daydata => daydata.DateC.Day == SelectedDay).ToArray();
            }
            if (SelectedYear != 0)
            {
                tempdata = tempdata.Where(daydata => daydata.DateC.Year == SelectedYear).ToArray();
            }

            LoadedData.Clear();

            foreach (DayData day in tempdata)
            {
                LoadedData.Add(day);
            }
        }
    }
}
