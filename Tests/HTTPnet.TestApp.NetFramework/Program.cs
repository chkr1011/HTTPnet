using HTTPnet.Core.Diagnostics;
using HTTPnet.Core.Http;
using HTTPnet.Core.Pipeline;
using HTTPnet.Core.Pipeline.Handlers;
using HTTPnet.Core.WebSockets;
using System;
using System.Globalization;
using System.IO;
using System.Net;
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
            HttpNetTrace.TraceMessagePublished += (s, e) => Console.WriteLine("[" + e.Source + "] [" + e.Level + "] [" + e.Message + "] [" + e.Exception + "]");

            var pipeline = new HttpContextPipeline(new SimpleExceptionHandler());
            pipeline.Add(new RequestBodyHandler());
            pipeline.Add(new TraceHandler());
            pipeline.Add(new WebSocketRequestHandler(ComputeSha1Hash, SessionCreated));
            pipeline.Add(new ResponseBodyLengthHandler());
            pipeline.Add(new ResponseCompressionHandler());
            pipeline.Add(new SimpleHttpRequestHandler());

            var httpServer = new HttpServerFactory().CreateHttpServer();
            httpServer.RequestHandler = pipeline;
            httpServer.StartAsync(HttpServerOptions.Default).GetAwaiter().GetResult();


            Thread.Sleep(Timeout.Infinite);
        }

        private static void SessionCreated(WebSocketSession webSocketSession)
        {
            webSocketSession.MessageReceived += async (s, e) =>
            {
                Console.WriteLine(((WebSocketTextMessage)e.Message).Text);

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
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                httpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(exception.ToString()));
                httpContext.Response.Headers[HttpHeader.ContentLength] = httpContext.Response.Body.Length.ToString(CultureInfo.InvariantCulture);

                httpContext.CloseConnection = true;

                return Task.FromResult(0);
            }
        }

        public class SimpleHttpRequestHandler : IHttpContextPipelineHandler
        {
            public Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
            {
                if (context.HttpContext.Request.Uri.Equals("/404test"))
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Task.FromResult(0);
                }

                if (context.HttpContext.Request.Method.Equals(HttpMethod.Post) && context.HttpContext.Request.Uri.Equals("/toUpper"))
                {
                    var s = new StreamReader(context.HttpContext.Request.Body).ReadToEnd();
                    context.HttpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(s.ToUpperInvariant()));
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK; // OK is also default
                    return Task.FromResult(0);
                }

                var filename = "C:" + context.HttpContext.Request.Uri.Replace("/", "\\");
                if (File.Exists(filename))
                {
                    // Return a file from the filesystem.
                    context.HttpContext.Response.Body = File.OpenRead(filename);
                }
                else
                {
                    // Return a static text.
                    context.HttpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes("Hello World"));
                }

                return Task.FromResult(0);
            }

            public Task ProcessResponseAsync(HttpContextPipelineHandlerContext context)
            {
                return Task.FromResult(0);
            }
        }
    }
}
