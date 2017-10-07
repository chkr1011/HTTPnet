using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPnet.Core.Http.Raw
{
    public class RawHttpResponseWriter
    {
        private readonly Stream _sendStream;
        private readonly HttpServerOptions _options;

        public RawHttpResponseWriter(Stream sendStream, HttpServerOptions options)
        {
            _sendStream = sendStream ?? throw new ArgumentNullException(nameof(sendStream));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task WriteAsync(RawHttpResponse response, CancellationToken cancellationToken)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            var buffer = new StringBuilder();
            buffer.AppendLine(response.Version + " " + response.StatusCode + " " + response.ReasonPhrase);

            foreach (var header in response.Headers)
            {
                buffer.AppendLine(header.Key + ":" + header.Value);
            }

            buffer.AppendLine();

            var binaryBuffer = Encoding.UTF8.GetBytes(buffer.ToString());

            await _sendStream.WriteAsync(binaryBuffer, 0, binaryBuffer.Length, cancellationToken).ConfigureAwait(false);

            if (response.Body != null && response.Body.Length > 0)
            {
                response.Body.Position = 0;
                await response.Body.CopyToAsync(_sendStream, _options.SendBufferSize, cancellationToken).ConfigureAwait(false);
            }

            await _sendStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
