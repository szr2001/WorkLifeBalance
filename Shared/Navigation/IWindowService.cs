namespace WorkLifeBalance.Shared.Navigation
{
    public interface IWindowService<in T> where T : PageViewModelBase
    {
        Action? OnPageLoaded { get; set; }

        Task Close();

        Task OpenWith<TViewModel>(object? args = null) where TViewModel : PageViewModelBase;
    }
}
