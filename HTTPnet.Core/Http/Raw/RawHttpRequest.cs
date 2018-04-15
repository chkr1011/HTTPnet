using System;
using System.Collections.Generic;
using System.IO;

namespace HTTPnet.Http.Raw
{
    public sealed class RawHttpRequest
    {
        public string Method { get; set; }
        public Uri Uri { get; set; }
        public Version Version { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Stream Body { get; set; }
    }
}