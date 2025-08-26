namespace WorkLifeBalance.Interfaces
{
    public interface ISoundService
    {
        public void PlaySound(SoundType type);

        public enum SoundType 
        {
            Warning,
            Termination,
            Finish
        }
    }
}
