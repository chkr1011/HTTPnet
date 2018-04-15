using System;
using System.IO;
using System.Text;
using System.Threading;
using HTTPnet.WebSockets.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HTTPnet.Core.Tests
{
    [TestClass]
    public class WebSocketFrameTests
    {
        [TestMethod]
        public void WebSocketFrame_Simple()
        {
            var payload = "{\r\n  \"Hello\": \"World\"\r\n}";
            var payloadBuffer = Encoding.UTF8.GetBytes(payload);
            var webSocketFrame = new WebSocketFrame { Opcode = WebSocketOpcode.Binary, Payload = payloadBuffer };

            var memoryStream = new MemoryStream();
            new WebSocketFrameWriter(memoryStream).WriteAsync(webSocketFrame, CancellationToken.None).Wait();
            var result = memoryStream.ToArray();

            var expected = Convert.FromBase64String("ghh7DQogICJIZWxsbyI6ICJXb3JsZCINCn0=");

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void WebSocketFrame_LargePayload()
        {
            var payload = "{\r\n  \"Hello12121212121212121212121212121212121212121212121212121AAAAAAAAAAAAAAA\": \"World56565656565656565656565656565656565656565656565BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBCCCCCCCCCC\"\r\n}";
            var payloadBuffer = Encoding.UTF8.GetBytes(payload.ToString());
            var webSocketFrame = new WebSocketFrame { Opcode = WebSocketOpcode.Binary, Payload = payloadBuffer };

            var memoryStream = new MemoryStream();
            new WebSocketFrameWriter(memoryStream).WriteAsync(webSocketFrame, CancellationToken.None).Wait();
            var result = memoryStream.ToArray();

            var expected = Convert.FromBase64String("gn4As3sNCiAgIkhlbGxvMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjEyMTIxMjFBQUFBQUFBQUFBQUFBQUEiOiAiV29ybGQ1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NTY1NjU2NUJCQkJCQkJCQkJCQkJCQkJCQkJCQkJCQkJCQkJCQkNDQ0NDQ0NDQ0MiDQp9");

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void WebSocketFrame_Parse()
        {
            var payload = "{\"Hello\":\"World\"}";
            var payloadBuffer = Encoding.UTF8.GetBytes(payload);
            var sourceWebSocketFrame = new WebSocketFrame { Opcode = WebSocketOpcode.Binary, Payload = payloadBuffer };
            sourceWebSocketFrame.Opcode = WebSocketOpcode.Ping;

            var memoryStream = new MemoryStream();
            new WebSocketFrameWriter(memoryStream).WriteAsync(sourceWebSocketFrame, CancellationToken.None).Wait();

            memoryStream.Position = 0;
            var targetWebSocketFrame = new WebSocketFrameReader(memoryStream).ReadAsync(CancellationToken.None).Result;

            Assert.AreEqual(sourceWebSocketFrame.Fin, targetWebSocketFrame.Fin);
            Assert.AreEqual(sourceWebSocketFrame.Opcode, targetWebSocketFrame.Opcode);
            CollectionAssert.AreEqual(payloadBuffer, targetWebSocketFrame.Payload);
        }
    }
}
