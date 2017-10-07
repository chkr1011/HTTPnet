using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Core.Exceptions;

namespace HTTPnet.Core.Http.Raw
{
    public sealed class RawHttpRequestReader
    {
        private readonly Stream _receiveStream;
        private readonly Queue<byte> _buffer = new Queue<byte>();
        private readonly byte[] _chunkBuffer;

        public RawHttpRequestReader(Stream receiveStream, HttpServerOptions options)
        {
            _receiveStream = receiveStream ?? throw new ArgumentNullException(nameof(receiveStream));
            if (options == null) throw new ArgumentNullException(nameof(options));

            _chunkBuffer = new byte[options.ReceiveChunkSize];
        }

        public int BufferLength => _buffer.Count;

        public async Task<RawHttpRequest> ReadAsync(CancellationToken cancellationToken)
        {
            await FetchChunk(cancellationToken).ConfigureAwait(false);

            var statusLine = ReadLine();
            var statusLineItems = statusLine.Split(' ');

            if (statusLineItems.Length != 3)
            {
                throw new HttpRequestInvalidException();
            }

            var request = new RawHttpRequest
            {
                Method = statusLineItems[0].ToUpperInvariant(),
                Uri = statusLineItems[1],
                Version = statusLineItems[2].ToUpperInvariant(),
                Headers = ParseHeaders()
            };

            return request;
        }

        public async Task FetchChunk(CancellationToken cancellationToken)
        {
            var size = await _receiveStream.ReadAsync(_chunkBuffer, 0, _chunkBuffer.Length, cancellationToken);
            if (size == 0)
            {
                throw new TaskCanceledException();
            }

            for (var i = 0; i < size; i++)
            {
                _buffer.Enqueue(_chunkBuffer[i]);
            }
        }

        public byte DequeueFromBuffer()
        {
            return _buffer.Dequeue();
        }

        private Dictionary<string, string> ParseHeaders()
        {
            var headers = new Dictionary<string, string>();

            var line = ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                var header = ParseHeader(line);
                headers.Add(header.Key, header.Value);

                line = ReadLine();
            }

            return headers;
        }

        private static KeyValuePair<string, string> ParseHeader(string source)
        {
            var delimiterIndex = source.IndexOf(':');
            if (delimiterIndex == -1)
            {
                return new KeyValuePair<string, string>(source, null);
            }

            var name = source.Substring(0, delimiterIndex).Trim();
            var value = source.Substring(delimiterIndex + 1).Trim();

            return new KeyValuePair<string, string>(name, value);
        }
        
        private string ReadLine()
        {
            var buffer = new StringBuilder();

            while (_buffer.Count > 0)
            {
                var @char = (char)_buffer.Dequeue();

                if (@char == '\r')
                {
                    @char = (char)_buffer.Dequeue();
                    if (@char != '\n')
                    {
                        throw new HttpRequestInvalidException();
                    }

                    return buffer.ToString();
                }

                buffer.Append(@char);
            }

            throw new HttpRequestInvalidException();
        }
    }
}