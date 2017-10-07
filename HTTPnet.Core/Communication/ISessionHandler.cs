using System.Threading.Tasks;

namespace HTTPnet.Core.Communication
{
    public interface ISessionHandler
    {
        Task ProcessAsync();
    }
}
