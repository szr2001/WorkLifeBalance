using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.Data
{
    public class WLBSettings
    {
        public string LastTimeOpened { get; set; } = "";
        public int StartWithWindows { get; set; } = 0;
        public int StartUpCorner { get; set; } = 0;

        public DateTime LastTimeOpenedC = new();
        public bool StartWithWindowsC = false;
        public AnchorCorner StartUpCornerC = AnchorCorner.BootomLeft;
        
        public void ConvertSaveDataToUsableData()
        {

        }

        public void ConvertUsableDataToSaveData()
        {

        }
    }
    public enum AnchorCorner
    {
        TopRight = 0,
        TopLeft = 1,
        BootomLeft = 2,
        BottomRight = 3
    }
}
