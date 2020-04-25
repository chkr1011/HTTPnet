using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Exceptions;

namespace HTTPnet.Http.Raw
{
    public sealed class RawHttpRequestReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly RawHttpStreamReader _reader;

        public RawHttpRequestReader(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            _reader = new RawHttpStreamReader(stream);
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public async Task<RawHttpRequest> ReadAsync(CancellationToken cancellationToken)
        {
            var requestLine = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

            var requestLineParts = requestLine.Split(' ');

            if (requestLineParts.Length != 3)
            {
                throw new HttpRequestInvalidException();
            }

            var request = new RawHttpRequest
            {
                Method = requestLineParts[0].ToUpperInvariant(),
                Uri = new Uri("http://" + "localhost" + requestLineParts[1]),
                Version = ParseVersion(requestLineParts[2].ToUpperInvariant()),
                Headers = await ReadHeadersAsync(cancellationToken).ConfigureAwait(false),
            };

            if (request.Headers.HasExpectsContinue())
            {
                return request;
            }

            var contentLength = request.Headers.GetContentLength();
            if (contentLength == 0)
            {
                request.Body = new MemoryStream(0);
                return request;
            }
            
            request.Body = new MemoryStream(await _reader.ReadAsync(contentLength, cancellationToken).ConfigureAwait(false));

            return request;
        }

        private async Task<Dictionary<string, string>> ReadHeadersAsync(CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            while (!string.IsNullOrEmpty(line))
            {
                var header = ParseHeader(line);
                headers.Add(header.Key, header.Value);

                line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
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

        private static Version ParseVersion(string source)
        {
            if (string.Equals(source, "HTTP/1.0"))
            {
                return HttpVersion.Version1;
            }

            if (string.Equals(source, "HTTP/1.1"))
            {
                return HttpVersion.Version1_1;
            }

            throw new NotSupportedException($"HTTP version '{source}' is not supported.");
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}