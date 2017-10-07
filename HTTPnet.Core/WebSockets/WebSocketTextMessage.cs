using System;

namespace HTTPnet.Core.WebSockets
{
    public class WebSocketTextMessage : WebSocketMessage
    {
        public WebSocketTextMessage(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            Text = text;
        }

        public string Text { get; }
    }
}
