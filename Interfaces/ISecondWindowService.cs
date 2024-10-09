using System.Threading.Tasks;

namespace WorkLifeBalance.Interfaces
{
    public interface ISecondWindowService
    {
        Task OpenSettings();
        Task OpenViewData();
        Task ViewDays();
        Task BackgroundProcesses();
        Task ViewDayActivity();
        Task Options();
    }
}