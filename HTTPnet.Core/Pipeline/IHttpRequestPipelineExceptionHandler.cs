using System;
using System.Threading.Tasks;
using HTTPnet.Http;

namespace HTTPnet.Pipeline
{
    public interface IHttpRequestPipelineExceptionHandler
    {
        Task HandleExceptionAsync(HttpContext httpContext, Exception exception);
    }
}
