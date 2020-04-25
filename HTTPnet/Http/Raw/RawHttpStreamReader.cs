using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPnet.Http.Raw
{
    public sealed class RawHttpStreamReader : IDisposable
    {
        private readonly Stream _stream;

        public RawHttpStreamReader(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public async Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            var line = new StringBuilder();

            var hasR = false;
            var hasN = false;

            var buffer = new byte[1];
            while (!hasR || !hasN)
            {
                var readBytes = await _stream.ReadAsync(buffer, 0, 1, cancellationToken).ConfigureAwait(false);
                if (readBytes == 0)
                {
                    throw new OperationCanceledException();
                }

                var @char = (char)buffer[0];
                if (@char == '\r')
                {
                    hasR = true;
                }
                else if (@char == '\n')
                {
                    hasN = true;
                }
                else
                {
                    line.Append(@char);
                }
            }

            return line.ToString();
        }

        public async Task<byte[]> ReadAsync(long length, CancellationToken cancellationToken)
        {
            var buffer = new byte[length];
            var offset = 0;

            while (offset < buffer.Length)
            {
                var bytesRead = await _stream.ReadAsync(buffer, offset, buffer.Length - offset, cancellationToken).ConfigureAwait(false);
                if (bytesRead == -1)
                {
                    throw new OperationCanceledException();
                }

                offset += bytesRead;
            }

            return buffer;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}