﻿using System;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services
{
    public class SecondWindowService : ISecondWindowService
    {
        private readonly SecondWindowVM secondWindowVm;
        private readonly INavigationService navigation;

        public SecondWindowService(SecondWindowVM secondWindowVm, INavigationService navigation)
        {
            this.secondWindowVm = secondWindowVm;
            this.navigation = navigation;
            secondWindowVm.OnWindowClosing += ClearWindow;
        }

        private async Task ClearWindow()
        {
            SecondWindowPageVMBase activeModel = (SecondWindowPageVMBase)navigation.ActiveView;
            await activeModel.OnPageClosingAsync();
            secondWindowVm.OnWindowClosed.Invoke();
        }

        public async Task OpenWindowWith<T>(object args) where T : SecondWindowPageVMBase 
        {
            navigation.NavigateTo<T>();
            SecondWindowPageVMBase activeModel = (SecondWindowPageVMBase)navigation.ActiveView;
            secondWindowVm.Width = (int)activeModel.RequiredWindowSize.X;
            secondWindowVm.Height = (int)activeModel.RequiredWindowSize.Y;
            secondWindowVm.PageName = activeModel.WindowPageName;
            secondWindowVm.ActivePage = activeModel;
            secondWindowVm.OnWindowRequested.Invoke();
            await activeModel.OnPageOppeningAsync(args);
        }
    }
}
