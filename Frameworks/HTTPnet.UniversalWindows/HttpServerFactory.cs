using HTTPnet.Core;
using HTTPnet.Implementations;

namespace HTTPnet
{
    public class HttpServerFactory
    {
        public HttpServer CreateHttpServer()
        {
            return new HttpServer(o => new ServerSocketWrapper(o));
        }
    }
}
