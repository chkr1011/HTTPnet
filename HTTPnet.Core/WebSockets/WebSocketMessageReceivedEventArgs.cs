using System;

namespace HTTPnet.Core.WebSockets
{
    public class WebSocketMessageReceivedEventArgs : EventArgs
    {
        public WebSocketMessageReceivedEventArgs(WebSocketMessage message, WebSocketSession webSocketSession)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            WebSocketSession = webSocketSession ?? throw new ArgumentNullException(nameof(webSocketSession));
        }

        public WebSocketSession WebSocketSession { get; }

        public WebSocketMessage Message { get; }
    }
}
