using System;
using System.Globalization;
using System.IO;
using System.Text;
using HTTPnet.Core.Http;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Core.Diagnostics;
using HTTPnet.Core.Pipeline;
using HTTPnet.Core.Pipeline.Handlers;
using HTTPnet.TestApp.NetFramework.Processors;

namespace HTTPnet.TestApp.NetFramework
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            HttpNetTrace.TraceMessagePublished += (s, e) => Console.WriteLine(e);

            var pipeline = new HttpContextPipeline(new SimpleExceptionHandler());

            pipeline.Add(new RequestBodyHandler());
            pipeline.Add(new TraceHandler());
            pipeline.Add(new ResponseBodyLengthHandler());
            pipeline.Add(new ResponseCompressionHandler());
            pipeline.Add(new SimpleHttpRequestHandler());
            
            var options = new HttpServerOptions
            {
                RequestHandler = pipeline
            };

            var httpServer = new HttpServerFactory().CreateHttpServer();
            httpServer.StartAsync(options).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }

        public class SimpleExceptionHandler : IHttpContextPipelineExceptionHandler
        {
            public Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
            {
                httpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                httpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(exception.ToString()));
                httpContext.Response.Headers[HttpHeaderName.ContentLength] = httpContext.Response.Body.Length.ToString(CultureInfo.InvariantCulture);

                httpContext.CloseConnection = true;
                
                return Task.FromResult(0);
            }
        }

        public class SimpleHttpRequestHandler : IHttpContextPipelineHandler
        {
            public Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
            {
                var filename = "C:" + context.HttpContext.Request.Uri.Replace("/", "\\");

                context.HttpContext.Response.Body = File.OpenRead(filename);

                //var b = Encoding.UTF8.GetBytes("Hello World");
                //context.HttpContext.Response.Body.Write(b, 0, b.Length);

                return Task.FromResult(0);
            }

            public Task ProcessResponseAsync(HttpContextPipelineHandlerContext context)
            {


                return Task.FromResult(0);
            }
        }
    }
}
