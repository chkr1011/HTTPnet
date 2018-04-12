using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HTTPnet.Core.Http;
using HTTPnet.Core.Http.Raw;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace HTTPnet.Core.Tests
{
    [TestClass]
    public class RawHttpResponseWriterTests
    {
        [TestMethod]
        public void Http_SerializeHttpRequest()
        {
            var response = new RawHttpResponse
            {
                StatusCode = (int) HttpStatusCode.BadRequest,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"text\":1234}")),
                Headers = new Dictionary<string, string>
                {
                    ["A"] = 1.ToString(),
                    ["B"] = "x"
                }
            };

            var memoryStream = new MemoryStream();
            var serializer = new RawHttpResponseWriter(memoryStream, HttpServerOptions.Default);
            serializer.WriteAsync(response, CancellationToken.None).Wait();

            var requiredBuffer = Convert.FromBase64String("SFRUUC8xLjEgNDAwIEJhZFJlcXVlc3QNCkE6MQ0KQjp4DQpDb250ZW50LVR5cGU6dGV4dC9wbGFpbjsgY2hhcnNldD11dGYtOA0KQ29udGVudC1MZW5ndGg6MTMNCg0KeyJ0ZXh0IjoxMjM0fQ==");
            Assert.IsTrue(memoryStream.ToArray().SequenceEqual(requiredBuffer));
        }
    }
}
