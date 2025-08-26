using System;
using System.Collections.Generic;
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

            MediaPlayer Finish = new();
            Finish.Open(new Uri("Assets/Sounds/Finish.mp3", UriKind.Relative));

            Sounds.Add(ISoundService.SoundType.Warning, Warning);
            Sounds.Add(ISoundService.SoundType.Termination, Termination);
            Sounds.Add(ISoundService.SoundType.Finish, Finish);
        }

        public void PlaySound(ISoundService.SoundType type)
        {
            MediaPlayer activeSound = Sounds[type];
            activeSound.Position = TimeSpan.Zero;
            activeSound.Play();
        }
    }
}
