using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HTTPnet.Core.Communication;
using HTTPnet.Core.Diagnostics;
using HTTPnet.Core.Http.Raw;

namespace HTTPnet.Core.Http
{
    public sealed class HttpSessionHandler : ISessionHandler
    {
        private readonly ClientSession _clientSession;
        private readonly HttpServerOptions _options;
        
        public HttpSessionHandler(ClientSession clientSession, HttpServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _clientSession = clientSession ?? throw new ArgumentNullException(nameof(clientSession));

            RequestReader = new RawHttpRequestReader(clientSession.Client.ReceiveStream, options);
            ResponseWriter = new RawHttpResponseWriter(clientSession.Client.SendStream, options);
        }

        public RawHttpRequestReader RequestReader { get; }

        public RawHttpResponseWriter ResponseWriter { get; }

        public async Task ProcessAsync()
        {
            RawHttpRequest httpRequest;
            try
            {
                httpRequest = await RequestReader.ReadAsync(_clientSession.CancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _clientSession.Close();
                return;
            }
            catch (Exception exception)
            {
                HttpNetTrace.Verbose(exception.ToString());
                _clientSession.Close();
                return;
            }
            
            var httpResponse = new RawHttpResponse
            {
                Version = httpRequest.Version,
                Headers = new Dictionary<string, string>(),
                StatusCode = 404
            };

            var httpContext = new HttpContext(httpRequest, httpResponse, _clientSession, this);

            await _clientSession.HandleHttpRequestAsync(httpContext);

            if (httpContext.Response != null)
            {
                await ResponseWriter.WriteAsync(httpContext.Response, _clientSession.CancellationToken);
                HttpNetTrace.Verbose("Response '{0}' sent to '{1}'.", httpContext.Response.StatusCode, _clientSession.Client.Identifier);
            }
            
            if (httpContext.CloseConnection)
            {
                _clientSession.Close();
            }
        }
    }
}