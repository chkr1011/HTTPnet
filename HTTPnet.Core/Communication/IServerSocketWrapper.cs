using System;
using System.Threading.Tasks;

namespace HTTPnet.Core.Communication
{
    public interface IServerSocketWrapper : IDisposable
    {
        Task StartAsync();
        Task StopAsync();

        Task<IClientSocketWrapper> AcceptAsync();
    }
}
