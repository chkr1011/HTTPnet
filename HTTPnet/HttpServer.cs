using System;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Communication;
using HTTPnet.Diagnostics;
using HTTPnet.Http;

namespace HTTPnet
{
    public sealed class HttpServer : IHttpServer
    {
        private readonly Func<HttpServerOptions, IServerSocketWrapper> _socketWrapperFactory;

        private IServerSocketWrapper _socketWrapper;
        private HttpServerOptions _options;
        private CancellationTokenSource _cancellationTokenSource;

        public HttpServer(Func<HttpServerOptions, IServerSocketWrapper> socketWrapperFactory)
        {
            _socketWrapperFactory = socketWrapperFactory ?? throw new ArgumentNullException(nameof(socketWrapperFactory));
        }

        public IHttpRequestHandler RequestHandler { get; set; }

        public async Task StartAsync(HttpServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _socketWrapper = _socketWrapperFactory(options);

                await _socketWrapper.StartAsync().ConfigureAwait(false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => AcceptConnectionsAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (Exception)
            {
                await StopAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (_socketWrapper != null)
            {
                await _socketWrapper.StopAsync().ConfigureAwait(false);
            }

            _socketWrapper?.Dispose();
            _socketWrapper = null;
        }

        private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await _socketWrapper.AcceptAsync().ConfigureAwait(false);
                    HttpNetTrace.Information(nameof(HttpServer), "Client '{0}' connected.", client.Identifier);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() => HandleClientAsync(client), _cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                HttpNetTrace.Error(nameof(HttpServer), exception, "Unhandled exception while accepting clients.");
            }
            finally
            {
                _cancellationTokenSource?.Cancel(false);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel(false);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _socketWrapper?.Dispose();
        }

        private async Task HandleClientAsync(IClientSocketWrapper client)
        {
            using (var clientSession = new ClientSession(client, this, _options))
            {
                try
                {
                    await clientSession.RunAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    HttpNetTrace.Error(nameof(HttpServer), exception, "Unhandled exception while handling cient connection.");
                }
                finally
                {
                    HttpNetTrace.Information(nameof(HttpServer), "Client '{0}' disconnected.", client.Identifier);
                    await client.DisconnectAsync();
                }
            }
        }

        internal async Task HandleHttpRequestAsync(HttpContext httpContext)
        {
            try
            {
                var handler = RequestHandler;
                if (handler == null)
                {
                    return;
                }

                await RequestHandler.HandleHttpRequestAsync(httpContext).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                HttpNetTrace.Error(nameof(HttpServer), exception, "Unhandled exception while handling received HTTP request.");
            }
        }
    }
}