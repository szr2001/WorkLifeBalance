using System.Windows.Media;

namespace WorkLifeBalance.Shared.Sound
{
    public class SoundService : ISoundService
    {
        private readonly Dictionary<SoundType, MediaPlayer> _sounds = new()
        {
            [SoundType.Warning] = Open("Assets/Sounds/Error.mp3"),
            [SoundType.Termination] = Open("Assets/Sounds/Termination.mp3"),
            [SoundType.Finish] = Open("Assets/Sounds/Finish.mp3"),
        };

        public void PlaySound(SoundType type)
        {
            var sound = _sounds[type];
            sound.Position = TimeSpan.Zero;
            sound.Play();
        }

        private static MediaPlayer Open(string path)
        {
            var player = new MediaPlayer();
            player.Open(new Uri(path, UriKind.Relative));
            return player;
        }
    }
}
