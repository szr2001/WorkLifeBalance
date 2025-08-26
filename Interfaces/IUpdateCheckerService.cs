using System.Threading.Tasks;

namespace WorkLifeBalance.Interfaces
{
    public interface IUpdateCheckerService
    {
        public Task CheckForUpdate();
    }
}
