using System.IO;
using System.Threading.Tasks;
using HTTPnet.Core.Http;
using HTTPnet.Core.Http.Raw;

namespace HTTPnet.Core.Pipeline.Handlers
{
    public class RequestBodyHandler : IHttpContextPipelineHandler
    {
        public async Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
        {
            var bodyLength = 0;
            if (context.HttpContext.Request.Headers.TryGetValue(HttpHeader.ContentLength, out var v))
            {
                bodyLength = int.Parse(v);
            }
            
            if (bodyLength == 0)
            {
                context.HttpContext.Request.Body = new MemoryStream(0);
                return;
            }

            if (context.HttpContext.Request.Headers.ValueEquals(HttpHeader.Expect, "100-Continue"))
            {
                var response = new RawHttpResponse
                {
                    Version = context.HttpContext.Request.Version,
                    StatusCode = (int)HttpStatusCode.Continue
                };

                await context.HttpContext.SessionHandler.ResponseWriter.WriteAsync(response, context.HttpContext.ClientSession.CancellationToken);
            }

            while (context.HttpContext.SessionHandler.RequestReader.BufferLength < bodyLength)
            {
                await context.HttpContext.SessionHandler.RequestReader.FetchChunk(context.HttpContext.ClientSession.CancellationToken);
            }

            context.HttpContext.Request.Body = new MemoryStream(bodyLength);
            for (var i = 0; i < bodyLength; i++)
            {
                context.HttpContext.Request.Body.WriteByte(context.HttpContext.SessionHandler.RequestReader.DequeueFromBuffer());
            }
             
            context.HttpContext.Request.Body.Position = 0;
        }

        public Task ProcessResponseAsync(HttpContextPipelineHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }
}
