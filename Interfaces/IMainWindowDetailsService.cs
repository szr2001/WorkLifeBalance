using System.Threading.Tasks;
using WorkLifeBalance.ViewModels.Base;

namespace WorkLifeBalance.Interfaces
{
    public interface IMainWindowDetailsService
    {
        Task OpenWindowWith<T>() where T : MainWindowDetailsPageBase;
    }
}