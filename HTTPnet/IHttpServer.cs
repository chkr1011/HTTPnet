using System;
using System.Threading.Tasks;
using HTTPnet.Http;

namespace HTTPnet
{
    public interface IHttpServer : IDisposable
    {
        IHttpRequestHandler RequestHandler { get; set; }

        Task StartAsync(HttpServerOptions options);
        Task StopAsync();
    }
}