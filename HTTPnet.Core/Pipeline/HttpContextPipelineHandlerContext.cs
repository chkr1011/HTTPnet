using System;
using System.Collections.Generic;
using HTTPnet.Core.Http;

namespace HTTPnet.Core.Pipeline
{
    public class HttpContextPipelineHandlerContext
    {
        public HttpContextPipelineHandlerContext(HttpContext httpContext)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        }

        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
        public HttpContext HttpContext { get; }
        public bool BreakPipeline { get; set; }
    }
}
