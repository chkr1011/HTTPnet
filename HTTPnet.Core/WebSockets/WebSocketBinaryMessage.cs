using System;

namespace HTTPnet.WebSockets
{
    public class WebSocketBinaryMessage : WebSocketMessage
    {
        public WebSocketBinaryMessage(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public byte[] Data { get; }
    }
}
