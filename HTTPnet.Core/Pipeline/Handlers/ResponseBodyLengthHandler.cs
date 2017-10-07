using System.Globalization;
using System.Threading.Tasks;
using HTTPnet.Core.Http;

namespace HTTPnet.Core.Pipeline.Handlers
{
    public class ResponseBodyLengthHandler : IHttpContextPipelineHandler
    {
        public Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
        {
            return Task.FromResult(0);
        }

        public Task ProcessResponseAsync(HttpContextPipelineHandlerContext context)
        {
            var bodyLength = context.HttpContext.Response.Body?.Length ?? 0;
            context.HttpContext.Response.Headers[HttpHeaderName.ContentLength] = bodyLength.ToString(CultureInfo.InvariantCulture);

            return Task.FromResult(0);
        }
    }
}
