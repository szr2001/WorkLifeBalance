using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Services
{
    public class SecondWindowService : ISecondWindowService
    {
        private SecondWindowVM secondWindowVm;
        public SecondWindowService(SecondWindowVM secondWindowVm)
        {
            this.secondWindowVm = secondWindowVm;
        }

        public Task BackgroundProcesses()
        {
            throw new NotImplementedException();
        }

        public Task OpenSettings()
        {
            throw new NotImplementedException();
        }

        public Task OpenViewData()
        {
            throw new NotImplementedException();
        }

        public Task Options()
        {
            throw new NotImplementedException();
        }

        public Task ViewDayActivity()
        {
            throw new NotImplementedException();
        }

        public Task ViewDays()
        {
            throw new NotImplementedException();
        }
    }
}
