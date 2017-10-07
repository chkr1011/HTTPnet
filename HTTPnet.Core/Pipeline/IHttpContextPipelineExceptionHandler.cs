using System;
using System.Threading.Tasks;
using HTTPnet.Core.Http;

namespace HTTPnet.Core.Pipeline
{
    public interface IHttpContextPipelineExceptionHandler
    {
        Task HandleExceptionAsync(HttpContext httpContext, Exception exception);
    }
}
