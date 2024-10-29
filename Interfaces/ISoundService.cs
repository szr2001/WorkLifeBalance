using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLifeBalance.Interfaces
{
    public interface ISoundService
    {
        public void PlaySound(SoundType type);

        public enum SoundType 
        {
            Warning,
            Termination
        }
    }
}
