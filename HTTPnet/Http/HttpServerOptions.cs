using System.IO.Compression;
using System.Net;

namespace HTTPnet.Http
{
    public class HttpServerOptions
    {
        public static HttpServerOptions Default => new HttpServerOptions();

        public IPAddress BoundIPAddress { get; set; } = IPAddress.Any;
            
        public int Port { get; set; } = 80;
        
        public int Backlog { get; set; } = 10;

        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Fastest;
    }
}
