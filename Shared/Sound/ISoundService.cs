namespace WorkLifeBalance.Shared.Sound
{
    public interface ISoundService
    {
        void PlaySound(SoundType type);
    }

    public enum SoundType
    {
        Warning,
        Termination,
        Finish,
    }
}
