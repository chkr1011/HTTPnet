using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Http.Raw;

namespace HTTPnet.Http.Streams
{
    public sealed class HttpExpectContinueStream : Stream // TODO: Build basic HttpContentStream which cannot seek!
    {
        private readonly RawHttpRequest _request;
        private readonly RawHttpRequestReader _requestReader;
        private readonly RawHttpResponseWriter _responseWriter;

        private bool _responseSent;
        private long _position;

        public HttpExpectContinueStream(RawHttpRequest request, RawHttpRequestReader requestReader, RawHttpResponseWriter responseWriter)
        {
            _request = request;
            _requestReader = requestReader;
            _responseWriter = responseWriter;

            Length = request.Headers.GetContentLength();
        }

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite { get; } = false;

        public override long Length { get; }

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_position == Length)
            {
                return 0;
            }

            if (!_responseSent)
            {
                var response = new RawHttpResponse
                {
                    Version = _request.Version,
                    StatusCode = (int)HttpStatusCode.Continue
                };

                await _responseWriter.WriteAsync(response, cancellationToken).ConfigureAwait(false);
                _responseSent = true;
            }

            var bytesRead = await _requestReader.ReadAsync(buffer, 0, count, cancellationToken).ConfigureAwait(false);
            _position += bytesRead;
            return bytesRead;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}