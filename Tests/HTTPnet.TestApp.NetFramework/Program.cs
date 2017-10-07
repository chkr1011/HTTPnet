using HTTPnet.Core.Diagnostics;
using HTTPnet.Core.Http;
using HTTPnet.Core.Pipeline;
using HTTPnet.Core.Pipeline.Handlers;
using HTTPnet.Core.WebSockets;
using HTTPnet.TestApp.NetFramework.Processors;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            pipeline.Add(new WebSocketRequestHandler(ComputeSha1Hash, SessionCreated));
            pipeline.Add(new ResponseBodyLengthHandler());
            pipeline.Add(new ResponseCompressionHandler());
            pipeline.Add(new SimpleHttpRequestHandler());
            
            var options = new HttpServerOptions
            {
                HttpRequestHandler = pipeline
            };

            var httpServer = new HttpServerFactory().CreateHttpServer();
            httpServer.StartAsync(options).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void SessionCreated(WebSocketSession webSocketSession)
        {
            webSocketSession.MessageReceived += async (s, e) =>
            {
                Console.WriteLine(((WebSocketTextMessage) e.Message).Text);

                await webSocketSession.SendAsync("Reply...");
            };
        }

        private static byte[] ComputeSha1Hash(byte[] source)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(source);
            }
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
