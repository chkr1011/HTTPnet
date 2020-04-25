using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Communication;
using HTTPnet.Diagnostics;
using HTTPnet.Http.Raw;
using HTTPnet.Http.Streams;

namespace HTTPnet.Http
{
    public sealed class HttpClientSessionHandler : IClientSessionHandler
    {
        private readonly ClientSession _clientSession;
        private readonly HttpServerOptions _options;
        private readonly RawHttpRequestReader _requestReader;
        private readonly RawHttpResponseWriter _responseWriter;

        public HttpClientSessionHandler(ClientSession clientSession, HttpServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _clientSession = clientSession ?? throw new ArgumentNullException(nameof(clientSession));

            _requestReader = new RawHttpRequestReader(clientSession.Client.ReceiveStream);
            _responseWriter = new RawHttpResponseWriter(clientSession.Client.SendStream, options);
        }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            RawHttpRequest httpRequest;
            try
            {
                httpRequest = await _requestReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                if (!(exception is OperationCanceledException))
                {
                    HttpNetTrace.Error(nameof(HttpClientSessionHandler), exception, "Unhandled exception while processing HTTP request.");
                }

                _clientSession.Close();
                return;
            }

            if (httpRequest.Headers.HasExpectsContinue())
            {
                httpRequest.Body = new HttpExpectContinueStream(httpRequest, _requestReader, _responseWriter);
            }

            var httpResponse = new RawHttpResponse
            {
                Version = httpRequest.Version,
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                StatusCode = (int)HttpStatusCode.BadRequest
            };

            try
            {
                var httpContext = new HttpContext(httpRequest, httpResponse, _clientSession);
                await _clientSession.HandleHttpRequestAsync(httpContext).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (httpContext.Response != null)
                {
                    if (httpContext.Response.Body != null && httpContext.CompressResponseIfSupported && httpContext.Request.Headers.SupportsGzipCompression())
                    {
                        httpContext.Response.Headers[HttpHeader.ContentEncoding] = "gzip";
                        httpContext.Response.Body = await CompressAsync(httpContext.Response.Body).ConfigureAwait(false);
                        httpContext.Response.Headers[HttpHeader.ContentLength] = httpContext.Response.Body.Length.ToString();
                    }

                    await _responseWriter.WriteAsync(httpContext.Response, cancellationToken).ConfigureAwait(false);
                }

                if (httpContext.CloseConnection || !httpContext.Request.Headers.HasConnectionKeepAlive())
                {
                    _clientSession.Close();
                }
            }
            finally
            {
                httpResponse.Body?.Dispose();
            }
        }

        public void Dispose()
        {
            _clientSession?.Dispose();
            _requestReader?.Dispose();
        }

        private async Task<Stream> CompressAsync(Stream source)
        {
            var compressedStream = new MemoryStream();
            using (var zipStream = new GZipStream(compressedStream, _options.CompressionLevel, true))
            {
                await source.CopyToAsync(zipStream).ConfigureAwait(false);
            }

            compressedStream.Position = 0;
            return compressedStream;
        }
    }
}