using System.Threading.Tasks;
using HTTPnet.Diagnostics;

namespace HTTPnet.Pipeline.Modules
{
    public class TraceModule : IHttpRequestPipelineModule
    {
        public Task ProcessRequestAsync(HttpRequestPipelineModuleContext context)
        {
            //var body = "<no body>";

            //var memStream = new MemoryStream();

            //if (context.HttpContext.Request.Body != null)
            //{
            //    context.HttpContext.Request.Body.CopyToAsync(memStream);
            //    memStream.Position = 0;

            //    using (var streamReader = new StreamReader(memStream, Encoding.UTF8, false, 1024, true))
            //    {
            //        body = streamReader.ReadToEnd();
            //    }

            //    memStream.Position = 0;
            //    context.HttpContext.Request.Body = memStream;
            //}

            HttpNetTrace.Verbose(nameof(TraceModule), "IN: " + context.HttpContext.Request.Method + " " + context.HttpContext.Request.Uri);
            return Task.FromResult(0);
        }

        public Task ProcessResponseAsync(HttpRequestPipelineModuleContext context)
        {
            HttpNetTrace.Verbose(nameof(TraceModule), "OUT: " + context.HttpContext.Response.StatusCode + " " + context.HttpContext.Response.ReasonPhrase);
            return Task.FromResult(0);
        }
    }
}
