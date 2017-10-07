using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Core.Http;

namespace HTTPnet.Core.Communication
{
    public sealed class ClientSession : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpServerOptions _options;
        private readonly HttpServer _httpServer;
        private ISessionHandler _sessionHandler;
        
        public ClientSession(IClientSocketWrapper client, HttpServer httpServer, HttpServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            
            _sessionHandler = new HttpSessionHandler(this, options);
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;
        public IClientSocketWrapper Client { get; }
        
        public async Task RunAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                Debug.Assert(_sessionHandler != null);

                await _sessionHandler.ProcessAsync();
            }
        }

        public Task HandleHttpRequestAsync(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            return _httpServer.HandleHttpRequestAsync(httpContext);
        }

        public void SwitchProtocol(ISessionHandler sessionHandler)
        {
            _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
        }

        public void Close()
        {
            _cancellationTokenSource?.Cancel(false);
        }

        public void Dispose()
        {
            Close();
            Client?.Dispose();
        }
    }
}
