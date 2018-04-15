#if NET452 || NET462 || NET471 || NETSTANDARD1_3 || NETSTANDARD2_0
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using HTTPnet.Communication;
using HTTPnet.Http;

namespace HTTPnet.Implementations
{
    public class ServerSocketWrapper : IServerSocketWrapper
    {
        private readonly HttpServerOptions _options;
        private Socket _listener;

        public ServerSocketWrapper(HttpServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Task StartAsync()
        {
            if (_listener != null)
            {
                return Task.FromResult(0);
            }

            _listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(_options.BoundIPAddress, _options.Port));
            _listener.Listen(_options.Backlog);
            
            return Task.FromResult(0);
        }

        public Task StopAsync()
        {
            _listener?.Shutdown(SocketShutdown.Both);
            _listener?.Dispose();
            _listener = null;

            return Task.FromResult(0);
        }

        public void Dispose()
        {
            _listener?.Shutdown(SocketShutdown.Both);
            _listener?.Dispose();
            _listener = null;
        }

        public async Task<IClientSocketWrapper> AcceptAsync()
        {
#if NET462 || NET452
            var clientSocket = await Task.Factory.FromAsync(_listener.BeginAccept, _listener.EndAccept, null).ConfigureAwait(false);
#else
            var clientSocket = await _listener.AcceptAsync().ConfigureAwait(false);
#endif
            
            return new ClientSocketWrapper(clientSocket);
        }
    }
}
#endif