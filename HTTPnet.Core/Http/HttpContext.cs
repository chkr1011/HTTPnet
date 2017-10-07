using System;
using System.Collections.Generic;
using HTTPnet.Core.Communication;
using HTTPnet.Core.Http.Raw;

namespace HTTPnet.Core.Http
{
    public class HttpContext
    {
        public HttpContext(RawHttpRequest request, RawHttpResponse response, ClientSession clientSession, HttpSessionHandler sessionHandler)
        {
            ClientSession = clientSession ?? throw new ArgumentNullException(nameof(clientSession));
            SessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public RawHttpRequest Request { get; }
        public RawHttpResponse Response { get; }

        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
        public HttpSessionHandler SessionHandler { get; }
        public ClientSession ClientSession { get; }
        public bool CloseConnection { get; set; }
    }
}