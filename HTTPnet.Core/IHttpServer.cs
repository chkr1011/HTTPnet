using System;
using System.Threading.Tasks;
using HTTPnet.Core.Http;

namespace HTTPnet.Core
{
    public interface IHttpServer : IDisposable
    {
        IHttpRequestHandler RequestHandler { get; set; }

        Task StartAsync(HttpServerOptions options);
        Task StopAsync();
    }
}