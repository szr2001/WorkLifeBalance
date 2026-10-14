namespace WorkLifeBalance.Shared.Navigation
{
    public abstract class PageViewModelBase : ViewModelBase
    {
        public virtual Task OnPageOpeningAsync(object? args = null) => Task.CompletedTask;

        public virtual Task OnPageClosingAsync() => Task.CompletedTask;
    }
}
