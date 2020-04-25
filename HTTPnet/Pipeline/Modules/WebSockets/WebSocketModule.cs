using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HTTPnet.Http;
using HTTPnet.WebSockets;

namespace HTTPnet.Pipeline.Modules.WebSockets
{
    public class WebSocketModule : IHttpRequestPipelineModule
    {
        private readonly Action<WebSocketClientSessionHandler> _sessionCreatedCallback;
        
        public WebSocketModule(Action<WebSocketClientSessionHandler> sessionCreatedCallback)
        {
            _sessionCreatedCallback = sessionCreatedCallback ?? throw new ArgumentNullException(nameof(sessionCreatedCallback));
        }

        public Task ProcessRequestAsync(HttpRequestPipelineModuleContext context)
        {
            var isWebSocketRequest = context.HttpContext.Request.Headers.ValueEquals(HttpHeader.Upgrade, "websocket");
            if (!isWebSocketRequest)
            {
                return Task.FromResult(0);
            }

            var webSocketKey = context.HttpContext.Request.Headers[HttpHeader.SecWebSocketKey];
            var responseKey = webSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            var responseKeyBuffer = Encoding.UTF8.GetBytes(responseKey);

            var hash = ComputeHash(responseKeyBuffer);
            var secWebSocketAccept = Convert.ToBase64String(hash);

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.SwitchingProtocols;
            context.HttpContext.Response.Headers[HttpHeader.Connection] = "Upgrade";
            context.HttpContext.Response.Headers[HttpHeader.Upgrade] = "websocket";
            context.HttpContext.Response.Headers[HttpHeader.SecWebSocketAccept] = secWebSocketAccept;

            var webSocketSession = new WebSocketClientSessionHandler(context.HttpContext.ClientSession);
            context.HttpContext.ClientSession.SwitchProtocol(webSocketSession);

            _sessionCreatedCallback(webSocketSession);

            context.BreakPipeline = true;

            return Task.FromResult(0);
        }

        public Task ProcessResponseAsync(HttpRequestPipelineModuleContext context)
        {
            return Task.FromResult(0);
        }

        private static byte[] ComputeHash(byte[] buffer)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(buffer);
            }
        }
    }
}
