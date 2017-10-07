using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPnet.Core.WebSockets.Protocol
{
    public class WebSocketFrameReader
    {
        private readonly Stream _receiveStream;

        public WebSocketFrameReader(Stream receiveStream)
        {
            _receiveStream = receiveStream ?? throw new ArgumentNullException(nameof(receiveStream));
        }

        public async Task<WebSocketFrame> ReadAsync(CancellationToken cancellationToken)
        {
            // https://tools.ietf.org/html/rfc6455

            var webSocketFrame = new WebSocketFrame();

            var byte0 = await ReadByteAsync(cancellationToken);
            var byte1 = await ReadByteAsync(cancellationToken);

            if ((byte0 & 128) == 128)
            {
                webSocketFrame.Fin = true;
                byte0 = (byte)(127 & byte0);
            }

            webSocketFrame.Opcode = (WebSocketOpcode)byte0;

            var hasMask = (byte1 & 128) == 128;
            var maskingKey = new byte[4];

            var payloadLength = byte1 & 127;
            if (payloadLength == 126)
            {
                // The length is 7 + 16 bits.
                var byte2 = await ReadByteAsync(cancellationToken);
                var byte3 = await ReadByteAsync(cancellationToken);

                payloadLength = byte3 | byte2 >> 8 | 126 >> 16;
            }
            else if (payloadLength == 127)
            {
                // The length is 7 + 64 bits.
                var byte2 = await ReadByteAsync(cancellationToken);
                var byte3 = await ReadByteAsync(cancellationToken);
                var byte4 = await ReadByteAsync(cancellationToken);
                var byte5 = await ReadByteAsync(cancellationToken);
                var byte6 = await ReadByteAsync(cancellationToken);
                var byte7 = await ReadByteAsync(cancellationToken);
                var byte8 = await ReadByteAsync(cancellationToken);
                var byte9 = await ReadByteAsync(cancellationToken);

                payloadLength = byte9 | byte8 >> 56 | byte7 >> 48 | byte6 >> 40 | byte5 >> 32 | byte4 >> 24 | byte3 >> 16 | byte2 >> 8 | 127;
            }
            
            if (hasMask)
            {
                maskingKey[0] = await ReadByteAsync(cancellationToken);
                maskingKey[1] = await ReadByteAsync(cancellationToken);
                maskingKey[2] = await ReadByteAsync(cancellationToken);
                maskingKey[3] = await ReadByteAsync(cancellationToken);
            }

            webSocketFrame.MaskingKey = BitConverter.ToUInt32(maskingKey, 0);

            webSocketFrame.Payload = new byte[payloadLength];
            if (payloadLength > 0)
            {
                await _receiveStream.ReadAsync(webSocketFrame.Payload, 0, webSocketFrame.Payload.Length, cancellationToken).ConfigureAwait(false);
            }

            if (hasMask)
            {
                for (var i = 0; i < webSocketFrame.Payload.Length; i++)
                {
                    webSocketFrame.Payload[i] = (byte)(webSocketFrame.Payload[i] ^ maskingKey[i % 4]);
                }
            }

            return webSocketFrame;
        }

        private async Task<byte> ReadByteAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[1];
            var count = await _receiveStream.ReadAsync(buffer, 0, 1, cancellationToken);
            if (count == 0)
            {
                throw new TaskCanceledException();
            }

            return buffer[0];
        }
    }
}
