using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using HTTPnet.Core.Http;

namespace HTTPnet.Core.Pipeline.Handlers
{
    public class ResponseCompressionHandler : IHttpContextPipelineHandler
    {
        public Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
        {
            return Task.FromResult(0);
        }

        public async Task ProcessResponseAsync(HttpContextPipelineHandlerContext context)
        {
            if (context.HttpContext.Response.Body == null || context.HttpContext.Response.Body.Length == 0)
            {
                return;
            }

            if (!ClientSupportsGzipCompression(context.HttpContext.Request.Headers))
            {
                return;
            }

            context.HttpContext.Response.Headers[HttpHeader.ContentEncoding] = "gzip";

            var compressedBody = new MemoryStream();
            using (var zipStream = new GZipStream(compressedBody, CompressionLevel.Fastest, true))
            {
                context.HttpContext.Response.Body.Position = 0;
                await context.HttpContext.Response.Body.CopyToAsync(zipStream);
            }

            compressedBody.Position = 0;
            context.HttpContext.Response.Body = compressedBody;
        }

        private static bool ClientSupportsGzipCompression(Dictionary<string, string> headers)
        {
            if (headers.TryGetValue(HttpHeader.AcceptEncoding, out var headerValue))
            {
                return headerValue.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) > -1;
            }

            return false;
        }
    }
}
