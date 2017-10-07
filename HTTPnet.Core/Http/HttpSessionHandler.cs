using System;
using System.Collections.Generic;
using System.Text;
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
            var httpRequest = await RequestReader.TryReadAsync(_clientSession.CancellationToken).ConfigureAwait(false);
            if (httpRequest == null)
            {
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

            var isWebSocketRequest = httpRequest.Headers.ValueEquals(HttpHeaderName.Upgrade, "websocket");
            if (isWebSocketRequest)
            {
                await UpgradeToWebSocketAsync(httpContext).ConfigureAwait(false);
            }
            else
            {
                await _clientSession.HandleHttpRequestAsync(httpContext);
                await ResponseWriter.WriteAsync(httpContext.Response, _clientSession.CancellationToken);

                HttpNetTrace.Verbose("Response '{0}' sent to '{1}'.", httpContext.Response.StatusCode, _clientSession.Client.Identifier);
            }

            if (httpContext.CloseConnection)
            {
                _clientSession.Close();
            }
        }
        
        private async Task UpgradeToWebSocketAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.SwitchingProtocols;
            httpContext.Response.Headers[HttpHeaderName.Connection] = "Upgrade";
            httpContext.Response.Headers[HttpHeaderName.Upgrade] = "websocket";
            httpContext.Response.Headers[HttpHeaderName.SecWebSocketAccept] = GenerateWebSocketAccept(httpContext);

            await ResponseWriter.WriteAsync(httpContext.Response, _clientSession.CancellationToken);
            _clientSession.UpgradeToWebSocketSession(httpContext.Request);
        }

        private static string GenerateWebSocketAccept(HttpContext httpContext)
        {
            var webSocketKey = httpContext.Request.Headers[HttpHeaderName.SecWebSocketKey];
            var responseKey = webSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            var responseKeyBuffer = Encoding.UTF8.GetBytes(responseKey);

            return "";

            //var sha = SHA1.Create().ComputeHash(responseKeyBuffer);
            //return Convert.ToBase64String(sha);
        }
    }
}