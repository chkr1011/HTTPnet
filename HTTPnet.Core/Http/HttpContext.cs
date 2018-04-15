using System;
using System.Collections.Generic;
using HTTPnet.Communication;
using HTTPnet.Http.Raw;

namespace HTTPnet.Http
{
    public class HttpContext
    {
        public HttpContext(RawHttpRequest request, RawHttpResponse response, ClientSession clientSession)
        {
            ClientSession = clientSession ?? throw new ArgumentNullException(nameof(clientSession));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public RawHttpRequest Request { get; }
        public RawHttpResponse Response { get; }

        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public bool CompressResponseIfSupported { get; set; } = true;

        public ClientSession ClientSession { get; }
        public bool CloseConnection { get; set; }
    }
}