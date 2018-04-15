using System;

namespace HTTPnet.WebSockets
{
    public class WebSocketMessageReceivedEventArgs : EventArgs
    {
        public WebSocketMessageReceivedEventArgs(WebSocketMessage message, WebSocketClientSessionHandler webSocketClientSession)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            WebSocketClientSession = webSocketClientSession ?? throw new ArgumentNullException(nameof(webSocketClientSession));
        }

        public WebSocketClientSessionHandler WebSocketClientSession { get; }

        public WebSocketMessage Message { get; }
    }
}
