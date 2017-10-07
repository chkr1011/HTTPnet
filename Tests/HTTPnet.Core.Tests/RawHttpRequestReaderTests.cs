using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using HTTPnet.Core.Http;
using HTTPnet.Core.Http.Raw;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HTTPnet.Core.Tests
{
    [TestClass]
    public class RawHttpRequestReaderTests
    {
        [TestMethod]
        public async void HttpRequestReader_ParseWithoutContentLength()
        {
            var buffer = new MemoryStream(Encoding.UTF8.GetBytes(GetRequestTextWithoutContentLength())) { Position = 0 };
            var parser = new RawHttpRequestReader(buffer, new HttpServerOptions());

            var request = await parser.ReadAsync(CancellationToken.None);
            Assert.IsTrue(request != null, "Parse failed.");
            Assert.AreEqual(HttpMethod.Delete, request.Method);
            Assert.AreEqual("/Uri%20/lalalo323/_/-/+/%/@/&/./~/:/#/;/,/*", request.Uri);
            Assert.AreEqual("Body123{}%!(:<>=", Encoding.UTF8.GetString(StreamToArray(request.Body)));
            Assert.AreEqual(HttpVersion.Version1_1, request.Version);
            Assert.AreEqual("localhost:2400", request.Headers["Host"]);
            Assert.AreEqual("keep-alive", request.Headers["Connection"]);
            Assert.AreEqual("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8", request.Headers["Accept"]);
            Assert.AreEqual("1", request.Headers["Upgrade-Insecure-Requests"]);
            Assert.AreEqual("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36", request.Headers["User-Agent"]);
            Assert.AreEqual("gzip, deflate, sdch", request.Headers["Accept-Encoding"]);
            Assert.AreEqual("de,en-US;q=0.8,en;q=0.6,de-DE;q=0.4", request.Headers["Accept-Language"]);
        }

        [TestMethod]
        public async void HttpRequestReader_ParseWithContentLength()
        {
            var buffer = new MemoryStream(Encoding.UTF8.GetBytes(GetRequestTextWithContentLength())) { Position = 0 };
            var parser = new RawHttpRequestReader(buffer, new HttpServerOptions());

            var request = await parser.ReadAsync(CancellationToken.None);

            Assert.IsTrue(request != null, "Parse failed.");
            Assert.AreEqual("Body123{}%!(:<>=", Encoding.UTF8.GetString(StreamToArray(request.Body)));
        }

        private byte[] StreamToArray(Stream source)
        {
            var buffer = new byte[source.Length];
            source.Read(buffer, 0, buffer.Length);

            return buffer;
        }
        
        private static string GetRequestTextWithContentLength()
        {
            return @"GET /Uri%20/lalalo323/_/-/+/%/@/&/./~/:/#/;/,/* HTTP/1.1
Host: localhost:2400
Content-Length: 16
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Upgrade-Insecure-Requests: 1

Body123{}%!(:<>=";
        }

        private static string GetRequestTextWithoutContentLength()
        {
            return @"DELETE /Uri%20/lalalo323/_/-/+/%/@/&/./~/:/#/;/,/* HTTP/1.1
Host: localhost:2400
Connection: keep-alive
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36
Accept-Encoding: gzip, deflate, sdch
Accept-Language: de,en-US;q=0.8,en;q=0.6,de-DE;q=0.4

Body123{}%!(:<>=";
        }
    }
}
