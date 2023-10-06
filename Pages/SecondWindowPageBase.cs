using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WorkLifeBalance.Windows;

namespace WorkLifeBalance.Pages
{
    public class SecondWindowPageBase : Page
    {
        public SecondWindow ParentWindow;
        public Vector2 RequiredWindowSize = new Vector2(250,300);
        public string pageNme = "Page";
        public SecondWindowPageBase(SecondWindow secondwindow, object? args)
        {
            ParentWindow = secondwindow;
        }
    }
}
