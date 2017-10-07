using System.Collections.Generic;
using System.IO;

namespace HTTPnet.Core.Http.Raw
{
    public class RawHttpRequest
    {
        public string Method { get; set; }
        public string Uri { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Stream Body { get; set; }
    }
}