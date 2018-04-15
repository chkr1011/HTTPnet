using System;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Http;

namespace HTTPnet.Communication
{
    public sealed class ClientSession : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpServer _httpServer;

        private IClientSessionHandler _sessionHandler;
        
        public ClientSession(IClientSocketWrapper client, HttpServer httpServer, HttpServerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _httpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            
            _sessionHandler = new HttpClientSessionHandler(this, options);
        }

        public IClientSocketWrapper Client { get; }
        
        public async Task RunAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await _sessionHandler.ProcessAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public Task HandleHttpRequestAsync(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            return _httpServer.HandleHttpRequestAsync(httpContext);
        }

        public void SwitchProtocol(IClientSessionHandler sessionHandler)
        {
            _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
        }

        public void Close()
        {
            _cancellationTokenSource?.Cancel(false);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel(false);
            _cancellationTokenSource?.Dispose();

            Client?.Dispose();
        }
    }
}
