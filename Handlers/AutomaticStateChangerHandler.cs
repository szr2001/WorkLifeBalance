using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.Handlers
{
    public class AutomaticStateChangerHandler
    {
        private static AutomaticStateChangerHandler? _instance;
        public static AutomaticStateChangerHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AutomaticStateChangerHandler();
                }
                return _instance;
            }
        }
    }
}
