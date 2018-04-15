using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Communication;
using HTTPnet.WebSockets.Protocol;

namespace HTTPnet.WebSockets
{
    public sealed class WebSocketClientSessionHandler : IClientSessionHandler
    {
        private readonly List<WebSocketFrame> _frameQueue = new List<WebSocketFrame>();
        private readonly WebSocketFrameWriter _webSocketFrameWriter;
        private readonly ClientSession _clientSession;

        private CancellationToken _cancellationToken;

        public WebSocketClientSessionHandler(ClientSession clientSession)
        {
            _clientSession = clientSession ?? throw new ArgumentNullException(nameof(clientSession));

            _webSocketFrameWriter = new WebSocketFrameWriter(_clientSession.Client.SendStream);
        }

        public event EventHandler<WebSocketMessageReceivedEventArgs> MessageReceived;

        public event EventHandler Closed;

        public async Task ProcessAsync(CancellationToken token)
        {
            _cancellationToken = token;
            var webSocketFrame = await new WebSocketFrameReader(_clientSession.Client.ReceiveStream).ReadAsync(token).ConfigureAwait(false);
            switch (webSocketFrame.Opcode)
            {
                case WebSocketOpcode.Ping:
                    {
                        webSocketFrame.Opcode = WebSocketOpcode.Pong;
                        await _webSocketFrameWriter.WriteAsync(webSocketFrame, token).ConfigureAwait(false);
                        return;
                    }

                case WebSocketOpcode.ConnectionClose:
                    {
                        await CloseAsync().ConfigureAwait(false);
                        return;
                    }

                case WebSocketOpcode.Pong:
                    {
                        return;
                    }
            }

            _frameQueue.Add(webSocketFrame);

            if (webSocketFrame.Fin)
            {
                var message = GenerateMessage();
                _frameQueue.Clear();

                MessageReceived?.Invoke(this, new WebSocketMessageReceivedEventArgs(message, this));
            }
        }

        public Task CloseAsync()
        {
            _clientSession.Close();
            Closed?.Invoke(this, EventArgs.Empty);
            
            return Task.FromResult(0);
        }

        public async Task SendAsync(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            await _webSocketFrameWriter.WriteAsync(new WebSocketFrame
            {
                Opcode = WebSocketOpcode.Text,
                Payload = Encoding.UTF8.GetBytes(text)
            }, _cancellationToken).ConfigureAwait(false);
        }

        public async Task SendAsync(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            await _webSocketFrameWriter.WriteAsync(new WebSocketFrame
            {
                Opcode = WebSocketOpcode.Binary,
                Payload = data
            }, _cancellationToken).ConfigureAwait(false);
        }

        private WebSocketMessage GenerateMessage()
        {
            ValidateFrameQueue();

            var buffer = new List<byte>();
            foreach (var frame in _frameQueue)
            {
                buffer.AddRange(frame.Payload);
            }

            var messageType = _frameQueue.First().Opcode;

            if (messageType == WebSocketOpcode.Text)
            {
                var text = Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count);
                return new WebSocketTextMessage(text);
            }

            if (messageType == WebSocketOpcode.Binary)
            {
                return new WebSocketBinaryMessage(buffer.ToArray());
            }

            throw new NotSupportedException();
        }

        private void ValidateFrameQueue()
        {
            // Details: https://tools.ietf.org/html/rfc6455#section-5.6 PAGE 34
            if (!_frameQueue.Last().Fin)
            {
                throw new InvalidOperationException("Fragmented frames are invalid.");
            }

            if (_frameQueue.First().Opcode != WebSocketOpcode.Binary &&
                _frameQueue.First().Opcode != WebSocketOpcode.Text)
            {
                throw new InvalidOperationException("Frame opcode is invalid.");
            }

            if (_frameQueue.Count > 2)
            {
                for (int i = 1; i < _frameQueue.Count - 1; i++)
                {
                    if (_frameQueue[i].Opcode != WebSocketOpcode.Continuation)
                    {
                        throw new InvalidOperationException("Fragmented frame is invalid.");
                    }

                    if (_frameQueue[i].Fin)
                    {
                        throw new InvalidOperationException("Fragmented frame is invalid.");
                    }
                }
            }
        }

        public void Dispose()
        {
            _clientSession?.Dispose();
        }
    }
}
