using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkLifeBalance.Services;

namespace WorkLifeBalance.ViewModels
{
    public class MainMenuVM
    {
        private TimeHandler mainTimer;
        private LowLevelHandler lowLevelTimer;
        public MainMenuVM(TimeHandler maintimer, LowLevelHandler lowlevelhandler)
        {
            mainTimer = maintimer;
            lowLevelTimer = lowlevelhandler;
        }
    }
}
