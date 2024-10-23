using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.Interfaces
{
    public interface IUpdateCheckerService
    {
        public Task CheckForUpdate();
    }
}
