using System;
using HTTPnet.Core.Http;
using HTTPnet.Core.Http.Raw;

namespace HTTPnet.Core.WebSockets
{
    public class WebSocketConnectedEventArgs : EventArgs
    {
        public WebSocketConnectedEventArgs(RawHttpRequest httpRequest, WebSocketSession webSocketSession)
        {
            HttpRequest = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
            WebSocketSession = webSocketSession ?? throw new ArgumentNullException(nameof(webSocketSession));
        }

        public RawHttpRequest HttpRequest { get; set; }

        public WebSocketSession WebSocketSession { get; }

        public bool IsHandled { get; set; }
    }
}
