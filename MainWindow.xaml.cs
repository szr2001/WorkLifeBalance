﻿using System;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window, IDisposable
    {
        private readonly MainWindowVM ViewModel;
        private readonly NotifyIcon NotifyIcon;

        public MainWindow(MainWindowVM viewModel)
        {
            Topmost = true;
            ViewModel = viewModel;
            DataContext = viewModel;
            NotifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
            };
            NotifyIcon.DoubleClick += OnNotifyIconOnDoubleClick;
            SetStartUpLocation();
            InitializeComponent();
        }

        private void OnNotifyIconOnDoubleClick(object? sender, EventArgs args)
        {
            Show();
            WindowState = WindowState.Normal;
            NotifyIcon.Visible = false;
        }

        private void SetStartUpLocation()
        {
            int ScreenHeight = (int)SystemParameters.PrimaryScreenHeight;
            Left = 0;
            Top = ScreenHeight - 297;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void HideWindow(object sender, RoutedEventArgs e)
        {
            Hide();
            NotifyIcon.Visible = true;
        }

        public void Dispose()
        {
            NotifyIcon.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
