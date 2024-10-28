using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkLifeBalance.Interfaces;
using System.Windows.Media;

namespace WorkLifeBalance.Services
{
    public class SoundService : ISoundService
    {
        private Dictionary<ISoundService.SoundType, MediaPlayer> Sounds = new();

        public SoundService()
        {
            MediaPlayer Warning = new();
            Warning.Open(new Uri("Assets/Sounds/Error.mp3", UriKind.Relative));

            MediaPlayer Termination = new();
            Termination.Open(new Uri("Assets/Sounds/Termination.mp3", UriKind.Relative));

            Sounds.Add(ISoundService.SoundType.Warning, Warning);
            Sounds.Add(ISoundService.SoundType.Termination, Termination);
        }

        public void PlaySound(ISoundService.SoundType type)
        {
            Sounds[type].Play();
        }
    }
}
