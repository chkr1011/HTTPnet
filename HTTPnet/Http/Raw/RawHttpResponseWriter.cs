using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPnet.Http.Raw
{
    public class RawHttpResponseWriter
    {
        private readonly Stream _stream;
        private readonly HttpServerOptions _options;

        public RawHttpResponseWriter(Stream stream, HttpServerOptions options)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task WriteAsync(RawHttpResponse response, CancellationToken cancellationToken)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            var buffer = new StringBuilder();
            buffer.Append(response.Version == HttpVersion.Version1_1 ? "HTTP/1.1" : "HTTP/1.0");

            buffer.AppendLine(" " + response.StatusCode + " " + response.ReasonPhrase);

            if (response.Headers != null)
            {
                foreach (var header in response.Headers)
                {
                    buffer.AppendLine(header.Key + ":" + header.Value);
                }
            }

            buffer.AppendLine();

            var binaryBuffer = Encoding.ASCII.GetBytes(buffer.ToString());
            await _stream.WriteAsync(binaryBuffer, 0, binaryBuffer.Length, cancellationToken).ConfigureAwait(false);
            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);

            if (response.Body != null && response.Body.Length > 0)
            {
                response.Body.Position = 0;
                await response.Body.CopyToAsync(_stream, 81920, cancellationToken).ConfigureAwait(false);
            }

            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
