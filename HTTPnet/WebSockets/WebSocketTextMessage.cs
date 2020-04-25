using System;

namespace HTTPnet.WebSockets
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
