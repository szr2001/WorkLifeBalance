using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WorkLifeBalance.HandlerClasses;
using WorkLifeBalance.Handlers;
using WorkLifeBalance.Windows;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for BackgroundWindowsViewPage.xaml
    /// </summary>
    public partial class BackgroundWindowsViewPage : SecondWindowPageBase
    {
        public ObservableCollection<string> DetectedWindows { get; set; } = new();
        public ObservableCollection<string> SelectedWindows { get; set; } = new();
        public BackgroundWindowsViewPage(object? args) : base(args)
        {
            RequiredWindowSize = new Vector2(700, 570);
            pageNme = "Automatic Customize";
            InitializeProcessNames();
            DataContext = this;
            InitializeComponent();
            AutomaticStateChangerHandler.Instance.OnWindowChange += UpdateActiveWindowUi;
        }

        private void InitializeProcessNames()
        {
            SelectedWindows = new ObservableCollection<string>(DataHandler.Instance.AutoChangeData.WorkingStateWindows);
            DetectedWindows = new ObservableCollection<string>(WindowOptionsHelper.GetBackgroundApplicationsName());
            DetectedWindows = new ObservableCollection<string>(DetectedWindows.Except(SelectedWindows));
        }

        private void ReturnToPreviousPage(object sender, RoutedEventArgs e)
        {
           SecondWindow.RequestSecondWindow(SecondWindowType.Settings);

        }

        public override async Task ClosePageAsync()
        {
            AutomaticStateChangerHandler.Instance.OnWindowChange -= UpdateActiveWindowUi;
            DataHandler.Instance.AutoChangeData.WorkingStateWindows = SelectedWindows.ToArray();
            await DataHandler.Instance.SaveData();
        }

        private void UpdateActiveWindowUi(string newwindow)
        {
            Console.WriteLine("D");
            ActiveWindowT.Text = newwindow;
        }

        private void SelectProcess(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            TextBlock textBlock = (TextBlock)button.Content;

            if (DetectedWindows.Contains(textBlock.Text))
            {
                DetectedWindows.Remove(textBlock.Text);
                SelectedWindows.Add(textBlock.Text);
            }
        }

        private void DeselectProcess(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            TextBlock textBlock = (TextBlock)button.Content;

            if (SelectedWindows.Contains(textBlock.Text))
            {
                SelectedWindows.Remove(textBlock.Text);
                DetectedWindows.Add(textBlock.Text);
            }
        }
    }
}
