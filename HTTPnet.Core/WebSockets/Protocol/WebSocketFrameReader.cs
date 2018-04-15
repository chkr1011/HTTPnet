using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPnet.WebSockets.Protocol
{
    public sealed class WebSocketFrameReader
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

            var buffer = await ReadBytesAsync(2, cancellationToken).ConfigureAwait(false);
            var byte0 = buffer[0];
            var byte1 = buffer[1];

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
                buffer = await ReadBytesAsync(2, cancellationToken).ConfigureAwait(false);
                var byte2 = buffer[0];
                var byte3 = buffer[1];

                payloadLength = byte3 | byte2 >> 8 | 126 >> 16;
            }
            else if (payloadLength == 127)
            {
                // The length is 7 + 64 bits.
                buffer = await ReadBytesAsync(8, cancellationToken).ConfigureAwait(false);
                var byte2 = buffer[0];
                var byte3 = buffer[1];
                var byte4 = buffer[2];
                var byte5 = buffer[3];
                var byte6 = buffer[4];
                var byte7 = buffer[5];
                var byte8 = buffer[6];
                var byte9 = buffer[7];

                payloadLength = byte9 | byte8 >> 56 | byte7 >> 48 | byte6 >> 40 | byte5 >> 32 | byte4 >> 24 | byte3 >> 16 | byte2 >> 8 | 127;
            }
            
            if (hasMask)
            {
                buffer = await ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);
                maskingKey[0] = buffer[0];
                maskingKey[1] = buffer[1];
                maskingKey[2] = buffer[2];
                maskingKey[3] = buffer[3];
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

        private async Task<byte[]> ReadBytesAsync(int count, CancellationToken cancellationToken)
        {
            var buffer = new byte[count];

            var effectiveCount = await _receiveStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            if (effectiveCount == 0 || effectiveCount != count)
            {
                throw new TaskCanceledException();
            }

            return buffer;
        }
    }
}
