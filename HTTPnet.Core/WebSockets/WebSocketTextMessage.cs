using System;

namespace HTTPnet.Core.WebSockets
{
    public class WebSocketTextMessage : WebSocketMessage
    {
        public WebSocketTextMessage(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string Text { get; }
    }
}
