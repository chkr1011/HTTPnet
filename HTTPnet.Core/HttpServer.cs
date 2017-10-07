using System;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Core.Communication;
using HTTPnet.Core.Diagnostics;
using HTTPnet.Core.Http;

namespace HTTPnet.Core
{
    public sealed class HttpServer : IDisposable
    {
        private readonly Func<HttpServerOptions, IServerSocketWrapper> _socketWrapperFactory;

        private IServerSocketWrapper _socketWrapper;
        private HttpServerOptions _options;
        private CancellationTokenSource _cancellationTokenSource;

        public HttpServer(Func<HttpServerOptions, IServerSocketWrapper> socketWrapperFactory)
        {
            _socketWrapperFactory = socketWrapperFactory ?? throw new ArgumentNullException(nameof(socketWrapperFactory));
        }
        
        public async Task StartAsync(HttpServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (_options.HttpRequestHandler == null) throw new InvalidOperationException("HTTP request handler not set.");

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _socketWrapper = _socketWrapperFactory(options);

                await _socketWrapper.StartAsync();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(() => AcceptConnectionsAsync(_cancellationTokenSource.Token).ConfigureAwait(false), _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (Exception e)
            {
                HttpNetTrace.Verbose(e.ToString());
                await StopAsync();
            }
        }

        public async Task StopAsync()
        {
            if (_socketWrapper != null)
            {
                await _socketWrapper.StopAsync();
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
                    try
                    {
                        var client = await _socketWrapper.AcceptAsync().ConfigureAwait(false);
                        HttpNetTrace.Verbose("Client '{0}' connected.", client.Identifier);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () => await HandleConnectionAsync(client).ConfigureAwait(false), _cancellationTokenSource.Token).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception e)
                    {
                        HttpNetTrace.Verbose(e.ToString());
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                HttpNetTrace.Verbose(exception.ToString());
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel(false);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _socketWrapper?.Dispose();
        }

        private async Task HandleConnectionAsync(IClientSocketWrapper clientSocket)
        {
            using (var clientSession = new ClientSession(clientSocket, this, _options))
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
                    HttpNetTrace.Verbose("Error while handling HTTP client requests. " + exception);
                }
                finally
                {
                    HttpNetTrace.Verbose("Client '{0}' disconnected.", clientSocket.Identifier);
                    await clientSocket.DisconnectAsync();
                }
            }
        }

        internal async Task HandleHttpRequestAsync(HttpContext httpContext)
        {
            var handler = _options.HttpRequestHandler;
            if (handler == null)
            {
                return;
            }

            try
            {
                await handler.HandleHttpRequestAsync(httpContext);
            }
            catch (Exception exception)
            {
                HttpNetTrace.Verbose(exception.ToString());
                await handler.HandleUnhandledExceptionAsync(httpContext, exception);
            }
        }
    }
}