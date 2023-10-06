using System;
using System.Collections.Generic;
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
using WorkLifeBalance.Windows;

namespace WorkLifeBalance.Pages
{
    /// <summary>
    /// Interaction logic for ViewDaysPage.xaml
    /// </summary>
    public partial class ViewDaysPage : SecondWindowPageBase
    {
        List<DayData> LoadedData = new();
        public ViewDaysPage(SecondWindow secondwindow, object? args) : base(secondwindow, args)
        {
            InitializeComponent();
            RequiredWindowSize = new Vector2(700, 500);
            pageNme = "Days Viewer";

            if(args != null)
            {
                if(args is int loadedpagetype)
                {
                    _ = RequiestData(loadedpagetype);
                    return;
                }
            }

            ParentWindow.Close();
        }

        private async Task RequiestData(int requiestedDataType)
        {
            switch (requiestedDataType)
            {
                //call database
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    break;
            }
            LoadUi();
        }
        
        private void LoadUi()
        {

        }
    }
}
