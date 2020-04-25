using System;
using System.Collections.Generic;
using System.IO;

namespace HTTPnet.Http.Raw
{
    public class RawHttpResponse
    {
        public Version Version { get; set; }
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Stream Body { get; set; }
    }
}