using System;
using System.Collections.Generic;
using HTTPnet.Http;

namespace HTTPnet.Pipeline
{
    public class HttpRequestPipelineModuleContext
    {
        public HttpRequestPipelineModuleContext(HttpContext httpContext)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        }

        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
        public HttpContext HttpContext { get; }
        public bool BreakPipeline { get; set; }
    }
}
