#if WINDOWS_UWP
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using HTTPnet.Communication;

namespace HTTPnet.Implementations
{
    public sealed class ClientSocketWrapper : IClientSocketWrapper
    {
        private readonly Socket _socket;

        public ClientSocketWrapper(Socket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));

            Identifier = socket.RemoteEndPoint.ToString();
            
            ReceiveStream = new NetworkStream(socket, true);
            SendStream = ReceiveStream;
        }

        public string Identifier { get; }

        public Stream ReceiveStream { get; }
        public Stream SendStream { get; }

        public Task DisconnectAsync()
        {
            _socket.Shutdown(SocketShutdown.Both);
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            ReceiveStream?.Dispose();
            SendStream?.Dispose();

            _socket?.Dispose();
        }
    }
}
#endif