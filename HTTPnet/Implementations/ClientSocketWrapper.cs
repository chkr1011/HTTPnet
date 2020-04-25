#if NET452 || NET462 || NET471 || NETSTANDARD1_3 || NETSTANDARD2_0
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
        private readonly NetworkStream _networkStream;

        public ClientSocketWrapper(Socket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));

            Identifier = socket.RemoteEndPoint.ToString();
            
            _networkStream = new NetworkStream(socket, true);

            ReceiveStream = _networkStream; // TODO: Use BufferedStream
            SendStream = _networkStream; // TODO: Use BufferedStream
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
            _socket?.Shutdown(SocketShutdown.Both);
            _networkStream?.Dispose();
            _socket?.Dispose();

            ReceiveStream?.Dispose();
            SendStream?.Dispose();
        }
    }
}
#endif