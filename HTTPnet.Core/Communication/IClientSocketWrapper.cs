using System;
using System.IO;
using System.Threading.Tasks;

namespace HTTPnet.Communication
{
    public interface IClientSocketWrapper : IDisposable
    {
        string Identifier { get; }

        Stream ReceiveStream { get; }
        Stream SendStream { get; }

        Task DisconnectAsync();
    }
}
