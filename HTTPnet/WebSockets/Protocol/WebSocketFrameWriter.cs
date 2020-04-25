using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPnet.WebSockets.Protocol
{
    public class WebSocketFrameWriter
    {
        private readonly Stream _sendStream;

        public WebSocketFrameWriter(Stream sendStream)
        {
            _sendStream = sendStream ?? throw new ArgumentNullException(nameof(sendStream));
        }

        public async Task WriteAsync(WebSocketFrame frame, CancellationToken cancellationToken)
        {
            // https://tools.ietf.org/html/rfc6455

            var buffer = new byte[10];
            var frameSize = 2;

            if (frame.Fin)
            {
                buffer[0] |= 128;
            }

            buffer[0] |= (byte)frame.Opcode;

            if (frame.MaskingKey != 0)
            {
                buffer[1] |= 128;
            }

            var payloadLength = frame.Payload?.Length ?? 0;

            if (payloadLength > 0)
            {
                if (payloadLength <= 125)
                {
                    buffer[1] |= (byte)payloadLength;
                }
                else if (payloadLength >= 126 && payloadLength <= 65535)
                {
                    buffer[1] |= 126;
                    buffer[2] = (byte)(payloadLength >> 8);
                    buffer[3] = (byte)payloadLength;
                    frameSize = 4;
                }
                else
                {
                    buffer[1] |= 127;
                    buffer[2] = (byte)(payloadLength >> 56);
                    buffer[3] = (byte)(payloadLength >> 48);
                    buffer[4] = (byte)(payloadLength >> 40);
                    buffer[5] = (byte)(payloadLength >> 32);
                    buffer[6] = (byte)(payloadLength >> 24);
                    buffer[7] = (byte)(payloadLength >> 16);
                    buffer[8] = (byte)(payloadLength >> 8);
                    buffer[9] = (byte)payloadLength;
                    frameSize = 10;
                }
            }

            await _sendStream.WriteAsync(buffer, 0, frameSize, cancellationToken).ConfigureAwait(false);
            await _sendStream.WriteAsync(frame.Payload, 0, payloadLength, cancellationToken).ConfigureAwait(false);
            await _sendStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
