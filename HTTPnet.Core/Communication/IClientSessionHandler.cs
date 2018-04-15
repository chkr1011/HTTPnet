using System;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPnet.Communication
{
    public interface IClientSessionHandler : IDisposable
    {
        Task ProcessAsync(CancellationToken cancellationToken);
    }
}
