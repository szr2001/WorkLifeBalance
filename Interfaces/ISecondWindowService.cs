using System.Threading.Tasks;
using WorkLifeBalance.ViewModels;

namespace WorkLifeBalance.Interfaces
{
    public interface ISecondWindowService
    {
        Task OpenWindowWith<T>(object? args = null) where T : SecondWindowPageVMBase;
    }
}