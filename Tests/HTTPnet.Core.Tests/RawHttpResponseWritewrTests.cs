using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using HTTPnet.Core.Http;
using HTTPnet.Core.Http.Raw;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HTTPnet.Core.Tests
{
    [TestClass]
    public class RawHttpResponseWritewrTests
    {
        [TestMethod]
        public async void Http_SerializeHttpRequest()
        {
            var response = new RawHttpResponse
            {
                StatusCode = (int) HttpStatusCode.BadRequest,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"text\":1234}")),
                Headers =
                {
                    ["A"] = 1.ToString(),
                    ["B"] = "x"
                }
            };

            var memoryStream = new MemoryStream();
            var serializer = new RawHttpResponseWriter(memoryStream, new HttpServerOptions());
            await serializer.WriteAsync(response, CancellationToken.None);
            var requiredBuffer = Convert.FromBase64String("SFRUUC8xLjEgNDAwIEJhZFJlcXVlc3QNCkE6MQ0KQjp4DQpDb250ZW50LVR5cGU6dGV4dC9wbGFpbjsgY2hhcnNldD11dGYtOA0KQ29udGVudC1MZW5ndGg6MTMNCg0KeyJ0ZXh0IjoxMjM0fQ==");

            Assert.IsTrue(memoryStream.ToArray().SequenceEqual(requiredBuffer));
        }
    }
}
