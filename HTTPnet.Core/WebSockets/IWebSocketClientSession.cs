using System;
using System.Threading.Tasks;

namespace HTTPnet.Core.WebSockets
{
    public interface IWebSocketClientSession
    {
        event EventHandler Closed;

        event EventHandler<WebSocketMessageReceivedEventArgs> MessageReceived;

        Task SendAsync(string text);

        Task SendAsync(byte[] data);

        Task CloseAsync();
    }
}
