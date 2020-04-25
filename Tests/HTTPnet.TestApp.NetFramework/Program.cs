using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HTTPnet.Diagnostics;
using HTTPnet.Http;
using HTTPnet.Pipeline;
using HTTPnet.Pipeline.Modules;
using HTTPnet.Pipeline.Modules.Rest;
using HTTPnet.Pipeline.Modules.StaticFiles;
using HTTPnet.Pipeline.Modules.WebSockets;
using HTTPnet.WebSockets;

namespace HTTPnet.TestApp.NetFramework
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("HTTPnet test app");
            Console.WriteLine("1 - Start server");
            Console.WriteLine("2 - Send Expect 100-Continue request");
            if (Console.ReadKey(true).KeyChar == '1')
            {
                HttpNetTrace.TraceMessagePublished += (s, e) => Console.WriteLine("[" + e.Source + "] [" + e.Level + "] [" + e.Message + "] [" + e.Exception + "]");


                var restModule = new RestModule("api");
                restModule.RegisterController(new TestController());


                var pipeline = new HttpRequestPipeline(new SimpleExceptionHandler());
                pipeline.Add(new TraceModule());
                pipeline.Add(new WebSocketModule(SessionCreated));
                pipeline.Add(new StaticFilesModule("app", new PhysicalStaticFilesStorage(".\\wwwroot"), new DefaultMimeTypeDetector()));
                pipeline.Add(restModule);

                var httpServer = new HttpServerFactory().CreateHttpServer();
                httpServer.RequestHandler = pipeline;
                httpServer.StartAsync(HttpServerOptions.Default).GetAwaiter().GetResult();


                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                var webRequest = WebRequest.CreateHttp("http://localhost/hello");
                webRequest.Method = "POST";
                var stream = webRequest.GetRequestStream();

                var buffer = Encoding.ASCII.GetBytes("Hans");
                stream.Write(buffer, 0, buffer.Length);
                var response = webRequest.GetResponse();

                Console.WriteLine(new StreamReader(response.GetResponseStream()).ReadToEnd());
            }



        }

        private static void SessionCreated(WebSocketClientSessionHandler webSocketSession)
        {
            webSocketSession.MessageReceived += async (s, e) =>
            {
                Console.WriteLine(((WebSocketTextMessage)e.Message).Text);

                await webSocketSession.SendAsync("Reply...");
            };
        }

        public class SimpleExceptionHandler : IHttpRequestPipelineExceptionHandler
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

        public class SimpleHttpRequestHandler : IHttpRequestPipelineModule
        {
            public Task ProcessRequestAsync(HttpRequestPipelineModuleContext context)
            {
                try
                {
                    if (context.HttpContext.Request.Uri.Equals("/hello"))
                    {
                        var c = new StreamReader(context.HttpContext.Request.Body).ReadToEnd();

                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.HttpContext.Response.Body = new MemoryStream(Encoding.ASCII.GetBytes("Hello " + c));
                        context.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
                        return Task.FromResult(0);
                    }

                    if (context.HttpContext.Request.Uri.Equals("/404test"))
                    {
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return Task.FromResult(0);
                    }

                    if (context.HttpContext.Request.Method.Equals(HttpMethod.Post) &&
                        context.HttpContext.Request.Uri.Equals("/toUpper"))
                    {
                        var s = new StreamReader(context.HttpContext.Request.Body).ReadToEnd();
                        context.HttpContext.Response.Body =
                            new MemoryStream(Encoding.UTF8.GetBytes(s.ToUpperInvariant()));
                        context.HttpContext.Response.StatusCode = (int) HttpStatusCode.OK; // OK is also default
                        return Task.FromResult(0);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return Task.FromResult(0);
            }

            public Task ProcessResponseAsync(HttpRequestPipelineModuleContext context)
            {
                return Task.FromResult(0);
            }
        }
    }

    public class TestController
    {
        public void Ping()
        {

        }
    }
}
