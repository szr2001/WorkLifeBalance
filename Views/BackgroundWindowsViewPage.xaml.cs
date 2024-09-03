using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WorkLifeBalance.Services;
using WorkLifeBalance.Services.Feature;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for BackgroundWindowsViewPage.xaml
    /// </summary>
    public partial class BackgroundWindowsViewPage : Page
    {
        public ObservableCollection<string> DetectedWindows { get; set; } = new();
        public ObservableCollection<string> SelectedWindows { get; set; } = new();
        private BackgroundWindowsViewPageVM backgroundWindowsViewPageVM;
        public BackgroundWindowsViewPage(BackgroundWindowsViewPageVM backgroundWindowsViewPageVM)
        {
            RequiredWindowSize = new Vector2(700, 570);
            pageNme = "Automatic Customize";
            InitializeProcessNames();
            DataContext = this;
            InitializeComponent();
            ActivityTrackerFeature.Instance.OnWindowChange += UpdateActiveWindowUi;
            this.backgroundWindowsViewPageVM = backgroundWindowsViewPageVM;
        }

        private void InitializeProcessNames()
        {
            SelectedWindows = new ObservableCollection<string>(DataStorageFeature.Instance.AutoChangeData.WorkingStateWindows);
            DetectedWindows = new ObservableCollection<string>(LowLevelHandler.GetBackgroundApplicationsName());
            DetectedWindows = new ObservableCollection<string>(DetectedWindows.Except(SelectedWindows));
        }

        private void ReturnToPreviousPage(object sender, RoutedEventArgs e)
        {
            SecondWindow.RequestSecondWindow(SecondWindowType.Settings);

        }

        public override async Task ClosePageAsync()
        {
            ActivityTrackerFeature.Instance.OnWindowChange -= UpdateActiveWindowUi;
            DataStorageFeature.Instance.AutoChangeData.WorkingStateWindows = SelectedWindows.ToArray();
            await DataStorageFeature.Instance.SaveData();
        }

        private void UpdateActiveWindowUi(string newwindow)
        {
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
